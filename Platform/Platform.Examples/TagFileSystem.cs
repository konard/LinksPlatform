using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Unicode;

namespace Platform.Examples
{
    /// <summary>
    /// Implements a tag-based file system using LinksPlatform's associative memory model.
    /// Files are stored in a flat collection with tags, where paths are represented through tag inheritance.
    /// Each file can have multiple tags, and tags can inherit from other tags to create hierarchical structures.
    /// </summary>
    public class TagFileSystem
    {
        private readonly ILinks<ulong> _links;
        private readonly ulong _fileMarker;
        private readonly ulong _tagMarker;
        private readonly ulong _hasTagRelation;
        private readonly ulong _inheritsFromRelation;
        private readonly ulong _filePathRelation;

        /// <summary>
        /// Initializes a new instance of TagFileSystem.
        /// </summary>
        /// <param name="links">The associative memory storage (doublets).</param>
        public TagFileSystem(ILinks<ulong> links)
        {
            _links = links;

            // Create marker links for different entity types
            _fileMarker = GetOrCreateMarker("FILE");
            _tagMarker = GetOrCreateMarker("TAG");
            _hasTagRelation = GetOrCreateMarker("HAS_TAG");
            _inheritsFromRelation = GetOrCreateMarker("INHERITS_FROM");
            _filePathRelation = GetOrCreateMarker("FILE_PATH");
        }

        /// <summary>
        /// Creates or retrieves a marker link for a given name.
        /// </summary>
        private ulong GetOrCreateMarker(string name)
        {
            var sequence = UnicodeMap.FromStringToLinkArray(name);
            var link = _links.SearchOrDefault(sequence[0], sequence[sequence.Length - 1]);

            if (link == _links.Constants.Null)
            {
                // Create a sequence representing the marker name
                link = _links.GetOrCreate(sequence[0], sequence[sequence.Length - 1]);
                for (int i = 1; i < sequence.Length - 1; i++)
                {
                    link = _links.Update(link, sequence[i], _links.GetTarget(link));
                }
            }

            return link;
        }

        /// <summary>
        /// Creates a new tag with the specified name.
        /// </summary>
        /// <param name="tagName">The name of the tag.</param>
        /// <returns>The link representing the tag.</returns>
        public ulong CreateTag(string tagName)
        {
            var tagSequence = UnicodeMap.FromStringToLinkArray(tagName);
            var tagContent = CreateSequence(tagSequence);
            var tag = _links.GetOrCreate(_tagMarker, tagContent);
            return tag;
        }

        /// <summary>
        /// Gets an existing tag by name, or returns null link if not found.
        /// </summary>
        /// <param name="tagName">The name of the tag.</param>
        /// <returns>The link representing the tag, or Constants.Null if not found.</returns>
        public ulong GetTag(string tagName)
        {
            var tagSequence = UnicodeMap.FromStringToLinkArray(tagName);
            var tagContent = SearchSequence(tagSequence);

            if (tagContent == _links.Constants.Null)
            {
                return _links.Constants.Null;
            }

            return _links.SearchOrDefault(_tagMarker, tagContent);
        }

        /// <summary>
        /// Gets or creates a tag with the specified name.
        /// </summary>
        /// <param name="tagName">The name of the tag.</param>
        /// <returns>The link representing the tag.</returns>
        public ulong GetOrCreateTag(string tagName)
        {
            var existing = GetTag(tagName);
            return existing != _links.Constants.Null ? existing : CreateTag(tagName);
        }

        /// <summary>
        /// Creates a new file entry with the specified path.
        /// </summary>
        /// <param name="filePath">The path of the file.</param>
        /// <returns>The link representing the file.</returns>
        public ulong CreateFile(string filePath)
        {
            var pathSequence = UnicodeMap.FromStringToLinkArray(filePath);
            var pathContent = CreateSequence(pathSequence);
            var pathLink = _links.GetOrCreate(_filePathRelation, pathContent);
            var file = _links.GetOrCreate(_fileMarker, pathLink);
            return file;
        }

        /// <summary>
        /// Gets an existing file by path, or returns null link if not found.
        /// </summary>
        /// <param name="filePath">The path of the file.</param>
        /// <returns>The link representing the file, or Constants.Null if not found.</returns>
        public ulong GetFile(string filePath)
        {
            var pathSequence = UnicodeMap.FromStringToLinkArray(filePath);
            var pathContent = SearchSequence(pathSequence);

            if (pathContent == _links.Constants.Null)
            {
                return _links.Constants.Null;
            }

            var pathLink = _links.SearchOrDefault(_filePathRelation, pathContent);
            if (pathLink == _links.Constants.Null)
            {
                return _links.Constants.Null;
            }

            return _links.SearchOrDefault(_fileMarker, pathLink);
        }

        /// <summary>
        /// Gets or creates a file with the specified path.
        /// </summary>
        /// <param name="filePath">The path of the file.</param>
        /// <returns>The link representing the file.</returns>
        public ulong GetOrCreateFile(string filePath)
        {
            var existing = GetFile(filePath);
            return existing != _links.Constants.Null ? existing : CreateFile(filePath);
        }

