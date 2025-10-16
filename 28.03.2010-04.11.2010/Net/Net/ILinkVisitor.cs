using System;

namespace Net
{
	/// <summary>
	/// Interface for event-driven link traversal.
	/// Implements the Visitor pattern for Links.
	/// </summary>
	public interface ILinkVisitor
	{
		/// <summary>
		/// Called when entering a link during traversal.
		/// </summary>
		/// <param name="link">The link being visited</param>
		/// <returns>True to continue traversal, false to stop</returns>
		bool Visit(Link link);

		/// <summary>
		/// Called when leaving a link during traversal.
		/// </summary>
		/// <param name="link">The link being left</param>
		void Leave(Link link);
	}
}
