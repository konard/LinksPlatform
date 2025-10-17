using System;
using System.Text;
using Platform.Examples;

namespace Platform.Experiments
{
    /// <summary>
    /// Example demonstrating the usage of FragmentStorage for storing variable-length fragments
    /// </summary>
    public class FragmentStorageExample
    {
        public static void Run()
        {
            const string storageFile = "fragments.dat";

            Console.WriteLine("=== Fragment Storage Example ===\n");

            // Create storage instance
            using (var storage = new FragmentStorage<ulong>(storageFile))
            {
                Console.WriteLine("1. Storing fragments...");

                // Store some text fragments with different lengths
                storage.StoreFragment(1, Encoding.UTF8.GetBytes("Hello, World!"));
                storage.StoreFragment(2, Encoding.UTF8.GetBytes("This is a longer fragment with more content."));
                storage.StoreFragment(3, Encoding.UTF8.GetBytes("Short"));
                storage.StoreFragment(4, Encoding.UTF8.GetBytes("Another variable-length fragment that can be stored in the same file."));

                Console.WriteLine("   Stored 4 fragments\n");

                // Retrieve and display fragments
                Console.WriteLine("2. Retrieving fragments...");

                for (ulong i = 1; i <= 4; i++)
                {
                    var content = storage.GetFragment(i);
                    var text = Encoding.UTF8.GetString(content);
                    var metadata = storage.GetFragmentMetadata(i);

                    Console.WriteLine($"   Fragment {i}:");
                    Console.WriteLine($"     Content: \"{text}\"");
                    Console.WriteLine($"     Offset: {metadata.FragmentContentOffset}");
                    Console.WriteLine($"     Length: {metadata.FragmentContentLength}\n");
                }

                // Display all fragments
                Console.WriteLine("3. All stored fragments:");
                foreach (var fragment in storage.GetAllFragments())
                {
                    Console.WriteLine($"   LinkAddress: {fragment.LinkAddress}, " +
                                    $"Offset: {fragment.FragmentContentOffset}, " +
                                    $"Length: {fragment.FragmentContentLength}");
                }

                Console.WriteLine("\n=== Example completed ===");
            }

            // Clean up example file
            if (System.IO.File.Exists(storageFile))
            {
                System.IO.File.Delete(storageFile);
            }
        }
    }
}
