using System;
using System.Collections.Generic;
using Platform.Examples;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Experiments
{
    /// <summary>
    /// Test and demonstration of the SequenceAutoCompleter functionality.
    /// This experiment creates a simple in-memory doublets storage, stores some sequences,
    /// and tests the autocomplete algorithm.
    /// </summary>
    class AutoCompleteTest
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Sequence Auto-Complete Test ===\n");

            // Create in-memory doublets storage
            using (var links = new UnitedMemoryLinks<uint>())
            {
                var autoCompleter = new SequenceAutoCompleter<uint>(links);

                Console.WriteLine("Creating test sequences...\n");

                // Create some character/element links (points)
                var h = links.CreatePoint();
                var e = links.CreatePoint();
                var l = links.CreatePoint();
                var o = links.CreatePoint();
                var w = links.CreatePoint();
                var r = links.CreatePoint();
                var d = links.CreatePoint();

                Console.WriteLine($"Created elements: h={h}, e={e}, l={l}, o={o}, w={w}, r={r}, d={d}\n");

                // Create sequences:
                // "hello" -> h-e-l-l-o
                // "help" -> h-e-l-p (where p would be different)
                // "world" -> w-o-r-l-d

                // For simplicity, create sequences as nested doublets
                // Sequence structure: (element1, (element2, (element3, ...)))

                // "he" = h->e
                var he = links.Create(h, e);

                // "hel" = h->(e->l)
                var el = links.Create(e, l);
                var hel = links.Create(h, el);

                // "hell" = h->(e->(l->l))
                var ll = links.Create(l, l);
                var ell = links.Create(e, ll);
                var hell = links.Create(h, ell);

                // "hello" = h->(e->(l->(l->o)))
                var lo = links.Create(l, o);
                var llo = links.Create(l, lo);
                var ello = links.Create(e, llo);
                var hello = links.Create(h, ello);

                Console.WriteLine($"Created sequences:");
                Console.WriteLine($"  'he' = {he}");
                Console.WriteLine($"  'hel' = {hel}");
                Console.WriteLine($"  'hell' = {hell}");
                Console.WriteLine($"  'hello' = {hello}\n");

                // Test 1: Find completions for prefix [h]
                Console.WriteLine("Test 1: Finding completions for prefix [h]...");
                var prefix1 = new List<uint> { h };
                var completions1 = autoCompleter.FindCompletions(prefix1);
                Console.WriteLine($"Found {completions1.Count} completions:");
                foreach (var completion in completions1)
                {
                    Console.WriteLine($"  Link {completion}");
                }
                Console.WriteLine();

                // Test 2: Find completions for prefix [h, e]
                Console.WriteLine("Test 2: Finding completions for prefix [h, e]...");
                var prefix2 = new List<uint> { h, e };
                var completions2 = autoCompleter.FindCompletions(prefix2);
                Console.WriteLine($"Found {completions2.Count} completions:");
                foreach (var completion in completions2)
                {
                    Console.WriteLine($"  Link {completion}");
                }
                Console.WriteLine();

                // Test 3: Find next possible elements after prefix [h, e, l]
                Console.WriteLine("Test 3: Finding next elements after prefix [h, e, l]...");
                var prefix3 = new List<uint> { h, e, l };
                var nextElements = autoCompleter.FindNextElements(prefix3);
                Console.WriteLine($"Found {nextElements.Count} possible next elements:");
                foreach (var element in nextElements)
                {
                    Console.WriteLine($"  Element {element}" + (element == l ? " (l)" : element == o ? " (o)" : ""));
                }
                Console.WriteLine();

                Console.WriteLine("=== Test completed ===");
            }
        }
    }
}
