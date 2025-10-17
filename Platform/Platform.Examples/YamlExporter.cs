using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences;
using Platform.Data;
using Platform.Data.Doublets.Unicode;

namespace Platform.Examples
{
    /// <summary>
    /// Exports links data to YAML format
    /// </summary>
    public class YamlExporter
    {
        protected SynchronizedLinks<ulong> _links;
        protected bool _unicodeMapped;
        protected bool _convertUnicodeLinksToCharacters;
        protected HashSet<ulong> _visited;
        protected ulong _linksCounter;

        public void Export(SynchronizedLinks<ulong> links, string path, bool unicodeMapped, bool convertUnicodeLinksToCharacters, CancellationToken cancellationToken)
        {
            _links = links;
            _unicodeMapped = unicodeMapped;
            _convertUnicodeLinksToCharacters = convertUnicodeLinksToCharacters;
            _visited = new HashSet<ulong>();

            var linksData = new List<Dictionary<string, object>>();

            _linksCounter = 1;
            _links.Each(link =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return _links.Constants.Break;
                }
                var linkIndex = link[_links.Constants.IndexPart];
                if (!_unicodeMapped || _linksCounter > UnicodeMap.MapSize)
                {
                    if (Visit(linkIndex))
                    {
                        linksData.Add(CreateLinkData(link));
                    }
                }
                _linksCounter++;
                return _links.Constants.Continue;
            });

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yaml = serializer.Serialize(new { links = linksData });

            File.WriteAllText(path, yaml, Encoding.UTF8);
        }

        protected bool Visit(ulong linkIndex)
        {
            return _visited.Add(linkIndex);
        }

        protected virtual Dictionary<string, object> CreateLinkData(IList<ulong> link)
        {
            return new Dictionary<string, object>
            {
                { "index", link[_links.Constants.IndexPart] },
                { "source", FormatLink(link[_links.Constants.SourcePart]) },
                { "target", FormatLink(link[_links.Constants.TargetPart]) }
            };
        }

        protected string FormatLink(ulong link)
        {
            if (_unicodeMapped && _convertUnicodeLinksToCharacters && link <= UnicodeMap.MapSize)
            {
                var character = UnicodeMap.FromLinkToChar(link);
                return character.ToString();
            }
            return link.ToString();
        }
    }
}
