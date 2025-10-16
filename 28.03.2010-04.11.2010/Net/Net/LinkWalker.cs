using System;
using System.Collections.Generic;
using System.Linq;

namespace Net
{
	/// <summary>
	/// Provides navigation and traversal capabilities for Links.
	/// Supports path tracking, Split/Join operations, and event-driven traversal.
	/// </summary>
	public class LinkWalker
	{
		private Link _current;
		private readonly Link _start;
		private readonly Stack<Link> _absolutePath;
		private readonly Stack<Link> _relativePath;
		private readonly HashSet<Link> _visitedLinks;
		private readonly PathTrackingMode _trackingMode;
		private readonly LinkWalker _parent;
		private LinkWalker _splitChild;

		/// <summary>
		/// Gets the current link being pointed to by the walker.
		/// </summary>
		public Link Current => _current;

		/// <summary>
		/// Gets the starting link of the walker.
		/// </summary>
		public Link Start => _start;

		/// <summary>
		/// Gets the path tracking mode.
		/// </summary>
		public PathTrackingMode TrackingMode => _trackingMode;

		/// <summary>
		/// Gets the absolute path (all moves) if tracking is enabled.
		/// </summary>
		public IEnumerable<Link> AbsolutePath => _absolutePath?.Reverse();

		/// <summary>
		/// Gets the relative path (shortest path back to start) if tracking is enabled.
		/// </summary>
		public IEnumerable<Link> RelativePath => _relativePath?.Reverse();

		/// <summary>
		/// Gets the set of unique visited links if unique tracking is enabled.
		/// </summary>
		public IEnumerable<Link> VisitedLinks => _visitedLinks;

		/// <summary>
		/// Gets whether this walker was created via Split operation.
		/// </summary>
		public bool IsSplit => _parent != null;

		/// <summary>
		/// Gets the number of steps taken from the start.
		/// </summary>
		public int StepCount => _absolutePath?.Count ?? 0;

		/// <summary>
		/// Creates a new LinkWalker starting at the specified link.
		/// </summary>
		/// <param name="startLink">The link to start at</param>
		/// <param name="trackingMode">The path tracking mode</param>
		public LinkWalker(Link startLink, PathTrackingMode trackingMode = PathTrackingMode.Absolute)
		{
			_current = startLink;
			_start = startLink;
			_trackingMode = trackingMode;

			switch (trackingMode)
			{
				case PathTrackingMode.Absolute:
					_absolutePath = new Stack<Link>();
					break;
				case PathTrackingMode.Relative:
					_relativePath = new Stack<Link>();
					break;
				case PathTrackingMode.UniqueElements:
					_visitedLinks = new HashSet<Link>();
					_visitedLinks.Add(startLink);
					break;
			}
		}

		/// <summary>
		/// Private constructor for Split operation.
		/// </summary>
		private LinkWalker(Link current, Link start, Stack<Link> absolutePath, Stack<Link> relativePath, HashSet<Link> visitedLinks, PathTrackingMode trackingMode, LinkWalker parent)
		{
			_current = current;
			_start = start;
			_absolutePath = absolutePath;
			_relativePath = relativePath;
			_visitedLinks = visitedLinks;
			_trackingMode = trackingMode;
			_parent = parent;
		}

		/// <summary>
		/// Moves to the Source of the current link.
		/// </summary>
		/// <returns>True if the move was successful, false otherwise</returns>
		public bool GoToSource()
		{
			if (_current?.Source == null)
				return false;

			return MoveTo(_current.Source);
		}

		/// <summary>
		/// Moves to the Target of the current link.
		/// </summary>
		/// <returns>True if the move was successful, false otherwise</returns>
		public bool GoToTarget()
		{
			if (_current?.Target == null)
				return false;

			return MoveTo(_current.Target);
		}

		/// <summary>
		/// Moves to the Linker of the current link.
		/// </summary>
		/// <returns>True if the move was successful, false otherwise</returns>
		public bool GoToLinker()
		{
			if (_current?.Linker == null)
				return false;

			return MoveTo(_current.Linker);
		}

		/// <summary>
		/// Moves to the first child link that uses the current link as Source.
		/// </summary>
		/// <returns>True if the move was successful, false otherwise</returns>
		public bool GoToFirstChildAsSource()
		{
			var firstChild = _current?.ReferersBySource?.FirstOrDefault();
			if (firstChild == null)
				return false;

			return MoveTo(firstChild);
		}

