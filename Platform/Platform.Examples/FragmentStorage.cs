using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace Platform.Examples
{
    /// <summary>
    /// Represents a fragment entry with link address and content location
    /// </summary>
    /// <typeparam name="TLink">The type of link address</typeparam>
    public struct Fragment<TLink>
    {
        /// <summary>
        /// The address of the link associated with this fragment
        /// </summary>
        public TLink LinkAddress { get; set; }

        /// <summary>
        /// The offset in the FragmentContent storage where the content starts
        /// </summary>
        public long FragmentContentOffset { get; set; }

        /// <summary>
        /// The length of the fragment content in bytes
        /// </summary>
        public long FragmentContentLength { get; set; }
    }

    /// <summary>
    /// Manages storage of variable-length fragments in a single file.
    /// Uses two logical tables: Fragments (metadata) and FragmentContent (actual content)
    /// </summary>
    /// <typeparam name="TLink">The type of link address</typeparam>
    public class FragmentStorage<TLink> : IDisposable
    {
        private readonly string _filePath;
        private readonly Dictionary<TLink, Fragment<TLink>> _fragments;
        private FileStream _contentStream;
        private long _currentContentOffset;

        /// <summary>
        /// Initializes a new instance of FragmentStorage
        /// </summary>
        /// <param name="filePath">Path to the storage file</param>
        public FragmentStorage(string filePath)
        {
            _filePath = filePath;
            _fragments = new Dictionary<TLink, Fragment<TLink>>();
            _currentContentOffset = 0;

            if (File.Exists(filePath))
            {
                _contentStream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                LoadFragments();
            }
            else
            {
                _contentStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            }
        }

        /// <summary>
        /// Stores a fragment with its content
        /// </summary>
        /// <param name="linkAddress">The link address associated with the fragment</param>
        /// <param name="content">The content to store</param>
        public void StoreFragment(TLink linkAddress, byte[] content)
        {
            if (content == null || content.Length == 0)
            {
                throw new ArgumentException("Content cannot be null or empty", nameof(content));
            }

            var fragment = new Fragment<TLink>
            {
                LinkAddress = linkAddress,
                FragmentContentOffset = _currentContentOffset,
                FragmentContentLength = content.Length
            };

            _contentStream.Seek(_currentContentOffset, SeekOrigin.Begin);
            _contentStream.Write(content, 0, content.Length);
            _contentStream.Flush();

            _fragments[linkAddress] = fragment;
            _currentContentOffset += content.Length;
        }

        /// <summary>
        /// Retrieves a fragment's content by link address
        /// </summary>
        /// <param name="linkAddress">The link address to retrieve</param>
        /// <returns>The fragment content as byte array</returns>
        public byte[] GetFragment(TLink linkAddress)
        {
            if (!_fragments.TryGetValue(linkAddress, out var fragment))
            {
                throw new KeyNotFoundException($"Fragment with link address {linkAddress} not found");
            }

            var content = new byte[fragment.FragmentContentLength];
            _contentStream.Seek(fragment.FragmentContentOffset, SeekOrigin.Begin);
            _contentStream.Read(content, 0, (int)fragment.FragmentContentLength);

            return content;
        }

        /// <summary>
        /// Gets fragment metadata without reading the content
        /// </summary>
        /// <param name="linkAddress">The link address to retrieve</param>
        /// <returns>The fragment metadata</returns>
        public Fragment<TLink> GetFragmentMetadata(TLink linkAddress)
        {
            if (!_fragments.TryGetValue(linkAddress, out var fragment))
            {
                throw new KeyNotFoundException($"Fragment with link address {linkAddress} not found");
            }

            return fragment;
        }

        /// <summary>
        /// Checks if a fragment exists for the given link address
        /// </summary>
        /// <param name="linkAddress">The link address to check</param>
        /// <returns>True if fragment exists, false otherwise</returns>
        public bool ContainsFragment(TLink linkAddress)
        {
            return _fragments.ContainsKey(linkAddress);
        }

        /// <summary>
        /// Gets all stored fragments
        /// </summary>
        /// <returns>Collection of all fragments</returns>
        public IEnumerable<Fragment<TLink>> GetAllFragments()
        {
            return _fragments.Values;
        }

        /// <summary>
        /// Loads fragment metadata from the file
        /// This is a simplified version - in production, metadata would be stored separately
        /// </summary>
        private void LoadFragments()
        {
            _currentContentOffset = _contentStream.Length;
        }

        /// <summary>
        /// Disposes the storage and closes the file
        /// </summary>
        public void Dispose()
        {
            _contentStream?.Dispose();
        }
    }
}
