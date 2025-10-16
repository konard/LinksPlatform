using System;
using System.Collections.Generic;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Numbers;

namespace Platform.Examples
{
    /// <summary>
    /// A real-life application example that demonstrates tracking resource transactions
    /// between users in different cities using the Links Platform.
    ///
    /// This example models:
    /// - Users (humans or bots)
    /// - Cities/Communities
    /// - Resources
    /// - Transactions (transfer of resources between users)
    /// - Timestamps
    /// </summary>
    public class ResourceTransactionTracker
    {
        private readonly ILinks<ulong> _links;
        private readonly Dictionary<string, ulong> _entities;
        private ulong _userType;
        private ulong _cityType;
        private ulong _resourceType;
        private ulong _transactionType;
        private ulong _timestampType;
        private ulong _meaningRoot;

        public ResourceTransactionTracker(ILinks<ulong> links)
        {
            _links = links;
            _entities = new Dictionary<string, ulong>();
            InitializeTypes();
        }

        private void InitializeTypes()
        {
            // Create a meaning root marker
            var markerIndex = _links.Count() + 1;
            _meaningRoot = _links.GetOrCreate(markerIndex, markerIndex);

            // Create fundamental types relative to meaning root
            _userType = _links.GetOrCreate(_meaningRoot, Arithmetic.Increment(markerIndex));
            _cityType = _links.GetOrCreate(_meaningRoot, Arithmetic.Increment(markerIndex));
            _resourceType = _links.GetOrCreate(_meaningRoot, Arithmetic.Increment(markerIndex));
            _transactionType = _links.GetOrCreate(_meaningRoot, Arithmetic.Increment(markerIndex));
            _timestampType = _links.GetOrCreate(_meaningRoot, Arithmetic.Increment(markerIndex));
        }

        public ulong CreateUser(string userName, string cityName)
        {
            var city = GetOrCreateCity(cityName);
            var user = _links.GetOrCreate(_userType, city);

            _entities[userName] = user;
            return user;
        }

        public ulong CreateResource(string resourceName, decimal amount)
        {
            // Create resource linked to resource type
            var amountMarker = (ulong)(amount * 100); // Simplified amount encoding
            var resource = _links.GetOrCreate(_resourceType, amountMarker);

            _entities[resourceName] = resource;
            return resource;
        }

        public ulong CreateTransaction(string fromUser, string toUser, string resourceName, decimal amount, DateTime timestamp)
        {
            if (!_entities.ContainsKey(fromUser) || !_entities.ContainsKey(toUser) || !_entities.ContainsKey(resourceName))
            {
                throw new ArgumentException("User or resource not found");
            }

            var fromUserLink = _entities[fromUser];
            var toUserLink = _entities[toUser];
            var resourceLink = _entities[resourceName];

            // Create transfer link: from -> to
            var transferLink = _links.GetOrCreate(fromUserLink, toUserLink);

            // Create transaction: transaction type -> transfer
            var transaction = _links.GetOrCreate(_transactionType, transferLink);

            // Link resource to transaction
            _links.GetOrCreate(transaction, resourceLink);

            // Add timestamp (simplified)
            var timestampMarker = (ulong)timestamp.Ticks;
            var timestampLink = _links.GetOrCreate(_timestampType, timestampMarker);
            _links.GetOrCreate(transaction, timestampLink);

            return transaction;
        }

        public int GetUserTransactionCount(string userName)
        {
            if (!_entities.ContainsKey(userName))
            {
                throw new ArgumentException("User not found");
            }

            var user = _entities[userName];
            int count = 0;

            // Count all transactions involving this user
            for (var i = _meaningRoot; i <= _links.Count(); i++)
            {
                var source = _links.GetSource(i);
                var target = _links.GetTarget(i);

                if (source == user || target == user)
                {
                    count++;
                }
            }

            return count;
        }

        private ulong GetOrCreateCity(string cityName)
        {
            if (_entities.ContainsKey(cityName))
            {
                return _entities[cityName];
            }

            var city = _links.GetOrCreate(_cityType, _links.Count() + 1);
            _entities[cityName] = city;
            return city;
        }

        public void PrintStatistics()
        {
            Console.WriteLine($"Total links: {_links.Count()}");
            Console.WriteLine($"Users created: {CountEntitiesOfType(_userType)}");
            Console.WriteLine($"Cities created: {CountEntitiesOfType(_cityType)}");
            Console.WriteLine($"Resources created: {CountEntitiesOfType(_resourceType)}");
            Console.WriteLine($"Transactions created: {CountEntitiesOfType(_transactionType)}");
        }

        private int CountEntitiesOfType(ulong typeLink)
        {
            int count = 0;
            for (var i = _meaningRoot; i <= _links.Count(); i++)
            {
                if (_links.GetSource(i) == typeLink)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
