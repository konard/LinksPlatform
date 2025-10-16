using System;
using Net;

namespace Experiments
{
	/// <summary>
	/// Simple test to demonstrate the Each method functionality for Triple Links
	/// </summary>
	public class EachMethodTest
	{
		public static void Main()
		{
			Console.WriteLine("=== Each Method Test for Triple Links ===\n");

			// Create some test links
			var link1 = Link.CreateLinkLinkingItself();
			link1.SetName("SelfLink");

			var link2 = Link.CreateCycleSelflink(Net.Net.IsA);
			link2.SetName("CycleSelfLink");

			var link3 = Link.Create(Net.Net.Thing, Net.Net.IsA, Net.Net.Link);
			link3.SetName("ThingIsALink");

			Console.WriteLine("Created 3 test links\n");

			// Test 1: Iterate through all links
			Console.WriteLine("Test 1: Iterate through ALL links");
			int count = 0;
			Link.Each(link =>
			{
				count++;
				string name;
				if (link.TryGetName(out name))
				{
					Console.WriteLine($"  Link #{count}: {name}");
				}
				else
				{
					Console.WriteLine($"  Link #{count}: (unnamed)");
				}
			});
			Console.WriteLine($"Total links found: {count}\n");

			// Test 2: Find links with specific source (Net.Thing)
			Console.WriteLine("Test 2: Find links where Source = Net.Thing");
			int specificCount = 0;
			Link.Each(Net.Net.Thing, null, null, link =>
			{
				specificCount++;
				string name;
				if (link.TryGetName(out name))
				{
					Console.WriteLine($"  Found: {name}");
				}
				else
				{
					Console.WriteLine($"  Found: (unnamed)");
				}
				return true; // Continue iteration
			});
			Console.WriteLine($"Links with Source=Net.Thing: {specificCount}\n");

			// Test 3: Find links with specific linker (Net.IsA)
			Console.WriteLine("Test 3: Find links where Linker = Net.IsA");
			specificCount = 0;
			Link.Each(null, Net.Net.IsA, null, link =>
			{
				specificCount++;
				string name;
				if (link.TryGetName(out name))
				{
					Console.WriteLine($"  Found: {name}");
				}
				else
				{
					Console.WriteLine($"  Found: (unnamed)");
				}
				return true;
			});
			Console.WriteLine($"Links with Linker=Net.IsA: {specificCount}\n");

			// Test 4: Early termination by returning false
			Console.WriteLine("Test 4: Early termination (stop after 3 links)");
			int limitCount = 0;
			Link.Each(null, null, null, link =>
			{
				limitCount++;
				Console.WriteLine($"  Processing link #{limitCount}");
				return limitCount < 3; // Stop after 3 links
			});
			Console.WriteLine($"Stopped after {limitCount} links\n");

			Console.WriteLine("=== All tests completed successfully ===");
		}
	}
}
