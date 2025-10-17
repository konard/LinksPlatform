using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace BreakingChangesMigrator
{
    /// <summary>
    /// Loads migration rules from JSON configuration files.
    /// </summary>
    public class MigrationRuleLoader
    {
        /// <summary>
        /// Loads migration rules from a JSON file.
        /// </summary>
        public static List<MigrationRule> LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Migration rules file not found: {filePath}");
            }

            var json = File.ReadAllText(filePath);
            var rules = JsonConvert.DeserializeObject<List<MigrationRule>>(json);

            if (rules == null)
            {
                throw new InvalidOperationException("Failed to deserialize migration rules.");
            }

            return rules;
        }

        /// <summary>
        /// Creates a sample migration rules file for demonstration.
        /// </summary>
        public static void CreateSampleRulesFile(string filePath)
        {
            var sampleRules = new List<MigrationRule>
            {
                new MigrationRule
                {
                    FromVersion = "0.5.0",
                    ToVersion = "0.6.0",
                    Description = "Method 'GetLink' renamed to 'GetLinkById'",
                    ChangeType = "MethodRename",
                    OldPattern = new PatternMatch
                    {
                        TypeName = "LinksOperator",
                        MemberName = "GetLink",
                        ParameterTypes = new List<string> { "ulong" }
                    },
                    NewPattern = new PatternMatch
                    {
                        MemberName = "GetLinkById"
                    }
                },
                new MigrationRule
                {
                    FromVersion = "0.5.0",
                    ToVersion = "0.6.0",
                    Description = "Type 'LinksConstants' renamed to 'LinksOperationConstants'",
                    ChangeType = "TypeRename",
                    OldPattern = new PatternMatch
                    {
                        TypeName = "LinksConstants"
                    },
                    NewPattern = new PatternMatch
                    {
                        TypeName = "LinksOperationConstants"
                    }
                }
            };

            var json = JsonConvert.SerializeObject(sampleRules, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }
    }
}
