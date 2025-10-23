using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Schema
{
    /// <summary>
    /// Parses a JSON Schema string into a strongly-typed object model using Newtonsoft.Json.
    /// This is a robust and reliable replacement for the previous regex-based parser.
    /// </summary>
    public static class SchemaParser
    {
        [System.Serializable]
        public class ParsedSchema
        {
            [JsonProperty("title")]
            public string Title { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("properties")]
            public Dictionary<string, SchemaProperty> Properties { get; set; } = new Dictionary<string, SchemaProperty>();

            [JsonProperty("required")]
            public List<string> RequiredFields { get; set; } = new List<string>();

            [JsonProperty("additionalProperties")]
            public bool AdditionalProperties { get; set; } = true;
        }

        [System.Serializable]
        public class SchemaProperty
        {
            [JsonProperty("description")]
            public string Description { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("minimum")]
            public int? Minimum { get; set; }

            [JsonProperty("maximum")]
            public int? Maximum { get; set; }

            [JsonProperty("enum")]
            public List<string> EnumValues { get; set; } = new List<string>();
        }

        /// <summary>
        /// Parses a JSON Schema string.
        /// </summary>
        /// <param name="schemaJson">The JSON Schema content.</param>
        /// <returns>A ParsedSchema object, or null if parsing fails.</returns>
        public static ParsedSchema ParseSchema(string schemaJson)
        {
            if (string.IsNullOrEmpty(schemaJson))
            {
                Debug.LogError("Schema JSON content is null or empty.");
                return null;
            }

            try
            {
                var parsedSchema = JsonConvert.DeserializeObject<ParsedSchema>(schemaJson);
                Debug.Log($"Successfully parsed schema: '{parsedSchema.Title}' with {parsedSchema.Properties.Count} properties.");
                return parsedSchema;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to parse schema with Newtonsoft.Json: {e.Message}");
                return null;
            }
        }
    }
}