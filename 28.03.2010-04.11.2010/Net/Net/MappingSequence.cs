using System;

namespace Net
{
	/// <summary>
	/// Provides a mapping sequence structure stored in the first link of the database.
	/// The mapping array uses power-of-2 sizes (2, 4, 8, 16, 32...) and is implemented
	/// as a binary tree where the leftmost branch height determines the size.
	/// </summary>
	public static class MappingSequence
	{
		private static Link _emptyLink;

		/// <summary>
		/// Gets or sets the empty/null marker link used to represent empty values in the mapping sequence.
		/// </summary>
		public static Link EmptyLink
		{
			get
			{
				if (_emptyLink == null)
				{
					// Create a special "empty" marker link
					_emptyLink = Link.CreateLinkLinkingItself();
				}
				return _emptyLink;
			}
			set
			{
				_emptyLink = value;
			}
		}

		/// <summary>
		/// Enlarges the mapping sequence by doubling its size.
		/// Half of the new values are filled with the empty/null marker link.
		/// </summary>
		/// <param name="root">The root link of the mapping sequence.</param>
		/// <returns>The new root link with doubled size.</returns>
		public static Link EnlargeMappingSequence(Link root)
		{
			if (root == null)
			{
				// Initialize with size 2: [empty, empty]
				return Link.Create(EmptyLink, Net.And, EmptyLink);
			}

			int currentSize = GetMappingSequenceSize(root);
			int newSize = currentSize * 2;

			// The new structure maintains the old data on the left
			// and fills the right half with empty values
			Link rightHalf = CreateEmptySequence(currentSize);

			return Link.Create(root, Net.And, rightHalf);
		}

		/// <summary>
		/// Shrinks the mapping sequence by dividing its size by 2.
		/// Half of the values are deleted (the right half).
		/// </summary>
		/// <param name="root">The root link of the mapping sequence.</param>
		/// <returns>The new root link with halved size.</returns>
		public static Link ShrinkMappingSequence(Link root)
		{
			if (root == null)
			{
				throw new InvalidOperationException("Cannot shrink an empty mapping sequence.");
			}

			int currentSize = GetMappingSequenceSize(root);
			if (currentSize <= 2)
			{
				throw new InvalidOperationException("Cannot shrink mapping sequence below size 2.");
			}

			// Return the left half (which contains the first half of elements)
			if (root.Linker == Net.And)
			{
				return root.Source;
			}

			throw new InvalidOperationException("Mapping sequence structure is invalid.");
		}

		/// <summary>
		/// Gets the size of the mapping sequence by calculating the height of the leftmost branch.
		/// </summary>
		/// <param name="root">The root link of the mapping sequence.</param>
		/// <returns>The size of the mapping sequence (always a power of 2).</returns>
		public static int GetMappingSequenceSize(Link root)
		{
			if (root == null)
			{
				return 0;
			}

			int height = GetLeftmostHeight(root);
			return (int)Math.Pow(2, height);
		}

		/// <summary>
		/// Gets an element from the mapping sequence at the specified index.
		/// </summary>
		/// <param name="root">The root link of the mapping sequence.</param>
		/// <param name="index">The index of the element (0-based).</param>
		/// <returns>The link at the specified index.</returns>
		public static Link GetMappingSequenceElement(Link root, int index)
		{
			if (root == null)
			{
				throw new ArgumentNullException("root", "Mapping sequence root cannot be null.");
			}

			int size = GetMappingSequenceSize(root);
			if (index < 0 || index >= size)
			{
				throw new ArgumentOutOfRangeException("index", $"Index {index} is out of range [0, {size}).");
			}

			return GetElementRecursive(root, index, size);
		}

		/// <summary>
		/// Sets an element in the mapping sequence at the specified index.
		/// </summary>
		/// <param name="root">The root link of the mapping sequence.</param>
		/// <param name="index">The index where to set the element (0-based).</param>
		/// <param name="value">The link value to set.</param>
		/// <returns>The new root link with the updated value.</returns>
		public static Link SetMappingSequenceElement(Link root, int index, Link value)
		{
			if (root == null)
			{
				throw new ArgumentNullException("root", "Mapping sequence root cannot be null.");
			}

			int size = GetMappingSequenceSize(root);
			if (index < 0 || index >= size)
			{
				throw new ArgumentOutOfRangeException("index", $"Index {index} is out of range [0, {size}).");
			}

			return SetElementRecursive(root, index, size, value);
		}

		#region Private Helper Methods

		/// <summary>
		/// Gets the height of the leftmost branch in the tree structure.
		/// </summary>
		private static int GetLeftmostHeight(Link link)
		{
			if (link == null)
			{
				return 0;
			}

			// If this is a sequence node (using And as linker)
			if (link.Linker == Net.And)
			{
				return 1 + GetLeftmostHeight(link.Source);
			}

			// Leaf node (single element)
			return 1;
		}

		/// <summary>
		/// Creates an empty sequence of the specified size filled with empty marker links.
		/// </summary>
		private static Link CreateEmptySequence(int size)
		{
			if (size == 1)
			{
				return EmptyLink;
			}

			int halfSize = size / 2;
			Link left = CreateEmptySequence(halfSize);
			Link right = CreateEmptySequence(halfSize);

			return Link.Create(left, Net.And, right);
		}

		/// <summary>
		/// Recursively retrieves an element at the specified index.
		/// </summary>
		private static Link GetElementRecursive(Link node, int index, int currentSize)
		{
			// Base case: single element
			if (currentSize == 1)
			{
				return node;
			}

			int halfSize = currentSize / 2;

			// Navigate to left or right subtree
			if (index < halfSize)
			{
				// Element is in the left subtree
				return GetElementRecursive(node.Source, index, halfSize);
			}
			else
			{
				// Element is in the right subtree
				return GetElementRecursive(node.Target, index - halfSize, halfSize);
			}
		}

		/// <summary>
		/// Recursively sets an element at the specified index, creating a new tree structure.
		/// </summary>
		private static Link SetElementRecursive(Link node, int index, int currentSize, Link value)
		{
			// Base case: single element - replace it
			if (currentSize == 1)
			{
				return value;
			}

			int halfSize = currentSize / 2;

			// Navigate to left or right subtree
			if (index < halfSize)
			{
				// Update element in the left subtree
				Link newLeft = SetElementRecursive(node.Source, index, halfSize, value);
				return Link.Create(newLeft, Net.And, node.Target);
			}
			else
			{
				// Update element in the right subtree
				Link newRight = SetElementRecursive(node.Target, index - halfSize, halfSize, value);
				return Link.Create(node.Source, Net.And, newRight);
			}
		}

		#endregion
	}
}
