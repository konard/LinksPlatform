using System;
using System.Collections.Generic;
using System.Text;

namespace Net
{
	/// <summary>
	/// Example implementation demonstrating LinkWalker capabilities.
	/// </summary>
	public static class LinkWalkerExample
	{
		/// <summary>
		/// Simple visitor that prints link information.
		/// </summary>
		private class PrintVisitor : ILinkVisitor
		{
			private int _depth = 0;
			private readonly StringBuilder _output;

			public PrintVisitor()
			{
				_output = new StringBuilder();
			}

			public bool Visit(Link link)
			{
				var indent = new string(' ', _depth * 2);
				_output.AppendLine($"{indent}Visiting: {link}");
				_depth++;
				return true; // Continue traversal
			}

			public void Leave(Link link)
			{
				_depth--;
				var indent = new string(' ', _depth * 2);
				_output.AppendLine($"{indent}Leaving: {link}");
			}

			public override string ToString()
			{
				return _output.ToString();
			}
		}

		/// <summary>
		/// Visitor that collects links into a list.
		/// </summary>
		private class CollectorVisitor : ILinkVisitor
		{
			private readonly List<Link> _collected = new List<Link>();
			private readonly Predicate<Link> _filter;

			public CollectorVisitor(Predicate<Link> filter = null)
			{
				_filter = filter;
			}

			public IEnumerable<Link> Collected => _collected;

			public bool Visit(Link link)
			{
				if (_filter == null || _filter(link))
				{
					_collected.Add(link);
				}
				return true;
			}

			public void Leave(Link link)
			{
				// Nothing to do on leave
			}
		}

		/// <summary>
		/// Demonstrates basic navigation with LinkWalker.
		/// </summary>
		public static void DemonstrateBasicNavigation()
		{
			Console.WriteLine("=== Basic Navigation Example ===");

			// Create a simple link structure
			var sourceLink = Link.CreateLinkLinkingItself();
			var linkerLink = Link.CreateLinkLinkingItself();
			var targetLink = Link.CreateLinkLinkingItself();
			var mainLink = Link.Create(sourceLink, linkerLink, targetLink);

			// Create walker
			var walker = new LinkWalker(mainLink, PathTrackingMode.Absolute);

			Console.WriteLine($"Starting at: {walker.Current}");

			// Navigate to source
			if (walker.GoToSource())
			{
				Console.WriteLine($"Moved to Source: {walker.Current}");
			}

			// Go back
			if (walker.GoBack())
			{
				Console.WriteLine($"Went back to: {walker.Current}");
			}

			// Navigate to target
			if (walker.GoToTarget())
			{
				Console.WriteLine($"Moved to Target: {walker.Current}");
			}

			// Navigate to linker
			walker.Reset();
			if (walker.GoToLinker())
			{
				Console.WriteLine($"Moved to Linker: {walker.Current}");
			}

			Console.WriteLine($"Steps taken: {walker.StepCount}");
			Console.WriteLine();
		}

		/// <summary>
		/// Demonstrates path tracking modes.
		/// </summary>
		public static void DemonstratePathTracking()
		{
			Console.WriteLine("=== Path Tracking Example ===");

			var link1 = Link.CreateLinkLinkingItself();
			var link2 = Link.CreateLinkLinkingItself();
			var link3 = Link.Create(link1, link2, link1);

			// Absolute path tracking
			var absoluteWalker = new LinkWalker(link3, PathTrackingMode.Absolute);
			absoluteWalker.GoToSource();
			absoluteWalker.GoToTarget();

			Console.WriteLine("Absolute path:");
			foreach (var link in absoluteWalker.AbsolutePath)
			{
				Console.WriteLine($"  {link}");
			}

			// Unique elements tracking (prevents infinite loops)
			var uniqueWalker = new LinkWalker(link3, PathTrackingMode.UniqueElements);
			uniqueWalker.GoToSource();

			if (uniqueWalker.HasVisited(link1))
			{
				Console.WriteLine("Link1 has already been visited!");
			}

			Console.WriteLine();
		}

		/// <summary>
		/// Demonstrates visitor pattern with event-driven traversal.
		/// </summary>
		public static void DemonstrateVisitorPattern()
		{
			Console.WriteLine("=== Visitor Pattern Example ===");

			var sourceLink = Link.CreateLinkLinkingItself();
			var targetLink = Link.CreateLinkLinkingItself();
			var mainLink = Link.Create(sourceLink, sourceLink, targetLink);

			var walker = new LinkWalker(mainLink, PathTrackingMode.UniqueElements);
			var visitor = new PrintVisitor();

			// Perform recursive walk with visitor
			walker.Walk(visitor, visitSource: true, visitTarget: true);

			Console.WriteLine(visitor.ToString());
		}

		/// <summary>
		/// Demonstrates Split and Join operations.
		/// </summary>
		public static void DemonstrateSplitJoin()
		{
			Console.WriteLine("=== Split and Join Example ===");

			var link1 = Link.CreateLinkLinkingItself();
			var link2 = Link.CreateLinkLinkingItself();
			var link3 = Link.CreateLinkLinkingItself();
			var mainLink = Link.Create(link1, link2, link3);

			// Create a walker
			var walker = new LinkWalker(mainLink, PathTrackingMode.Absolute);
			Console.WriteLine($"Main walker at: {walker.Current}");

			// Split the walker to explore both Source and Target simultaneously
			var splitWalker = walker.Split();
			Console.WriteLine($"Split walker created at: {splitWalker.Current}");

			// Main walker goes to Source
			walker.GoToSource();
			Console.WriteLine($"Main walker moved to Source: {walker.Current}");

			// Split walker goes to Target
			splitWalker.GoToTarget();
			Console.WriteLine($"Split walker moved to Target: {splitWalker.Current}");

			// Join them back when both are done
			if (walker.Join(splitWalker))
			{
				Console.WriteLine("Walkers joined successfully!");
			}

			Console.WriteLine();
		}

		/// <summary>
		/// Demonstrates walking through children.
		/// </summary>
		public static void DemonstrateWalkingChildren()
		{
			Console.WriteLine("=== Walking Children Example ===");

			// Create a parent link
			var parentLink = Link.CreateLinkLinkingItself();

			// Create several children using parent as source
			var child1 = Link.Create(parentLink, parentLink, parentLink);
			var child2 = Link.Create(parentLink, parentLink, parentLink);
			var child3 = Link.Create(parentLink, parentLink, parentLink);

			var walker = new LinkWalker(parentLink);

			Console.WriteLine("Children using parent as Source:");
			walker.WalkChildrenAsSource(child =>
			{
				Console.WriteLine($"  Child: {child}");
			});

			// Using visitor
			var collector = new CollectorVisitor();
			walker.WalkChildrenAsSource(collector);

			Console.WriteLine($"Collected {((List<Link>)collector.Collected).Count} children");
			Console.WriteLine();
		}

		/// <summary>
		/// Runs all examples.
		/// </summary>
		public static void RunAllExamples()
		{
			try
			{
				DemonstrateBasicNavigation();
				DemonstratePathTracking();
				DemonstrateVisitorPattern();
				DemonstrateSplitJoin();
				DemonstrateWalkingChildren();

				Console.WriteLine("All LinkWalker examples completed successfully!");
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error running examples: {ex.Message}");
				Console.WriteLine(ex.StackTrace);
			}
		}
	}
}
