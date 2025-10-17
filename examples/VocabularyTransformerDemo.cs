using System;
using Platform.Examples;

namespace VocabularyTransformerDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var cli = new VocabularyTransformerCLI();
            cli.Run(args);
        }
    }
}
