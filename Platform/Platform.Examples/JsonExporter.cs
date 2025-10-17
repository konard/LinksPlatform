using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Collections.Generic;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences;
using Platform.Data;
using Platform.Data.Doublets.Unicode;

namespace Platform.Examples
{
    /// <summary>
    /// Exports links data to JSON format
    /// </summary>
    public class JsonExporter
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

            using (var file = File.OpenWrite(path))
            using (var writer = new Utf8JsonWriter(file, new JsonWriterOptions
            {
                Indented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }))
            {
                writer.WriteStartObject();
                writer.WritePropertyName("links");
                writer.WriteStartArray();

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
                            WriteLink(writer, link);
                        }
                    }
                    _linksCounter++;
                    return _links.Constants.Continue;
                });

                writer.WriteEndArray();
                writer.WriteEndObject();
            }
        }

        protected bool Visit(ulong linkIndex)
        {
            return _visited.Add(linkIndex);
        }

        protected virtual void WriteLink(Utf8JsonWriter writer, IList<ulong> link)
        {
            writer.WriteStartObject();
            writer.WriteNumber("index", link[_links.Constants.IndexPart]);
            writer.WriteString("source", FormatLink(link[_links.Constants.SourcePart]));
            writer.WriteString("target", FormatLink(link[_links.Constants.TargetPart]));
            writer.WriteEndObject();
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
