using System;
using System.IO;
using Platform.CodeTransformations;

namespace Platform.CodeTransformations.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            var command = args[0].ToLower();

            try
            {
                switch (command)
                {
                    case "upgrade":
                    case "downgrade":
                        HandleTransform(args, command);
                        break;

                    case "init":
                        HandleInit(args);
                        break;

                    case "help":
                    case "--help":
                    case "-h":
                        ShowHelp();
                        break;

                    default:
                        Console.WriteLine($"Unknown command: {command}");
                        ShowHelp();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }

        static void HandleTransform(string[] args, string direction)
        {
            if (args.Length < 3)
            {
                Console.WriteLine($"Usage: code-transform {direction} <path> <target-version> [--config <config-file>]");
                return;
            }

            var path = args[1];
            var targetVersion = args[2];
            var configFile = ".code-transformations.json";

            for (int i = 3; i < args.Length; i++)
            {
                if (args[i] == "--config" && i + 1 < args.Length)
                {
                    configFile = args[i + 1];
                    i++;
                }
            }

            if (!File.Exists(configFile))
            {
                Console.WriteLine($"Configuration file not found: {configFile}");
                Console.WriteLine("Run 'code-transform init' to create a configuration file.");
                return;
            }

            var config = ConfigurationLoader.LoadConfig(configFile);
            var engine = ConfigurationLoader.CreateEngineFromConfig(config);

            var fromVersion = config.CurrentVersion;
            var toVersion = targetVersion;

            Console.WriteLine($"Transforming code from version {fromVersion} to {toVersion}...");

            if (Directory.Exists(path))
            {
                engine.TransformDirectory(path, fromVersion, toVersion);
                Console.WriteLine($"Successfully transformed all C# files in {path}");
            }
            else if (File.Exists(path))
            {
                var result = engine.TransformFile(path, fromVersion, toVersion);
                File.WriteAllText(path, result);
                Console.WriteLine($"Successfully transformed {path}");
            }
            else
            {
                Console.WriteLine($"Path not found: {path}");
                return;
            }

            config.CurrentVersion = toVersion;
            ConfigurationLoader.SaveConfig(config, configFile);
            Console.WriteLine($"Updated current version to {toVersion}");
        }

        static void HandleInit(string[] args)
        {
            var configFile = ".code-transformations.json";

            if (args.Length > 1)
            {
                configFile = args[1];
            }

            if (File.Exists(configFile))
            {
                Console.WriteLine($"Configuration file already exists: {configFile}");
                return;
            }

            var config = new TransformationConfig
            {
                LibraryName = "YourLibrary",
                CurrentVersion = "1.0.0",
                Versions = new System.Collections.Generic.List<string> { "1.0.0", "2.0.0" },
                Rules = new System.Collections.Generic.List<TransformationRuleConfig>
                {
                    new TransformationRuleConfig
                    {
                        Type = "MethodRename",
                        FromVersion = "1.0.0",
                        ToVersion = "2.0.0",
                        Parameters = new System.Collections.Generic.Dictionary<string, string>
                        {
                            { "OldMethodName", "OldMethod" },
                            { "NewMethodName", "NewMethod" }
                        }
                    }
                }
            };

            ConfigurationLoader.SaveConfig(config, configFile);
            Console.WriteLine($"Created configuration file: {configFile}");
            Console.WriteLine("Edit this file to define your transformation rules.");
        }

        static void ShowHelp()
        {
            Console.WriteLine(@"
Code Transformation Tool - Easy Breaking Changes Management

Usage:
  code-transform <command> [options]

Commands:
  init [config-file]                    Initialize a new configuration file
  upgrade <path> <version>              Upgrade code to a specific version
  downgrade <path> <version>            Downgrade code to a specific version
  help                                  Show this help message

Options:
  --config <file>                       Specify a custom configuration file

Examples:
  code-transform init
  code-transform upgrade ./src 2.0.0
  code-transform downgrade ./src 1.0.0 --config custom-config.json

Configuration File:
  The tool uses a .code-transformations.json file to define transformation rules.
  Use 'code-transform init' to create a template configuration file.
");
        }
    }
}
