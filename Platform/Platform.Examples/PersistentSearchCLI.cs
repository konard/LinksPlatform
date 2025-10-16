using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences;
using Platform.Data.Doublets.Unicode;

namespace Platform.Examples
{
    /// <summary>
    /// Command-line interface for persistent search and goal completion assist.
    /// Provides interactive commands for managing persistent searches.
    /// </summary>
    public class PersistentSearchCLI<TLinkAddress> : ICommandLineInterface
        where TLinkAddress : struct, IEquatable<TLinkAddress>, IComparable<TLinkAddress>
    {
        private readonly ILinks<TLinkAddress> _links;
        private readonly PersistentSearch<TLinkAddress> _persistentSearch;
        private readonly Sequences _sequences;
        private readonly Action<string> _output;
        private TLinkAddress _currentUser;

        public PersistentSearchCLI(ILinks<TLinkAddress> links, Sequences sequences, Action<string> output)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _sequences = sequences ?? throw new ArgumentNullException(nameof(sequences));
            _output = output ?? throw new ArgumentNullException(nameof(output));
            _persistentSearch = new PersistentSearch<TLinkAddress>(links);
        }

        public void Run(string[] args)
        {
            if (args.Length == 0)
            {
                ShowHelp();
                return;
            }

            var command = args[0].ToLower();

            try
            {
                switch (command)
                {
                    case "subscribe":
                        HandleSubscribe(args);
                        break;
                    case "unsubscribe":
                        HandleUnsubscribe(args);
                        break;
                    case "satisfy":
                        HandleSatisfy(args);
                        break;
                    case "list":
                        HandleList(args);
                        break;
                    case "info":
                        HandleInfo(args);
                        break;
                    case "setuser":
                        HandleSetUser(args);
                        break;
                    case "help":
                        ShowHelp();
                        break;
                    default:
                        _output($"Unknown command: {command}");
                        _output("Type 'help' for available commands.");
                        break;
                }
            }
            catch (Exception ex)
            {
                _output($"Error: {ex.Message}");
            }
        }

        private void ShowHelp()
        {
            _output("Persistent Search / Goal Completion Assist Commands:");
            _output("");
            _output("  setuser <userId>           - Set current user ID");
            _output("  subscribe <query>          - Subscribe to persistent search");
            _output("  unsubscribe <subId>        - Unsubscribe from search");
            _output("  satisfy <subId> [linkId]   - Mark search as satisfied");
            _output("  list                       - List active subscriptions");
            _output("  info <subId>               - Show subscription details");
            _output("  help                       - Show this help");
            _output("");
            _output("Description:");
            _output("  This system allows users to register search requests that persist");
            _output("  until the goal is achieved. Users are notified when new results");
            _output("  appear that match their search criteria.");
            _output("");
            _output("Examples:");
            _output("  setuser 1");
            _output("  subscribe \"concept1 concept2\"");
            _output("  list");
            _output("  satisfy 5 12");
        }

        private void HandleSetUser(string[] args)
        {
            if (args.Length < 2)
            {
                _output("Usage: setuser <userId>");
                return;
            }

            if (TryParseLink(args[1], out var userId))
            {
                _currentUser = userId;
                _output($"Current user set to: {userId}");
            }
            else
            {
                _output($"Invalid user ID: {args[1]}");
            }
        }

