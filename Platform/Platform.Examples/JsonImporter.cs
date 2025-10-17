using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Platform.Exceptions;
using Platform.Collections;
using Platform.IO;

namespace Platform.Examples
{
    public class JsonImporter<TLink>
    {
        private readonly IXmlStorage<TLink> _storage;

        public JsonImporter(IXmlStorage<TLink> storage) => _storage = storage;

        public Task Import(string file, CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    var document = _storage.CreateDocument(file);
                    var jsonString = System.IO.File.ReadAllText(file);

                    using (var jsonDocument = JsonDocument.Parse(jsonString))
                    {
                        Read(jsonDocument.RootElement, token, new ElementContext(document), "root");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToStringWithAllInnerExceptions());
                }

            }, token);
        }

        private void Read(JsonElement element, CancellationToken token, ElementContext context, string name)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            context.IncrementChildNameCount(name);
            var elementName = $"{name}[{context.ChildrenNamesCounts[name]}]";

            ConsoleHelpers.Debug("{0} starting...", elementName);

            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    var objectElement = _storage.CreateElement(name: elementName);
                    _storage.AttachElementToParent(elementToAttach: objectElement, parent: context.Parent);
                    var newContext = new ElementContext(objectElement);

                    foreach (var property in element.EnumerateObject())
                    {
                        Read(property.Value, token, newContext, property.Name);
                    }
                    break;

                case JsonValueKind.Array:
                    var arrayElement = _storage.CreateElement(name: elementName);
                    _storage.AttachElementToParent(elementToAttach: arrayElement, parent: context.Parent);
                    var arrayContext = new ElementContext(arrayElement);

                    int index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        Read(item, token, arrayContext, $"item{index}");
                        index++;
                    }
                    break;

                case JsonValueKind.String:
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                case JsonValueKind.Null:
                    string valueString;
                    if (element.ValueKind == JsonValueKind.String)
                    {
                        valueString = element.GetString();
                    }
                    else if (element.ValueKind == JsonValueKind.Number)
                    {
                        valueString = element.GetRawText();
                    }
                    else if (element.ValueKind == JsonValueKind.True)
                    {
                        valueString = "true";
                    }
                    else if (element.ValueKind == JsonValueKind.False)
                    {
                        valueString = "false";
                    }
                    else if (element.ValueKind == JsonValueKind.Null)
                    {
                        valueString = "null";
                    }
                    else
                    {
                        valueString = "";
                    }
                    ConsoleHelpers.Debug("Content: {0}", valueString.Truncate(50));
                    var textElement = _storage.CreateTextElement(content: valueString);
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
