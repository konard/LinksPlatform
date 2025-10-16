using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Communication.Protocol.Udp;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences;
using Platform.Data.Doublets.Unicode;

namespace Platform.Examples
{
    /// <summary>
    /// Host for managing multiple reference services.
    /// Acts as a coordinator for various small data type reference collections.
    /// Each service can be thought of as a neuron in the larger Links Platform system.
    /// </summary>
    public class ReferenceServiceHost
    {
        private readonly ILinks<ulong> _links;
        private readonly Sequences _sequences;
        private readonly UnicodeMap _unicodeMap;
        private readonly Dictionary<string, ReferenceService<ulong>> _services;
        private readonly UdpSender _sender;

        /// <summary>
        /// Initializes a new instance of the ReferenceServiceHost class.
        /// </summary>
        /// <param name="links">The links storage to use.</param>
        /// <param name="sequences">The sequences manager.</param>
        /// <param name="unicodeMap">The Unicode map for string conversion.</param>
        /// <param name="sender">The UDP sender for network communication.</param>
        public ReferenceServiceHost(ILinks<ulong> links, Sequences sequences, UnicodeMap unicodeMap, UdpSender sender)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _sequences = sequences ?? throw new ArgumentNullException(nameof(sequences));
            _unicodeMap = unicodeMap ?? throw new ArgumentNullException(nameof(unicodeMap));
            _sender = sender ?? throw new ArgumentNullException(nameof(sender));
            _services = new Dictionary<string, ReferenceService<ulong>>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Registers a new reference service for a specific data type.
        /// </summary>
        /// <param name="dataType">The data type name (e.g., "colors", "countries").</param>
        /// <param name="githubUrl">Optional GitHub URL to load initial data from.</param>
        public void RegisterService(string dataType, string githubUrl = null)
        {
            if (string.IsNullOrWhiteSpace(dataType))
            {
                throw new ArgumentException("Data type cannot be null or empty.", nameof(dataType));
            }

            if (_services.ContainsKey(dataType))
            {
                _sender.Send($"Service for '{dataType}' already registered.");
                return;
            }

            var service = new SimpleReferenceService(_links, _sequences, _unicodeMap, dataType);

            if (!string.IsNullOrWhiteSpace(githubUrl))
            {
                try
                {
                    service.LoadFromGitHub(githubUrl);
                    _sender.Send($"Service '{dataType}' registered and loaded from GitHub.");
                }
                catch (Exception ex)
                {
                    _sender.Send($"Service '{dataType}' registered but failed to load from GitHub: {ex.Message}");
                }
            }
            else
            {
                _sender.Send($"Service '{dataType}' registered.");
            }

            _services[dataType] = service;
        }

        /// <summary>
        /// Handles a query request for reference data.
        /// Format: "SERVICE:dataType:QUERY:referenceId" or "SERVICE:dataType:LIST"
        /// </summary>
        /// <param name="message">The query message.</param>
        public void HandleQuery(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var parts = message.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2 || !parts[0].Equals("SERVICE", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var dataType = parts[1];

            if (!_services.TryGetValue(dataType, out var service))
            {
                _sender.Send($"Service '{dataType}' not found. Available services: {string.Join(", ", _services.Keys)}");
                return;
            }

            if (parts.Length >= 3)
            {
                var command = parts[2].ToUpperInvariant();

                switch (command)
                {
                    case "LIST":
                        ListReferences(service);
                        break;

                    case "QUERY":
                        if (parts.Length >= 4)
                        {
                            QueryReference(service, parts[3]);
                        }
                        else
                        {
                            _sender.Send("QUERY command requires a reference ID.");
                        }
                        break;

                    case "SEARCH":
                        if (parts.Length >= 4)
                        {
                            SearchReferences(service, parts[3]);
                        }
                        else
                        {
                            _sender.Send("SEARCH command requires a search pattern.");
                        }
                        break;

                    case "EXPORT":
                        ExportService(service);
                        break;

                    default:
                        _sender.Send($"Unknown command '{command}'. Available: LIST, QUERY, SEARCH, EXPORT");
                        break;
                }
            }
            else
            {
                _sender.Send($"Service '{dataType}' found. Use SERVICE:{dataType}:LIST|QUERY|SEARCH|EXPORT");
            }
        }

        /// <summary>
        /// Lists all registered services.
        /// </summary>
        public void ListServices()
        {
            if (_services.Count == 0)
            {
                _sender.Send("No services registered.");
                return;
            }

            _sender.Send($"Registered services ({_services.Count}):");
            foreach (var serviceName in _services.Keys.OrderBy(k => k))
            {
                var service = _services[serviceName];
                var count = service.ListReferences().Count();
                _sender.Send($"  - {serviceName}: {count} references");
            }
        }

        private void ListReferences(ReferenceService<ulong> service)
        {
            var references = service.ListReferences().ToList();
            _sender.Send($"{service.DataType} references ({references.Count}):");

            foreach (var reference in references)
            {
                _sender.Send($"  - {reference}");
            }
        }

        private void QueryReference(ReferenceService<ulong> service, string referenceId)
        {
            var result = service.ServeReference(referenceId);

            if (result != null)
            {
                _sender.Send($"Reference found: {result}");
            }
            else
            {
                _sender.Send($"Reference '{referenceId}' not found in {service.DataType} collection.");
            }
        }

        private void SearchReferences(ReferenceService<ulong> service, string searchPattern)
        {
            var results = service.SearchReferences(searchPattern).ToList();

            if (results.Count > 0)
            {
                _sender.Send($"Found {results.Count} matching references:");
                foreach (var reference in results)
                {
                    _sender.Send($"  - {reference}");
                }
            }
            else
            {
                _sender.Send($"No references matching '{searchPattern}' found in {service.DataType} collection.");
            }
        }

        private void ExportService(ReferenceService<ulong> service)
        {
            var linoData = service.ExportToLiNo();
            _sender.Send($"Exported {service.DataType} to LiNo format:");
            _sender.Send(linoData);
        }

        /// <summary>
        /// Gets the number of registered services.
        /// </summary>
        public int ServiceCount => _services.Count;

        /// <summary>
        /// Gets the names of all registered services.
        /// </summary>
        public IEnumerable<string> ServiceNames => _services.Keys;
    }
}
