using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AICommander.Core;
using AICommander.Interfaces;
using SoldierSystem;
using Prompt;
using AICommander.Handlers;
using AICommander;
using UI;
using System.Linq;

// These wrapper classes are no longer needed in this script
// since we will parse the final command directly.
[Serializable]
public class AIResponseWrapper { public Choice[] choices; }
[System.Serializable]
public class Choice { public Message message; }
[System.Serializable]
public class Message { public string role; public string content; }

public class AICommandProcessor
{
    private IAICommanderService _aiService;
    private PromptProfileSO _promptProfile;
    private AIResponseLogger _logger;
    private string _lastGeneratedPrompt;
    private readonly Dictionary<string, ICommandHandler> _commandHandlers;
    private InfoCanvasManager _infoCanvasManager;

    public AICommandProcessor(IAICommanderService aiService, PromptProfileSO promptProfile)
    {
        _aiService = aiService;
        _promptProfile = promptProfile;
        _logger = new AIResponseLogger();
        _commandHandlers = new Dictionary<string, ICommandHandler>
        {
            { "move", new MoveCommandHandler() },
            { "hold", new MoveCommandHandler() } // Both move and hold are handled by MoveCommandHandler
        };
        _infoCanvasManager = GameObject.FindObjectOfType<InfoCanvasManager>();
    }

    public async Task<string> GetAICommandJsonAsync()
    {
        var soldierManager = SoldierManager.Instance;
        if (soldierManager == null)
        {
            Debug.LogError("[AICommandProcessor] SoldierManager instance not found");
            return null;
        }

        string battlefieldState = soldierManager.GetBattlefieldJson();
        if (string.IsNullOrEmpty(battlefieldState))
        {
            Debug.LogError("[AICommandProcessor] Battlefield state is empty or null");
            return null;
        }
        // Log the battlefield state for debugging
        Debug.Log($"[AICommandProcessor] Current Battlefield State: {battlefieldState}");
        string prompt = BuildPrompt(battlefieldState);
        _lastGeneratedPrompt = prompt;

        // Update UI with focused battlefield information
        if (_infoCanvasManager != null)
        {
            // Extract and format squad status
            string squadStatus = ExtractSquadStatus(battlefieldState);
            _infoCanvasManager.UpdateSquadStatus(squadStatus);

            // Extract and format soldier status
            string soldierStatus = ExtractSoldierStatus(battlefieldState);
            _infoCanvasManager.UpdateSoldierStatus(soldierStatus);
        }
        
        Debug.Log($"[AICommandProcessor] Sending prompt: {prompt}");
        
        // This now correctly returns the clean, inner JSON content
        string responseJson = await _aiService.SendCommand(prompt);
        
        // Update UI with response
        if (_infoCanvasManager != null)
        {
            _infoCanvasManager.UpdateResponseInfo(responseJson);
        }
        
        Debug.Log($"[AICommandProcessor] Received AI response: {responseJson}");
        _logger.LogResponse(prompt, responseJson);

        return responseJson;
    }

    private string ExtractSquadStatus(string battlefieldState)
    {
        try
        {
            // Parse the JSON to extract squad information
            var state = JsonUtility.FromJson<SoldierSystem.BattlefieldData>(battlefieldState);
            if (state == null) return "No battlefield data available";

            return $"Battlefield Timestamp: {state.timestamp:F1}\n" +
                   $"Active Soldiers: {state.soldiers.Count}";
        }
        catch (Exception e)
        {
            Debug.LogError($"Error extracting squad status: {e.Message}");
            return "Error reading battlefield status";
        }
    }

    private string ExtractSoldierStatus(string battlefieldState)
    {
        try
        {
            // Parse the JSON to extract Soldier information
            var state = JsonUtility.FromJson<SoldierSystem.BattlefieldData>(battlefieldState);
            if (state == null || state.soldiers == null || state.soldiers.Count == 0)
                return "No soldiers detected";

            var soldierInfo = state.soldiers.Select(e => 
                $"Soldier {e.runtimeId}: {e.name}\n" +
                $"Position: ({e.position.x:F1}, {e.position.y:F1}, {e.position.z:F1})\n" +
                $"Health: {e.health}/{e.maxHealth}\n" +
                $"State: {e.currentState}\n" +
                $"Zone: {e.zone}"
            ).ToArray();

            return string.Join("\n\n", soldierInfo);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error extracting Soldier status: {e.Message}");
            return "Error reading Soldier status";
        }
    }

    public string GetLastPrompt()
    {
        return _lastGeneratedPrompt;
    }
    
    private string BuildPrompt(string battlefieldState)
    {
        var promptData = new Dictionary<string, string> { { "squad_state", battlefieldState } };
        return PromptBuilder.BuildSquadSquadState(_promptProfile, promptData);
    }
    
    /// <summary>
    /// MODIFIED: This method now correctly parses the clean command JSON.
    /// </summary>
    public void ExecuteCommandFromJson(string jsonContent)
    {
        try
        {
            string commandContent = jsonContent;
            
            // Try to parse as wrapped response first
            var wrapper = JsonUtility.FromJson<AIResponseWrapper>(jsonContent);
            if (wrapper != null && wrapper.choices != null && wrapper.choices.Length > 0)
            {
                commandContent = wrapper.choices[0].message.content;
            }
            
            if (string.IsNullOrEmpty(commandContent))
            {
                Debug.LogWarning("Command content is empty.");
                return;
            }

            // Now parse the actual command
            var command = JsonUtility.FromJson<AICommanderResponse>(commandContent);
            
            if (command == null || string.IsNullOrEmpty(command.command_type))
            {
                Debug.LogWarning("Could not parse command from AI content.");
                return;
            }

            // Update UI with command information
            if (_infoCanvasManager != null)
            {
                _infoCanvasManager.UpdateCommandInfo(command.command_type, commandContent);
            }

            // Look up the correct handler in the dictionary.
            if (_commandHandlers.TryGetValue(command.command_type.ToLower(), out ICommandHandler handler))
            {
                Debug.Log($"Command '{command.command_type}' received. Delegating to {handler.GetType().Name}.");
                handler.Execute(command);
            }
            else
            {
                Debug.LogWarning($"Unknown command type received from AI: '{command.command_type}'. No handler registered.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing AI command JSON: {e.Message}\nJSON: {jsonContent}");
        }
    }
}