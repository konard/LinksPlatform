using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for the BasicWordsFinder
    /// </summary>
    public class BasicWordsFinderCLI : ICommandLineInterface
    {
        public void Run(params string[] args)
        {
            Console.WriteLine("=".PadRight(60, '='));
            Console.WriteLine("BASIC WORDS FINDER - Dictionary Analysis");
            Console.WriteLine("=".PadRight(60, '='));
            Console.WriteLine();

            var finder = new BasicWordsFinder();

            // Add example definitions
            // "be" is a classic basic word - it's hard to define without using itself
            finder.AddDefinition("be", "to exist or live");
            finder.AddDefinition("exist", "to be real or present");
            finder.AddDefinition("real", "actually existing or happening");
            finder.AddDefinition("present", "existing or happening now");
            finder.AddDefinition("happen", "to take place or occur");
            finder.AddDefinition("occur", "to happen or take place");
            finder.AddDefinition("place", "a particular position, point, or area");
            finder.AddDefinition("live", "to be alive or to exist");
            finder.AddDefinition("alive", "living or having life");
            finder.AddDefinition("life", "the condition that distinguishes living organisms from dead organisms");

            // Some compound words
            finder.AddDefinition("table", "a piece of furniture with a flat top and legs");
            finder.AddDefinition("furniture", "movable objects that make a room suitable for living or working");
            finder.AddDefinition("chair", "a piece of furniture for one person to sit on");
            finder.AddDefinition("sit", "to rest with the body supported by the buttocks");
            finder.AddDefinition("rest", "to cease work or movement in order to relax");

            // Classify words
            var (basicWords, compoundWords) = finder.ClassifyWords();
            var wordReferences = finder.GetWordReferences();

            Console.WriteLine($"Total words in dictionary: {finder.WordCount}");
            Console.WriteLine($"Basic words: {basicWords.Count}");
            Console.WriteLine($"Compound words: {compoundWords.Count}");
            Console.WriteLine();

            Console.WriteLine("-".PadRight(60, '-'));
            Console.WriteLine("BASIC WORDS (self-referential or in cyclic definitions):");
            Console.WriteLine("-".PadRight(60, '-'));

            foreach (var word in basicWords.OrderBy(w => w))
            {
                var refs = wordReferences.ContainsKey(word)
                    ? wordReferences[word].OrderBy(r => r).ToList()
                    : new List<string>();

                var selfRef = refs.Contains(word) ? "(SELF-REFERENTIAL)" : "";
                var refList = string.Join(", ", refs);

                Console.WriteLine($"  {word,-15} -> [{refList}] {selfRef}");
            }

            Console.WriteLine();
            Console.WriteLine("-".PadRight(60, '-'));
            Console.WriteLine("COMPOUND WORDS (can be defined without cycles):");
            Console.WriteLine("-".PadRight(60, '-'));

            foreach (var word in compoundWords.OrderBy(w => w))
            {
                var refs = wordReferences.ContainsKey(word)
                    ? wordReferences[word].OrderBy(r => r).ToList()
                    : new List<string>();

                var refList = string.Join(", ", refs);
                Console.WriteLine($"  {word,-15} -> [{refList}]");
            }

            Console.WriteLine();
        }
    }
}