        private void HandleSubscribe(string[] args)
        {
            if (args.Length < 2)
            {
                _output("Usage: subscribe <query>");
                _output("  Query can contain link IDs separated by spaces");
                return;
            }

            if (_currentUser.Equals(default(TLinkAddress)))
            {
                _output("Please set current user first using 'setuser <userId>'");
                return;
            }

            // Parse query - simplified version, just parse link IDs
            var queryParts = args[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var query = new List<TLinkAddress>();

            foreach (var part in queryParts)
            {
                if (TryParseLink(part, out var linkId))
                {
                    query.Add(linkId);
                }
                else
                {
                    _output($"Warning: Could not parse '{part}' as link ID, skipping");
                }
            }

            if (query.Count == 0)
            {
                _output("Error: No valid query elements");
                return;
            }

            var subscriptionId = _persistentSearch.Subscribe(
                userId: _currentUser,
                searchQuery: query.ToArray(),
                onResultFound: (resultLink) =>
                {
                    _output($"[NOTIFICATION] New result found for subscription: {resultLink}");
                    _output($"  Source: {_links.GetSource(resultLink)}, Target: {_links.GetTarget(resultLink)}");
                },
                onGoalComplete: () =>
                {
                    _output($"[COMPLETE] Search goal achieved!");
                }
            );

            _output($"Subscription created: {subscriptionId}");
            _output($"  User: {_currentUser}");
            _output($"  Query: [{string.Join(", ", query)}]");
        }

        private void HandleUnsubscribe(string[] args)
        {
            if (args.Length < 2)
            {
                _output("Usage: unsubscribe <subscriptionId>");
                return;
            }

            if (TryParseLink(args[1], out var subscriptionId))
            {
                if (_persistentSearch.Unsubscribe(subscriptionId))
                {
                    _output($"Successfully unsubscribed from: {subscriptionId}");
                }
                else
                {
                    _output($"Subscription not found: {subscriptionId}");
                }
            }
            else
            {
                _output($"Invalid subscription ID: {args[1]}");
            }
        }

        private void HandleSatisfy(string[] args)
        {
            if (args.Length < 2)
            {
                _output("Usage: satisfy <subscriptionId> [satisfyingLinkId]");
                return;
            }

            if (!TryParseLink(args[1], out var subscriptionId))
            {
                _output($"Invalid subscription ID: {args[1]}");
                return;
            }

            TLinkAddress? satisfyingLink = null;
            if (args.Length >= 3 && TryParseLink(args[2], out var linkId))
            {
                satisfyingLink = linkId;
            }

            _persistentSearch.MarkAsSatisfied(subscriptionId, satisfyingLink);
            _output($"Subscription {subscriptionId} marked as satisfied");
            if (satisfyingLink.HasValue)
            {
                _output($"  Satisfied with link: {satisfyingLink.Value}");
            }
        }

        private void HandleList(string[] args)
        {
            if (_currentUser.Equals(default(TLinkAddress)))
            {
                _output("Please set current user first using 'setuser <userId>'");
                return;
            }

            var subscriptions = _persistentSearch.GetUserSubscriptions(_currentUser);

            if (subscriptions.Count == 0)
            {
                _output("No active subscriptions for current user");
                return;
            }

            _output($"Active subscriptions for user {_currentUser}:");
            foreach (var subId in subscriptions)
            {
                var info = _persistentSearch.GetSubscriptionInfo(subId);
                _output($"  Subscription {subId}:");
                _output($"    Query: [{string.Join(", ", info.SearchQuery)}]");
                _output($"    Results found: {info.FoundResults.Count}");
                _output($"    Created: {info.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
            }
        }

        private void HandleInfo(string[] args)
        {
            if (args.Length < 2)
            {
                _output("Usage: info <subscriptionId>");
                return;
            }

            if (!TryParseLink(args[1], out var subscriptionId))
            {
                _output($"Invalid subscription ID: {args[1]}");
                return;
            }

            var info = _persistentSearch.GetSubscriptionInfo(subscriptionId);
            if (info == null)
            {
                _output($"Subscription not found: {subscriptionId}");
                return;
            }

            _output($"Subscription {subscriptionId}:");
            _output($"  User: {info.UserId}");
            _output($"  Query: [{string.Join(", ", info.SearchQuery)}]");
            _output($"  Active: {info.IsActive}");
            _output($"  Created: {info.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
            _output($"  Results found: {info.FoundResults.Count}");

            if (info.FoundResults.Count > 0)
            {
                _output("  Found links:");
                foreach (var result in info.FoundResults)
                {
                    _output($"    - {result}: {_links.GetSource(result)} -> {_links.GetTarget(result)}");
                }
            }

            if (info.SatisfiedAt.HasValue)
            {
                _output($"  Satisfied: {info.SatisfiedAt:yyyy-MM-dd HH:mm:ss} UTC");
                if (info.SatisfiedWith.HasValue)
                {
                    _output($"  Satisfied with: {info.SatisfiedWith.Value}");
                }
            }
        }

        private bool TryParseLink(string input, out TLinkAddress result)
        {
            result = default;
            try
            {
                if (typeof(TLinkAddress) == typeof(ulong))
                {
                    if (ulong.TryParse(input, out var ulongValue))
                    {
                        result = (TLinkAddress)(object)ulongValue;
                        return true;
                    }
                }
                else if (typeof(TLinkAddress) == typeof(uint))
                {
                    if (uint.TryParse(input, out var uintValue))
                    {
                        result = (TLinkAddress)(object)uintValue;
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
