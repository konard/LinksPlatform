using System;

namespace Platform.Experiments.CodeGraphVCS
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Code Graph VCS - Proof of Concept");
            Console.WriteLine("==================================");
            Console.WriteLine();

            CodeGraphVCSExample.Run();

            Console.WriteLine();
            Console.WriteLine("Example completed successfully!");
        }
    }
}