        /// <summary>
        /// Associates a tag with a file.
        /// </summary>
        /// <param name="file">The link representing the file.</param>
        /// <param name="tag">The link representing the tag.</param>
        public void AddTagToFile(ulong file, ulong tag)
        {
            var relation = _links.GetOrCreate(file, tag);
            _links.GetOrCreate(_hasTagRelation, relation);
        }

        /// <summary>
        /// Associates a tag with a file using their names.
        /// </summary>
        /// <param name="filePath">The path of the file.</param>
        /// <param name="tagName">The name of the tag.</param>
        public void AddTagToFile(string filePath, string tagName)
        {
            var file = GetOrCreateFile(filePath);
            var tag = GetOrCreateTag(tagName);
            AddTagToFile(file, tag);
        }

        /// <summary>
        /// Removes a tag from a file.
        /// </summary>
        /// <param name="file">The link representing the file.</param>
        /// <param name="tag">The link representing the tag.</param>
        public void RemoveTagFromFile(ulong file, ulong tag)
        {
            var relation = _links.SearchOrDefault(file, tag);
            if (relation != _links.Constants.Null)
            {
                var hasTagLink = _links.SearchOrDefault(_hasTagRelation, relation);
                if (hasTagLink != _links.Constants.Null)
                {
                    _links.Delete(hasTagLink);
                }
            }
        }

        /// <summary>
        /// Creates a tag inheritance relationship (parent -> child).
        /// This allows implementing hierarchical structures like paths.
        /// </summary>
        /// <param name="parentTag">The parent tag.</param>
        /// <param name="childTag">The child tag.</param>
        public void CreateTagInheritance(ulong parentTag, ulong childTag)
        {
            var relation = _links.GetOrCreate(childTag, parentTag);
            _links.GetOrCreate(_inheritsFromRelation, relation);
        }

        /// <summary>
        /// Creates a tag inheritance relationship using tag names.
        /// </summary>
        /// <param name="parentTagName">The name of the parent tag.</param>
        /// <param name="childTagName">The name of the child tag.</param>
        public void CreateTagInheritance(string parentTagName, string childTagName)
        {
            var parentTag = GetOrCreateTag(parentTagName);
            var childTag = GetOrCreateTag(childTagName);
            CreateTagInheritance(parentTag, childTag);
        }

        /// <summary>
        /// Gets all tags associated with a file.
        /// </summary>
        /// <param name="file">The link representing the file.</param>
        /// <returns>A list of tag links.</returns>
        public List<ulong> GetFileTags(ulong file)
        {
            var tags = new List<ulong>();

            _links.Each(link =>
            {
                if (_links.GetSource(link) == _hasTagRelation)
                {
                    var relation = _links.GetTarget(link);
                    var relationSource = _links.GetSource(relation);

                    if (relationSource == file)
                    {
                        tags.Add(_links.GetTarget(relation));
                    }
                }
                return _links.Constants.Continue;
            });

            return tags;
        }

        /// <summary>
        /// Gets all files that have a specific tag.
        /// </summary>
        /// <param name="tag">The link representing the tag.</param>
        /// <returns>A list of file links.</returns>
        public List<ulong> GetFilesWithTag(ulong tag)
        {
            var files = new List<ulong>();

            _links.Each(link =>
            {
                if (_links.GetSource(link) == _hasTagRelation)
                {
                    var relation = _links.GetTarget(link);
                    var relationTarget = _links.GetTarget(relation);

                    if (relationTarget == tag)
                    {
                        files.Add(_links.GetSource(relation));
                    }
                }
                return _links.Constants.Continue;
            });

            return files;
        }

        /// <summary>
        /// Gets all files that have a specific tag by tag name.
        /// </summary>
        /// <param name="tagName">The name of the tag.</param>
        /// <returns>A list of file links.</returns>
        public List<ulong> GetFilesWithTag(string tagName)
        {
            var tag = GetTag(tagName);
            if (tag == _links.Constants.Null)
            {
                return new List<ulong>();
            }
            return GetFilesWithTag(tag);
        }

        /// <summary>
        /// Gets all files that have ALL of the specified tags.
        /// </summary>
        /// <param name="tags">The tags to search for.</param>
        /// <returns>A list of file links that have all specified tags.</returns>
        public List<ulong> GetFilesWithAllTags(params ulong[] tags)
        {
            if (tags.Length == 0)
            {
                return new List<ulong>();
            }

            var filesWithFirstTag = GetFilesWithTag(tags[0]);

            if (tags.Length == 1)
            {
                return filesWithFirstTag;
            }

            return filesWithFirstTag.Where(file =>
            {
                var fileTags = GetFileTags(file);
                return tags.All(tag => fileTags.Contains(tag));
            }).ToList();
        }

