using System;
using System.Threading;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Sequences.Frequencies.Cache;
using Platform.Data.Doublets.Sequences.Frequencies.Counters;
using Platform.IO;

namespace Platform.Examples
{
    public class GitRepositoryImporterCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage: GitRepositoryImporter <repository-path> <links-db-path>");
                return;
            }

            var repositoryPath = args[0];
            var dbPath = args[1];

            ConsoleHelpers.Debug("Importing git repository: {0}", repositoryPath);
            ConsoleHelpers.Debug("Links database: {0}", dbPath);

            using (var links = new UnitedMemoryLinks<uint>(dbPath))
            {
                var frequencyCounter = new TotalSequenceSymbolFrequencyCounter<uint>(links);
                var cache = new LinkFrequenciesCache<uint>(links, frequencyCounter);
                var storage = new LinksCodeStorage<uint>(links, false, cache);
                var importer = new GitRepositoryImporter<uint>(storage);

                var cancellationTokenSource = new CancellationTokenSource();
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    cancellationTokenSource.Cancel();
                };

                var task = importer.Import(repositoryPath, cancellationTokenSource.Token);
                task.Wait();
            }

            ConsoleHelpers.Debug("Done");
        }
    }
}
