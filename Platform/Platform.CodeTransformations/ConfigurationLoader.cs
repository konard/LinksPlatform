using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Platform.CodeTransformations.Rules;

namespace Platform.CodeTransformations
{
    /// <summary>
    /// Loads transformation configurations from JSON files.
    /// </summary>
    public class ConfigurationLoader
    {
        /// <summary>
        /// Loads a transformation configuration from a JSON file.
        /// </summary>
        /// <param name="configPath">Path to the configuration file.</param>
        /// <returns>The loaded transformation configuration.</returns>
        public static TransformationConfig LoadConfig(string configPath)
        {
            var json = File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<TransformationConfig>(json);
        }

        /// <summary>
        /// Saves a transformation configuration to a JSON file.
        /// </summary>
        /// <param name="config">The configuration to save.</param>
        /// <param name="configPath">Path to save the configuration file.</param>
        public static void SaveConfig(TransformationConfig config, string configPath)
        {
            var json = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configPath, json);
        }

        /// <summary>
        /// Creates a transformation engine from a configuration.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <returns>A configured transformation engine.</returns>
        public static TransformationEngine CreateEngineFromConfig(TransformationConfig config)
        {
            var engine = new TransformationEngine();

            foreach (var ruleConfig in config.Rules)
            {
                var rule = CreateRuleFromConfig(ruleConfig);
                if (rule != null)
                {
                    engine.RegisterRule(rule);
                }
            }

            return engine;
        }

        private static ITransformationRule CreateRuleFromConfig(TransformationRuleConfig ruleConfig)
        {
            switch (ruleConfig.Type)
            {
                case "MethodRename":
                    return new MethodRenameRule(
                        ruleConfig.Parameters.GetValueOrDefault("ClassName"),
                        ruleConfig.Parameters["OldMethodName"],
                        ruleConfig.Parameters["NewMethodName"],
                        ruleConfig.FromVersion,
                        ruleConfig.ToVersion
                    );

                case "NamespaceRename":
                    return new NamespaceRenameRule(
                        ruleConfig.Parameters["OldNamespace"],
                        ruleConfig.Parameters["NewNamespace"],
                        ruleConfig.FromVersion,
                        ruleConfig.ToVersion
                    );

                case "TypeRename":
                    return new TypeRenameRule(
                        ruleConfig.Parameters["OldTypeName"],
                        ruleConfig.Parameters["NewTypeName"],
                        ruleConfig.FromVersion,
                        ruleConfig.ToVersion
                    );

                default:
                    throw new NotSupportedException($"Transformation rule type '{ruleConfig.Type}' is not supported.");
            }
        }
    }

    internal static class DictionaryExtensions
    {
        public static TValue GetValueOrDefault<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        {
            return dictionary.TryGetValue(key, out var value) ? value : default;
        }
    }
}