        /// <summary>
        /// Gets all files that have ALL of the specified tags by tag names.
        /// </summary>
        /// <param name="tagNames">The names of the tags to search for.</param>
        /// <returns>A list of file links that have all specified tags.</returns>
        public List<ulong> GetFilesWithAllTags(params string[] tagNames)
        {
            var tags = tagNames.Select(name => GetTag(name))
                .Where(tag => tag != _links.Constants.Null)
                .ToArray();

            if (tags.Length != tagNames.Length)
            {
                return new List<ulong>(); // Some tags don't exist
            }

            return GetFilesWithAllTags(tags);
        }

        /// <summary>
        /// Gets the parent tags of a given tag (tags it inherits from).
        /// </summary>
        /// <param name="tag">The link representing the tag.</param>
        /// <returns>A list of parent tag links.</returns>
        public List<ulong> GetParentTags(ulong tag)
        {
            var parents = new List<ulong>();

            _links.Each(link =>
            {
                if (_links.GetSource(link) == _inheritsFromRelation)
                {
                    var relation = _links.GetTarget(link);
                    var relationSource = _links.GetSource(relation);

                    if (relationSource == tag)
                    {
                        parents.Add(_links.GetTarget(relation));
                    }
                }
                return _links.Constants.Continue;
            });

            return parents;
        }

        /// <summary>
        /// Gets the child tags of a given tag (tags that inherit from it).
        /// </summary>
        /// <param name="tag">The link representing the tag.</param>
        /// <returns>A list of child tag links.</returns>
        public List<ulong> GetChildTags(ulong tag)
        {
            var children = new List<ulong>();

            _links.Each(link =>
            {
                if (_links.GetSource(link) == _inheritsFromRelation)
                {
                    var relation = _links.GetTarget(link);
                    var relationTarget = _links.GetTarget(relation);

                    if (relationTarget == tag)
                    {
                        children.Add(_links.GetSource(relation));
                    }
                }
                return _links.Constants.Continue;
            });

            return children;
        }

        /// <summary>
        /// Helper method to create a sequence from an array of links.
        /// </summary>
        private ulong CreateSequence(ulong[] sequence)
        {
            if (sequence.Length == 0)
            {
                return _links.Constants.Null;
            }

            if (sequence.Length == 1)
            {
                return sequence[0];
            }

            ulong result = _links.GetOrCreate(sequence[0], sequence[1]);
            for (int i = 2; i < sequence.Length; i++)
            {
                result = _links.GetOrCreate(result, sequence[i]);
            }

            return result;
        }

        /// <summary>
        /// Helper method to search for a sequence.
        /// </summary>
        private ulong SearchSequence(ulong[] sequence)
        {
            if (sequence.Length == 0)
            {
                return _links.Constants.Null;
            }

            if (sequence.Length == 1)
            {
                return sequence[0];
            }

            ulong result = _links.SearchOrDefault(sequence[0], sequence[1]);
            if (result == _links.Constants.Null)
            {
                return _links.Constants.Null;
            }

            for (int i = 2; i < sequence.Length; i++)
            {
                result = _links.SearchOrDefault(result, sequence[i]);
                if (result == _links.Constants.Null)
                {
                    return _links.Constants.Null;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the file path for a given file link.
        /// </summary>
        /// <param name="file">The link representing the file.</param>
        /// <returns>The file path as a string, or null if not found.</returns>
        public string GetFilePath(ulong file)
        {
            if (_links.GetSource(file) != _fileMarker)
            {
                return null;
            }

            var pathLink = _links.GetTarget(file);
            if (_links.GetSource(pathLink) != _filePathRelation)
            {
                return null;
            }

            var pathContent = _links.GetTarget(pathLink);
            return LinkToString(pathContent);
        }

        /// <summary>
        /// Gets the tag name for a given tag link.
        /// </summary>
        /// <param name="tag">The link representing the tag.</param>
        /// <returns>The tag name as a string, or null if not found.</returns>
        public string GetTagName(ulong tag)
        {
            if (_links.GetSource(tag) != _tagMarker)
            {
                return null;
            }

            var tagContent = _links.GetTarget(tag);
            return LinkToString(tagContent);
        }

        /// <summary>
        /// Converts a link representing a string sequence back to a string.
        /// </summary>
        private string LinkToString(ulong link)
        {
            var chars = new List<char>();
            CollectChars(link, chars);
            return new string(chars.ToArray());
        }

        /// <summary>
        /// Recursively collects characters from a link sequence.
        /// </summary>
        private void CollectChars(ulong link, List<char> chars)
        {
            if (link == _links.Constants.Null || link == 0)
            {
                return;
            }

            // Check if this is a character (within Unicode map range)
            if (link <= UnicodeMap.MapSize)
            {
                chars.Add(UnicodeMap.FromLinkToChar(link));
                return;
            }

            var source = _links.GetSource(link);
            var target = _links.GetTarget(link);

            CollectChars(source, chars);
            CollectChars(target, chars);
        }
    }
}
