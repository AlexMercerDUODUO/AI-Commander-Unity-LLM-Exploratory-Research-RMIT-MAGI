using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Schema
{
    /// <summary>
    /// JSON Schema管理器，负责加载和管理Schema文件
    /// </summary>
    public static class SchemaManager
    {
        private static Dictionary<string, string> _loadedSchemas = new Dictionary<string, string>();
        private static string _schemasPath;

        static SchemaManager()
        {
            _schemasPath = Path.Combine(Application.streamingAssetsPath, "Schemas");
        }

        /// <summary>
        /// 加载指定的Schema文件
        /// </summary>
        /// <param name="schemaName">Schema文件名（不包含扩展名）</param>
        /// <returns>Schema的JSON字符串</returns>
        public static string LoadSchema(string schemaName)
        {
            // 如果已经加载过，直接返回缓存的内容
            if (_loadedSchemas.ContainsKey(schemaName))
            {
                return _loadedSchemas[schemaName];
            }

            string schemaFilePath = Path.Combine(_schemasPath, $"{schemaName}.schema.json");
            
            if (!File.Exists(schemaFilePath))
            {
                Debug.LogError($"Schema文件不存在: {schemaFilePath}");
                return null;
            }

            try
            {
                string schemaContent = File.ReadAllText(schemaFilePath);
                _loadedSchemas[schemaName] = schemaContent;
                Debug.Log($"成功加载Schema: {schemaName}");
                return schemaContent;
            }
            catch (Exception e)
            {
                Debug.LogError($"加载Schema文件失败 {schemaName}: {e.Message}");
                return null;
            }
        }

        /// <summary>
        /// 验证Schema和提示词的一致性
        /// </summary>
        public static bool ValidateSchemaPromptConsistency(string schemaName, string prompt)
        {
            string schemaJson = LoadSchema(schemaName);
            if (string.IsNullOrEmpty(schemaJson))
            {
                Debug.LogError($"无法加载Schema: {schemaName}");
                return false;
            }

            var parsedSchema = SchemaParser.ParseSchema(schemaJson);
            if (parsedSchema == null || parsedSchema.Properties.Count == 0)
            {
                Debug.LogError($"Schema解析失败: {schemaName}");
                return false;
            }

            bool isConsistent = true;
            foreach (var property in parsedSchema.Properties)
            {
                // 检查枚举值是否在提示词中
                if (property.Value.EnumValues.Count > 0)
                {
                    foreach (string enumValue in property.Value.EnumValues)
                    {
                        if (!prompt.Contains(enumValue))
                        {
                            Debug.LogWarning($"Schema中的枚举值 '{enumValue}' 未在提示词中找到");
                            isConsistent = false;
                        }
                    }
                }

                // 检查数值范围是否在提示词中
                if (property.Value.Minimum.HasValue || property.Value.Maximum.HasValue)
                {
                    string rangeText = $"介于{property.Value.Minimum}到{property.Value.Maximum}之间";
                    if (!prompt.Contains(rangeText))
                    {
                        Debug.LogWarning($"Schema中的数值范围未在提示词中找到");
                        isConsistent = false;
                    }
                }
            }

            return isConsistent;
        }

        /// <summary>
        /// 获取Schema的描述信息
        /// </summary>
        /// <param name="schemaName">Schema名称</param>
        /// <returns>Schema描述</returns>
        public static string GetSchemaDescription(string schemaName)
        {
            string schemaJson = LoadSchema(schemaName);
            if (string.IsNullOrEmpty(schemaJson))
            {
                return "Schema未找到";
            }

            var parsedSchema = SchemaParser.ParseSchema(schemaJson);
            return parsedSchema?.Description ?? "无描述";
        }

        /// <summary>
        /// 获取所有可用的Schema列表
        /// </summary>
        /// <returns>Schema名称列表</returns>
        public static List<string> GetAvailableSchemas()
        {
            List<string> schemas = new List<string>();
            
            if (!Directory.Exists(_schemasPath))
            {
                Debug.LogWarning($"Schemas文件夹不存在: {_schemasPath}");
                return schemas;
            }

            try
            {
                string[] schemaFiles = Directory.GetFiles(_schemasPath, "*.schema.json");
                foreach (string filePath in schemaFiles)
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    // 移除.schema部分
                    if (fileName.EndsWith(".schema"))
                    {
                        fileName = fileName.Substring(0, fileName.Length - 7);
                    }
                    schemas.Add(fileName);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"获取Schema列表失败: {e.Message}");
            }

            return schemas;
        }

        /// <summary>
        /// 清除Schema缓存
        /// </summary>
        public static void ClearCache()
        {
            _loadedSchemas.Clear();
            Debug.Log("Schema缓存已清除");
        }
    }
} 