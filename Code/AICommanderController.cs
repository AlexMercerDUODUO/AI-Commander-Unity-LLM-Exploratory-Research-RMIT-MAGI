using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using UnityEngine.Networking;
using System;
using AICommander;
using System.Threading.Tasks;
using AICommander.Interfaces;
using AICommander.Services;
using Prompt;
using AICommander.Core;
using UI;
using UnityEngine.Serialization;

public class AICommanderController : MonoBehaviour
{
    [Header("API Settings")]
    [SerializeField] private AICommanderConfig config;
    
    [Header("Prompt & Validation Settings")]
    [SerializeField] private PromptProfileSO activePromptProfile;
    
    [Header("Timing Settings")]
    [SerializeField] private float updateInterval = 5f;
    
    [Header("Debug Settings")]
    [SerializeField] private bool useLocalTest = true;
    [Tooltip("If Use Local Test is true, this JSON will be used instead of calling the AI API.")]
    [TextArea(3, 10)]
    [SerializeField] private string localTestJson = "{\"choices\":[{\"message\":{\"role\":\"assistant\",\"content\":\"{\\\"command_type\\\":\\\"move\\\",\\\"target_id\\\":1,\\\"parameters\\\":{\\\"position\\\":{\\\"x\\\":10,\\\"y\\\":0,\\\"z\\\":5},\\\"speed\\\":3.0}}\"}}]}";
    
    private IAICommanderService _aiService;
    private AICommandProcessor _commandProcessor;
    private float _timer = 0f;
    private bool _isProcessing = false;
    
    public bool debugMode = true;
    
    [Header("Logging Settings")] // You can add a header for clarity
    public bool doLogging = true; // This toggle now controls logging
    
    //UI to show the status of the AI Commander
    [Header("UI Elements")]
    [SerializeField] private bool showAPISendReceive = true;
    [SerializeField] private bool showCommandExecution = true;
    [SerializeField] private bool showSoldierNextMove = true;
    
    private void Start()
    {
        InitializeServices();
    }

    private void InitializeServices()
    {
        if (activePromptProfile == null)
        {
            Debug.LogError("FATAL: Active Prompt Profile not assigned!", this);
            this.enabled = false;
            return;
        }

        _aiService = new DeepSeekAIService();
        _aiService.SetApiCredentials(GetApiKey(), GetApiUrl());
        
        _commandProcessor = new AICommandProcessor(_aiService, activePromptProfile);
    }
    private async void Update()
    {
        if (_isProcessing) return;

        _timer += Time.deltaTime;
        if (_timer >= updateInterval)
        {
            _timer = 0f;
            await ProcessAICommand();
        }
    }


    private string GetApiKey()
    {
        string envKey = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY");
        return !string.IsNullOrEmpty(envKey) ? envKey : (config?.apiKey ?? string.Empty);
    }

    private string GetApiUrl()
    {
        string envUrl = Environment.GetEnvironmentVariable("DEEPSEEK_API_URL");
        return !string.IsNullOrEmpty(envUrl) ? envUrl : (config?.apiUrl ?? string.Empty);
    }

    private async Task ProcessAICommand()
    {
        _isProcessing = true;
        try
        {
            string commandJson;
            if (useLocalTest)
            {
                Debug.Log($"Using local test JSON: {localTestJson}", this);
                commandJson = localTestJson;
            }
            else
            {
                //Debug.Log("Fetching AI command JSON from the service...", this);
                
                commandJson = await _commandProcessor.GetAICommandJsonAsync();
                
                if (string.IsNullOrEmpty(commandJson))
                {
                    Debug.LogWarning("Received empty command JSON from AI service.", this);
                    return;
                }
                
                if (debugMode) Debug.Log($"Received command JSON: {commandJson}", this);
            }

            if (!string.IsNullOrEmpty(commandJson))
            {
                _commandProcessor.ExecuteCommandFromJson(commandJson);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[AICommanderController] Error during command processing: {e.Message}", this);
        }
        finally
        {
            _isProcessing = false;
        }
    }
}