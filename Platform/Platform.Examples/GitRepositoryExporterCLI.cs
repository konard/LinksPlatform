using System;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.IO;

namespace Platform.Examples
{
    public class GitRepositoryExporterCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: GitRepositoryExporter <links-db-path> <repository-link> <output-path> <author-name> [author-email]");
                return;
            }

            var dbPath = args[0];
            var repositoryLink = uint.Parse(args[1]);
            var outputPath = args[2];
            var authorName = args[3];
            var authorEmail = args.Length > 4 ? args[4] : "noreply@example.com";

            ConsoleHelpers.Debug("Exporting repository link {0} to: {1}", repositoryLink, outputPath);
            ConsoleHelpers.Debug("Links database: {0}", dbPath);

            using (var links = new UnitedMemoryLinks<uint>(dbPath))
            {
                var exporter = new GitRepositoryExporter<uint>(links);
                exporter.Export(repositoryLink, outputPath, authorName, authorEmail);
            }

            ConsoleHelpers.Debug("Done");
        }
    }
}