		/// <summary>
		/// Moves to the last child link that uses the current link as Source.
		/// </summary>
		/// <returns>True if the move was successful, false otherwise</returns>
		public bool GoToLastChildAsSource()
		{
			var lastChild = _current?.ReferersBySource?.LastOrDefault();
			if (lastChild == null)
				return false;

			return MoveTo(lastChild);
		}

		/// <summary>
		/// Moves to the first child link that uses the current link as Target.
		/// </summary>
		/// <returns>True if the move was successful, false otherwise</returns>
		public bool GoToFirstChildAsTarget()
		{
			var firstChild = _current?.ReferersByTarget?.FirstOrDefault();
			if (firstChild == null)
				return false;

			return MoveTo(firstChild);
		}

		/// <summary>
		/// Moves to the last child link that uses the current link as Target.
		/// </summary>
		/// <returns>True if the move was successful, false otherwise</returns>
		public bool GoToLastChildAsTarget()
		{
			var lastChild = _current?.ReferersByTarget?.LastOrDefault();
			if (lastChild == null)
				return false;

			return MoveTo(lastChild);
		}

		/// <summary>
		/// Moves to the first child link that uses the current link as Linker.
		/// </summary>
		/// <returns>True if the move was successful, false otherwise</returns>
		public bool GoToFirstChildAsLinker()
		{
			var firstChild = _current?.ReferersByLinker?.FirstOrDefault();
			if (firstChild == null)
				return false;

			return MoveTo(firstChild);
		}

		/// <summary>
		/// Moves to the last child link that uses the current link as Linker.
		/// </summary>
		/// <returns>True if the move was successful, false otherwise</returns>
		public bool GoToLastChildAsLinker()
		{
			var lastChild = _current?.ReferersByLinker?.LastOrDefault();
			if (lastChild == null)
				return false;

			return MoveTo(lastChild);
		}

		/// <summary>
		/// Goes back one step in the path.
		/// </summary>
		/// <returns>True if going back was successful, false otherwise</returns>
		public bool GoBack()
		{
			Link previous = null;

			if (_trackingMode == PathTrackingMode.Absolute && _absolutePath.Count > 0)
			{
				previous = _absolutePath.Pop();
			}
			else if (_trackingMode == PathTrackingMode.Relative && _relativePath.Count > 0)
			{
				previous = _relativePath.Pop();
			}
			else
			{
				return false;
			}

			_current = previous;
			return true;
		}

		/// <summary>
		/// Checks if visiting a link would create an infinite loop.
		/// Only applicable when UniqueElements tracking is enabled.
		/// </summary>
		/// <param name="link">The link to check</param>
		/// <returns>True if the link has already been visited</returns>
		public bool HasVisited(Link link)
		{
			if (_trackingMode != PathTrackingMode.UniqueElements)
				throw new InvalidOperationException("HasVisited can only be called when PathTrackingMode is UniqueElements");

			return _visitedLinks.Contains(link);
		}

		/// <summary>
		/// Splits this walker into two walkers sharing a common path.
		/// The original walker continues, and a new walker is created at the same position.
		/// </summary>
		/// <returns>A new LinkWalker at the current position sharing the path history</returns>
		public LinkWalker Split()
		{
			var splitWalker = new LinkWalker(
				_current,
				_start,
				_absolutePath,
				_relativePath,
				_visitedLinks,
				_trackingMode,
				this
			);

			_splitChild = splitWalker;
			return splitWalker;
		}

		/// <summary>
		/// Joins a split walker back with its parent.
		/// Both walkers must have completed their tasks and returned to a joinable state.
		/// </summary>
		/// <param name="splitWalker">The walker to join with</param>
		/// <returns>True if the join was successful</returns>
		public bool Join(LinkWalker splitWalker)
		{
			if (splitWalker == null)
				throw new ArgumentNullException(nameof(splitWalker));

			if (splitWalker._parent != this)
				throw new InvalidOperationException("Can only join a walker that was split from this walker");

			if (_splitChild != splitWalker)
				throw new InvalidOperationException("Split walker has been modified");

			_splitChild = null;
			return true;
		}

