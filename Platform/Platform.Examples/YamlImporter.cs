using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using Platform.Exceptions;
using Platform.Collections;
using Platform.IO;

namespace Platform.Examples
{
    public class YamlImporter<TLink>
    {
        private readonly IXmlStorage<TLink> _storage;

        public YamlImporter(IXmlStorage<TLink> storage) => _storage = storage;

        public Task Import(string file, CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    var document = _storage.CreateDocument(file);
                    var yamlStream = new YamlStream();

                    using (var reader = new System.IO.StreamReader(file))
                    {
                        yamlStream.Load(reader);
                    }

                    foreach (var yamlDocument in yamlStream.Documents)
                    {
                        Read(yamlDocument.RootNode, token, new ElementContext(document), "root");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToStringWithAllInnerExceptions());
                }

            }, token);
        }

        private void Read(YamlNode node, CancellationToken token, ElementContext context, string name)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            context.IncrementChildNameCount(name);
            var elementName = $"{name}[{context.ChildrenNamesCounts[name]}]";

            ConsoleHelpers.Debug("{0} starting...", elementName);

            switch (node)
            {
                case YamlMappingNode mappingNode:
                    var mappingElement = _storage.CreateElement(name: elementName);
                    _storage.AttachElementToParent(elementToAttach: mappingElement, parent: context.Parent);
                    var mappingContext = new ElementContext(mappingElement);

                    foreach (var entry in mappingNode.Children)
                    {
                        var key = (entry.Key as YamlScalarNode)?.Value ?? "unknown";
                        Read(entry.Value, token, mappingContext, key);
                    }
                    break;

                case YamlSequenceNode sequenceNode:
                    var sequenceElement = _storage.CreateElement(name: elementName);
                    _storage.AttachElementToParent(elementToAttach: sequenceElement, parent: context.Parent);
                    var sequenceContext = new ElementContext(sequenceElement);

                    int index = 0;
                    foreach (var item in sequenceNode.Children)
                    {
                        Read(item, token, sequenceContext, $"item{index}");
                        index++;
                    }
                    break;

                case YamlScalarNode scalarNode:
                    var value = scalarNode.Value;
                    ConsoleHelpers.Debug("Content: {0}", value.Truncate(50));
                    var textElement = _storage.CreateTextElement(content: value);
                    _storage.AttachElementToParent(textElement, context.Parent);
                    break;
            }

            ConsoleHelpers.Debug("{0} finished.", elementName);
        }

        private class ElementContext : XmlElementContext
        {
            public readonly TLink Parent;

            public ElementContext(TLink parent)
            {
                Parent = parent;
            }
        }
    }
}
