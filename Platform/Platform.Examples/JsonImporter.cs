using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Platform.Exceptions;

namespace Platform.Examples
{
    /// <summary>
    /// Imports JSON data into links storage using hierarchical structure.
    /// </summary>
    public class JsonImporter<TLink>
    {
        private readonly IHierarchicalStorage<TLink> _storage;

        public JsonImporter(IHierarchicalStorage<TLink> storage) => _storage = storage;

        public Task Import(string file, CancellationToken token)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    var document = _storage.CreateRoot(file);
                    using (var stream = File.OpenRead(file))
                    using (var jsonDocument = JsonDocument.Parse(stream))
                    {
                        ProcessElement(jsonDocument.RootElement, document, token);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToStringWithAllInnerExceptions());
                }
            }, token);
        }

        private void ProcessElement(JsonElement element, TLink parent, CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var property in element.EnumerateObject())
                    {
                        if (token.IsCancellationRequested) return;

                        var propertyNode = _storage.CreateNode(property.Name, "property");
                        _storage.AttachToParent(propertyNode, parent);
                        ProcessElement(property.Value, propertyNode, token);
                    }
                    break;

                case JsonValueKind.Array:
                    var index = 0;
                    foreach (var item in element.EnumerateArray())
                    {
                        if (token.IsCancellationRequested) return;

                        var arrayItemNode = _storage.CreateNode($"[{index}]", "arrayItem");
                        _storage.AttachToParent(arrayItemNode, parent);
                        ProcessElement(item, arrayItemNode, token);
                        index++;
                    }
                    break;

                case JsonValueKind.String:
                    var stringValue = element.GetString();
                    var stringNode = _storage.CreateValueNode(stringValue);
                    _storage.AttachToParent(stringNode, parent);
                    break;

                case JsonValueKind.Number:
                    var numberValue = element.GetRawText();
                    var numberNode = _storage.CreateValueNode(numberValue);
                    _storage.AttachToParent(numberNode, parent);
                    break;

                case JsonValueKind.True:
                case JsonValueKind.False:
                    var boolValue = element.GetBoolean().ToString();
                    var boolNode = _storage.CreateValueNode(boolValue);
                    _storage.AttachToParent(boolNode, parent);
                    break;

                case JsonValueKind.Null:
                    var nullNode = _storage.CreateValueNode("null");
                    _storage.AttachToParent(nullNode, parent);
                    break;
            }
        }
    }
}
