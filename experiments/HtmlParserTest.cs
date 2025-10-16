using System;
using Platform.Examples.Html;
using Platform.Examples.Triggers;

namespace Experiments
{
    /// <summary>
    /// Test/experiment program for HTML parser with trigger system.
    /// </summary>
    class HtmlParserTest
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== HTML Parser with Triggers System Test ===\n");

            var parser = new HtmlParser<ulong>();

            // Test 1: Valid HTML
            Console.WriteLine("Test 1: Valid HTML");
            var html1 = "<html><head><title>Test</title></head><body><p>Hello World</p></body></html>";
            TestHtml(parser, html1);

            // Test 2: Invalid HTML - Mismatched tags
            Console.WriteLine("\nTest 2: Invalid HTML - Mismatched tags");
            var html2 = "<html><body><p>Test</div></body></html>";
            TestHtml(parser, html2);

            // Test 3: Invalid HTML - Unclosed tag
            Console.WriteLine("\nTest 3: Invalid HTML - Unclosed tag");
            var html3 = "<html><body><p>Test</body></html>";
            TestHtml(parser, html3);

            // Test 4: Self-closing tags
            Console.WriteLine("\nTest 4: Self-closing tags");
            var html4 = "<html><body><p>Test<br/>More text<img src='test.png'/></p></body></html>";
            TestHtml(parser, html4);

            // Test 5: Unexpected closing tag
            Console.WriteLine("\nTest 5: Unexpected closing tag");
            var html5 = "<html></div></html>";
            TestHtml(parser, html5);

            // Test 6: Valid nested structure
            Console.WriteLine("\nTest 6: Valid nested structure");
            var html6 = "<div><p><span>Text</span></p></div>";
            TestHtml(parser, html6);

            // Test 7: Empty input
            Console.WriteLine("\nTest 7: Empty input");
            TestHtml(parser, "");

            Console.WriteLine("\n=== All tests completed ===");
        }

        static void TestHtml(HtmlParser<ulong> parser, string html)
        {
            Console.WriteLine($"Input: {(string.IsNullOrEmpty(html) ? "(empty)" : html)}");

            var (isValid, elements, errors) = parser.Parse(html);

            Console.WriteLine($"Result: {(isValid ? "VALID" : "INVALID")}");

            if (elements.Count > 0)
            {
                Console.WriteLine("Elements:");
                foreach (var element in elements)
                {
                    Console.WriteLine($"  - {element}");
                }
            }

            if (errors.Count > 0)
            {
                Console.WriteLine("Errors:");
                foreach (var error in errors)
                {
                    Console.WriteLine($"  - {error}");
                }
            }
        }
    }
}
