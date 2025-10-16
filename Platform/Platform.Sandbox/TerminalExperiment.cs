using System;
using Platform.Data.Triplets;
using Platform.Examples;

namespace Platform.Sandbox
{
    public class TerminalExperiment : Terminal
    {
        public TerminalExperiment(string databaseFilePath = null) : base(databaseFilePath)
        {
        }

        protected override void OnStart()
        {
            if (!string.IsNullOrWhiteSpace(DatabaseFilePath))
            {
                Link.StartMemoryManager(DatabaseFilePath);
            }
            Link.CreatedEvent += OnLinkCreated;
        }

        protected override void OnStop()
        {
            Link.CreatedEvent -= OnLinkCreated;
            Link.StopMemoryManager();
        }

        public void Run()
        {
            Start();

            var x = Net.CreateThing();

            var number = NumberHelpers.FromNumber(4637694687);


            return;

            do
            {
                Console.Write("-> ");
                var readMessage = Console.ReadLine();

                if (string.Compare("exit", readMessage, ignoreCase: true) == 0)
                {
                    break;
                }

                Console.Write("<- ");
                Console.WriteLine(readMessage);
            }
            while (true);
        }

        private void OnLinkCreated(LinkDefinition createdLink)
        {
            Console.WriteLine($"Link created: {createdLink.Source.ToIndex()} {createdLink.Linker.ToIndex()} {createdLink.Target.ToIndex()}");
        }
    }
}