		/// <summary>
		/// Walks through all children that use the current link as Source.
		/// </summary>
		/// <param name="visitor">The visitor to call for each child</param>
		public void WalkChildrenAsSource(ILinkVisitor visitor)
		{
			if (_current == null || visitor == null)
				return;

			foreach (var child in _current.ReferersBySource)
			{
				if (!visitor.Visit(child))
					break;

				visitor.Leave(child);
			}
		}

		/// <summary>
		/// Walks through all children that use the current link as Source.
		/// </summary>
		/// <param name="action">The action to call for each child</param>
		public void WalkChildrenAsSource(Action<Link> action)
		{
			if (_current == null || action == null)
				return;

			foreach (var child in _current.ReferersBySource)
			{
				action(child);
			}
		}

		/// <summary>
		/// Walks through all children that use the current link as Target.
		/// </summary>
		/// <param name="visitor">The visitor to call for each child</param>
		public void WalkChildrenAsTarget(ILinkVisitor visitor)
		{
			if (_current == null || visitor == null)
				return;

			foreach (var child in _current.ReferersByTarget)
			{
				if (!visitor.Visit(child))
					break;

				visitor.Leave(child);
			}
		}

		/// <summary>
		/// Walks through all children that use the current link as Target.
		/// </summary>
		/// <param name="action">The action to call for each child</param>
		public void WalkChildrenAsTarget(Action<Link> action)
		{
			if (_current == null || action == null)
				return;

			foreach (var child in _current.ReferersByTarget)
			{
				action(child);
			}
		}

		/// <summary>
		/// Walks through all children that use the current link as Linker.
		/// </summary>
		/// <param name="visitor">The visitor to call for each child</param>
		public void WalkChildrenAsLinker(ILinkVisitor visitor)
		{
			if (_current == null || visitor == null)
				return;

			foreach (var child in _current.ReferersByLinker)
			{
				if (!visitor.Visit(child))
					break;

				visitor.Leave(child);
			}
		}

		/// <summary>
		/// Walks through all children that use the current link as Linker.
		/// </summary>
		/// <param name="action">The action to call for each child</param>
		public void WalkChildrenAsLinker(Action<Link> action)
		{
			if (_current == null || action == null)
				return;

			foreach (var child in _current.ReferersByLinker)
			{
				action(child);
			}
		}

		/// <summary>
		/// Performs a recursive traversal starting from the current link.
		/// </summary>
		/// <param name="visitor">The visitor to call for each link</param>
		/// <param name="visitSource">Whether to visit the Source</param>
		/// <param name="visitTarget">Whether to visit the Target</param>
		public void Walk(ILinkVisitor visitor, bool visitSource = true, bool visitTarget = true)
		{
			if (_current == null || visitor == null)
				return;

			WalkRecursive(_current, visitor, visitSource, visitTarget);
		}

		private void WalkRecursive(Link link, ILinkVisitor visitor, bool visitSource, bool visitTarget)
		{
			if (link == null)
				return;

			if (_trackingMode == PathTrackingMode.UniqueElements && _visitedLinks.Contains(link))
				return;

			if (!visitor.Visit(link))
				return;

			if (_trackingMode == PathTrackingMode.UniqueElements)
				_visitedLinks.Add(link);

			if (visitSource)
				WalkRecursive(link.Source, visitor, visitSource, visitTarget);

			if (visitTarget)
				WalkRecursive(link.Target, visitor, visitSource, visitTarget);

			visitor.Leave(link);
		}

		private bool MoveTo(Link newLink)
		{
			if (newLink == null)
				return false;

			if (_trackingMode == PathTrackingMode.UniqueElements && _visitedLinks.Contains(newLink))
				return false; // Prevent infinite loops

			var previousLink = _current;

			if (_trackingMode == PathTrackingMode.Absolute)
			{
				_absolutePath.Push(previousLink);
			}
			else if (_trackingMode == PathTrackingMode.Relative)
			{
				// For relative path, we need to track the inverse move
				_relativePath.Push(previousLink);
			}
			else if (_trackingMode == PathTrackingMode.UniqueElements)
			{
				_visitedLinks.Add(newLink);
			}

			_current = newLink;
			return true;
		}

		/// <summary>
		/// Resets the walker back to the starting link.
		/// </summary>
		public void Reset()
		{
			_current = _start;
			_absolutePath?.Clear();
			_relativePath?.Clear();
			_visitedLinks?.Clear();
			if (_trackingMode == PathTrackingMode.UniqueElements)
			{
				_visitedLinks?.Add(_start);
			}
		}
	}
}
