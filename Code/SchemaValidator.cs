using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json.Linq;

namespace Schema
{
    /// <summary>
    /// Validates a JSON data string against a parsed JSON Schema using Newtonsoft.Json.
    /// </summary>
    public static class SchemaValidator
    {
        /// <summary>
        /// Validates a JSON string against a raw JSON Schema content string.
        /// </summary>
        public static ValidationResult ValidateJson(string jsonData, string schemaContent)
        {
            var result = new ValidationResult();

            // 1. Parse the schema
            var schema = SchemaParser.ParseSchema(schemaContent);
            if (schema == null)
            {
                result.AddError("Schema parsing failed. Cannot perform validation.");
                return result;
            }

            // 2. Parse the JSON data
            JObject dataObject;
            try
            {
                dataObject = JObject.Parse(jsonData);
            }
            catch (Exception e)
            {
                result.AddError($"Invalid JSON data format: {e.Message}");
                return result;
            }

            // 3. Perform validation
            return ValidateWithSchemaRules(dataObject, schema);
        }

        /// <summary>
        /// Validates a JObject against the parsed schema rules.
        /// </summary>
        private static ValidationResult ValidateWithSchemaRules(JObject data, SchemaParser.ParsedSchema schema)
        {
            var result = new ValidationResult();

            // 1. Validate required fields
            foreach (string requiredField in schema.RequiredFields)
            {
                if (data[requiredField] == null)
                {
                    result.AddError($"Missing required field: '{requiredField}'");
                }
            }

            // 2. Validate each property defined in the schema
            foreach (var propertyPair in schema.Properties)
            {
                string propertyName = propertyPair.Key;
                SchemaParser.SchemaProperty propertyRule = propertyPair.Value;

                if (data.TryGetValue(propertyName, out JToken propertyValue))
                {
                    ValidateProperty(propertyValue, propertyName, propertyRule, result);
                }
            }

            // 3. Check for additional properties if not allowed
            if (!schema.AdditionalProperties)
            {
                foreach (var property in data.Properties())
                {
                    if (!schema.Properties.ContainsKey(property.Name))
                    {
                        result.AddWarning($"Detected an undefined property: '{property.Name}'");
                    }
                }
            }
            
            return result;
        }

        /// <summary>
        /// Validates a single property's value against its schema rule.
        /// </summary>
        private static void ValidateProperty(JToken value, string name, SchemaParser.SchemaProperty rule, ValidationResult result)
        {
            // Type validation
            string expectedType = rule.Type?.ToLower();
            if (string.IsNullOrEmpty(expectedType)) return; // No type to validate against

            JTokenType actualType = value.Type;

            bool typeMismatch = false;
            switch (expectedType)
            {
                case "integer":
                    if (actualType != JTokenType.Integer) typeMismatch = true;
                    else
                    {
                        int intValue = value.Value<int>();
                        if (rule.Minimum.HasValue && intValue < rule.Minimum.Value)
                        {
                            result.AddError($"Value for '{name}' ({intValue}) is less than the minimum of {rule.Minimum.Value}.");
                        }
                        if (rule.Maximum.HasValue && intValue > rule.Maximum.Value)
                        {
                            result.AddError($"Value for '{name}' ({intValue}) is greater than the maximum of {rule.Maximum.Value}.");
                        }
                    }
                    break;
                case "string":
                    if (actualType != JTokenType.String) typeMismatch = true;
                    else
                    {
                        string stringValue = value.Value<string>();
                        if (rule.EnumValues.Any() && !rule.EnumValues.Contains(stringValue))
                        {
                            result.AddError($"Value '{stringValue}' for '{name}' is not in the allowed list: [{string.Join(", ", rule.EnumValues)}]");
                        }
                    }
                    break;
                case "number":
                    if (actualType != JTokenType.Float && actualType != JTokenType.Integer) typeMismatch = true;
                    break;
                case "boolean":
                    if (actualType != JTokenType.Boolean) typeMismatch = true;
                    break;
            }

            if (typeMismatch)
            {
                result.AddError($"Invalid type for property '{name}'. Expected '{expectedType}' but got '{actualType.ToString().ToLower()}'.");
            }
        }
        
        // This is a simplified version of your legacy method, kept for reference but not used in the primary validation flow.
        public static ValidationResult ValidateRequiredFields(string jsonString, params string[] requiredFields)
        {
            var result = new ValidationResult();
            try
            {
                var data = JObject.Parse(jsonString);
                foreach(var field in requiredFields)
                {
                    if(data[field] == null)
                    {
                        result.AddError($"Missing required field: {field}");
                    }
                }
            }
            catch(Exception e)
            {
                result.AddError($"Invalid JSON: {e.Message}");
            }
            return result;
        }
    }

    /// <summary>
    /// Represents the result of a validation.
    /// </summary>
    [System.Serializable]
    public class ValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();

        public void AddError(string error) => Errors.Add(error);
        public void AddWarning(string warning) => Warnings.Add(warning);
        public string GetErrorsString() => string.Join("; ", Errors);
        public string GetWarningsString() => string.Join("; ", Warnings);
        
        public void LogToConsole(string context = "")
        {
            string prefix = string.IsNullOrEmpty(context) ? "" : $"[{context}] ";
            if (IsValid)
            {
                Debug.Log($"{prefix}✅ Validation Passed");
                if (Warnings.Count > 0) Debug.LogWarning($"{prefix}⚠️ Warnings: {GetWarningsString()}");
            }
            else
            {
                Debug.LogError($"{prefix}❌ Validation Failed: {GetErrorsString()}");
            }
        }
    }
}