using UnityEngine;
using System.Collections.Generic;

namespace Prompt
{
    [CreateAssetMenu(fileName = "PromptProfile_WithHold", menuName = "AI Commander/Prompt Profile (With Hold)", order = 2)]
    public class PromptProfileSO : ScriptableObject
    {
        [Header("Prompt Identification")]
        public string profileName = "DefaultWithHold";

        [Header("Prompt Content")]
        [TextArea(10, 15)]
        [Tooltip("The main template for the prompt. Use placeholders like {rules}, {battlefield_info}, {squad_state}, and {enemy_data}.")]
        public string promptTemplate =
            "You are the Commander of an autonomous infantry squad. Your mission is to defend a critical objective from an enemy player by issuing tactical commands.\n\n" +
            "{battlefield_info}\n\n" +
            "--- MISSION BRIEFING ---\n{rules}\n\n" +
            "--- FRIENDLY SQUAD STATUS ---\n{squad_state}\n\n" +
            "--- KNOWN ENEMY STATUS ---\n{enemy_data}";

        [Header("Dynamic Rules")]
        [TextArea(5, 10)]
        [Tooltip("Rules section that will be populated with dynamic values. Use placeholders like {min_id}, {max_id}, and {name_options}.")]
        public string rulesSection =
            "- Your primary objective is to prevent the enemy from capturing the Star located in Zone 3.\n" +
            "- For this turn, you must issue a single command: either 'move' to an adjacent zone or 'hold' position.\n" +
            "- Issuing a 'hold' command is a valid and strategic option if a unit is already in a strong defensive position or if moving would expose a flank.\n" +
            "- Enemy data may not always be available. If enemy positions are unknown, make a strategic decision based on defending the primary objective (Zone 3).\n" +
            "- Movement is restricted to one adjacent square at a time (horizontally or vertically). Diagonal moves are not permitted.\n" +
            "- Your soldiers will automatically engage the enemy on sight.\n" +
            "- Unit IDs for commands must be between {min_id} and {max_id}.\n" +
            "- Available commands: {name_options}.";

        [Header("Schema Definition")]
        [TextArea(15, 25)]
        [Tooltip("The JSON Schema that this prompt must conform to. This ensures the prompt and validation are always paired.")]
        public string schemaJson = @"{
    ""$schema"": ""http://json-schema.org/draft-07/schema#"",
    ""title"": ""Tactical Command"",
    ""description"": ""Defines the structure for commanding an NPC soldier."",
    ""type"": ""object"",
    ""properties"": {
        ""command_type"": {
            ""description"": ""The type of command to issue."",
            ""type"": ""string"",
            ""enum"": [""move"", ""hold""]
        },
        ""unit_id"": {
            ""description"": ""The unique identifier of the NPC soldier to command (1-8)."",
            ""type"": ""integer"",
            ""minimum"": 1,
            ""maximum"": 8
        }
    },
    ""required"": [
        ""command_type"",
        ""unit_id""
    ],
    ""if"": {
        ""properties"": { ""command_type"": { ""const"": ""move"" } }
    },
    ""then"": {
        ""properties"": {
            ""target_zone"": {
                ""description"": ""The zone number (1-9) the NPC should move to. Required only for 'move' command."",
                ""type"": ""integer"",
                ""minimum"": 1,
                ""maximum"": 9
            }
        },
        ""required"": [""target_zone""]
    },
    ""additionalProperties"": false
}";
    }
}