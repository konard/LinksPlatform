using System;
using System.IO;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Sequences.Frequencies.Cache;
using Platform.Data.Doublets.Sequences.Frequencies.Counters;
using Platform.Examples;

namespace Platform.Sandbox
{
    public static class CodeStorageTest
    {
        public static void Run()
        {
            var tempDb = Path.GetTempFileName();
            try
            {
                using (var links = new UnitedMemoryLinks<uint>(tempDb))
                {
                    var frequencyCounter = new TotalSequenceSymbolFrequencyCounter<uint>(links);
                    var cache = new LinkFrequenciesCache<uint>(links, frequencyCounter);
                    var storage = new LinksCodeStorage<uint>(links, false, cache);

                    // Create a repository
                    var repository = storage.CreateRepository("TestRepository");
                    Console.WriteLine($"Created repository: {repository}");

                    // Create a file
                    var file = storage.CreateFile("src/Program.cs", "using System;\n\nnamespace Test\n{\n    class Program\n    {\n        static void Main()\n        {\n            Console.WriteLine(\"Hello World\");\n        }\n    }\n}");
                    Console.WriteLine($"Created file: {file}");

                    // Create a commit
                    var commit = storage.CreateCommit("Initial commit", "Test Author <test@example.com>", default);
                    Console.WriteLine($"Created commit: {commit}");

                    // Attach file to commit
                    storage.AttachFileToCommit(file, commit);

                    // Attach commit to repository
                    storage.AttachCommitToRepository(commit, repository);

                    Console.WriteLine("Code storage test completed successfully!");
                }
            }
            finally
            {
                if (File.Exists(tempDb))
                {
                    File.Delete(tempDb);
                }
            }
        }
    }
}
