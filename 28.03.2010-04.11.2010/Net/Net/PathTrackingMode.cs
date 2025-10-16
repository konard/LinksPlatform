using System;

namespace Net
{
	/// <summary>
	/// Defines the path tracking mode for a LinkWalker.
	/// </summary>
	public enum PathTrackingMode
	{
		/// <summary>
		/// No path tracking.
		/// </summary>
		None,

		/// <summary>
		/// Absolute path tracking - logs each move.
		/// </summary>
		Absolute,

		/// <summary>
		/// Relative path tracking - tracks the shortest path back to the starting point.
		/// </summary>
		Relative,

		/// <summary>
		/// Tracks unique elements only to avoid infinite loops.
		/// </summary>
		UniqueElements
	}
}
