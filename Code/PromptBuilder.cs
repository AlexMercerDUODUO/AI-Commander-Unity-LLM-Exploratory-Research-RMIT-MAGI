using Schema;
using System.Linq;
using System.Text;
using ScriptableObjects;
using System.Collections.Generic;

namespace Prompt
{
    public static class PromptBuilder
    {
        public static string Build(PromptProfileSO profile, string squadStateJson, string battlefieldInfoJson = null)
        {
            if (profile == null) return string.Empty;

            // 1. Start with the main template
            StringBuilder promptBuilder = new StringBuilder(profile.promptTemplate);

            // 2. Populate the rules section
            StringBuilder rulesBuilder = new StringBuilder(profile.rulesSection);
            rulesBuilder.Replace("{min_id}", "1");
            rulesBuilder.Replace("{max_id}", "8");
            rulesBuilder.Replace("{name_options}", "move, hold");
            promptBuilder.Replace("{rules}", rulesBuilder.ToString());

            // 3. Add the battlefield information if available
            if (!string.IsNullOrEmpty(battlefieldInfoJson))
            {
                promptBuilder.Replace("{battlefield_info}", battlefieldInfoJson);
            }
            else
            {
                promptBuilder.Replace("{battlefield_info}", "No battlefield information available");
            }

            // 4. Add the squad state data with clear labeling
            promptBuilder.Replace("{enemy_data}", $"--- ENEMY UNITS INFORMATION ---\n{squadStateJson}");

            // 5. Append the schema for clear instructions
            promptBuilder.Append("\n\n--- REQUIRED OUTPUT FORMAT ---\n");
            promptBuilder.Append("You must strictly return a JSON object that adheres to the following schema. Do not include any other text or markdown formatting.\n\n");
            promptBuilder.Append(profile.schemaJson);

            return promptBuilder.ToString();
        }
        
        public static string BuildSquadSquadState(PromptProfileSO profile, Dictionary<string, string> data)
        {
            if (profile == null) return string.Empty;

            // 1. Start with the main template
            StringBuilder promptBuilder = new StringBuilder(profile.promptTemplate);

            // 2. Populate the rules section first
            StringBuilder rulesBuilder = new StringBuilder(profile.rulesSection);
            rulesBuilder.Replace("{min_id}", "1");
            rulesBuilder.Replace("{max_id}", "8");
            rulesBuilder.Replace("{name_options}", "move, hold");
            promptBuilder.Replace("{rules}", rulesBuilder.ToString());

            // 3. Replace all data placeholders with clear labeling
            foreach (var kvp in data)
            {
                string labeledValue = kvp.Key switch
                {
                    "squad_state" => $"--- FRIENDLY UNITS INFORMATION ---\n{kvp.Value}",
                    "enemy_data" => $"--- ENEMY UNITS INFORMATION ---\n{kvp.Value}",
                    "battlefield_info" => $"--- BATTLEFIELD INFORMATION ---\n{kvp.Value}",
                    _ => kvp.Value
                };

                promptBuilder.Replace($"{{{kvp.Key}}}", labeledValue);
            }

            // 4. Append the schema for clear instructions
            promptBuilder.Append("\n\n--- REQUIRED OUTPUT FORMAT ---\n");
            promptBuilder.Append("You must strictly return a JSON object that adheres to the following schema. Do not include any other text or markdown formatting.\n\n");
            promptBuilder.Append(profile.schemaJson);

            return promptBuilder.ToString();
        }
    }
}