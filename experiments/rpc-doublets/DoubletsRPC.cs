using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Sequences;
using Platform.Data.Doublets.Unicode;

namespace Platform.Examples.DoubletsRPC
{
    /// <summary>
    /// Implements Remote Procedure Call (RPC) mechanism over Doublets storage.
    /// Enables cross-language method invocation using Links as communication medium.
    /// </summary>
    /// <remarks>
    /// This is a conceptual reference implementation demonstrating the RPC protocol.
    /// Production use requires additional security, error handling, and optimization.
    /// </remarks>
    public class DoubletsRPC<TLinkAddress> where TLinkAddress : struct
    {
        private readonly ILinks<TLinkAddress> _links;
        private readonly IConverter<string, TLinkAddress> _stringToLink;
        private readonly IConverter<TLinkAddress, string> _linkToString;
        private readonly LinksConstants<TLinkAddress> _constants;

        // Marker links for RPC protocol
        private readonly TLinkAddress _rpcNamespaceMarker;
        private readonly TLinkAddress _methodMarker;
        private readonly TLinkAddress _callMarker;
        private readonly TLinkAddress _stateMarker;
        private readonly TLinkAddress _pendingMarker;
        private readonly TLinkAddress _processingMarker;
        private readonly TLinkAddress _completedMarker;
        private readonly TLinkAddress _failedMarker;
        private readonly TLinkAddress _resultMarker;

        // Method registry: MethodName -> Handler
        private readonly Dictionary<string, Func<object[], object>> _methodHandlers;

        public DoubletsRPC(
            ILinks<TLinkAddress> links,
            IConverter<string, TLinkAddress> stringToLink,
            IConverter<TLinkAddress, string> linkToString)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
            _stringToLink = stringToLink ?? throw new ArgumentNullException(nameof(stringToLink));
            _linkToString = linkToString ?? throw new ArgumentNullException(nameof(linkToString));
            _constants = links.Constants;
            _methodHandlers = new Dictionary<string, Func<object[], object>>();

            // Initialize RPC protocol markers
            _rpcNamespaceMarker = GetOrCreateMarker("RPC");
            _methodMarker = GetOrCreateMarker("RPC.Method");
            _callMarker = GetOrCreateMarker("RPC.Call");
            _stateMarker = GetOrCreateMarker("RPC.State");
            _pendingMarker = GetOrCreateMarker("RPC.State.Pending");
            _processingMarker = GetOrCreateMarker("RPC.State.Processing");
            _completedMarker = GetOrCreateMarker("RPC.State.Completed");
            _failedMarker = GetOrCreateMarker("RPC.State.Failed");
            _resultMarker = GetOrCreateMarker("RPC.Result");
        }

        #region Method Registration (Executor Side)

        /// <summary>
        /// Registers a method that can be called remotely.
        /// </summary>
        /// <param name="methodName">Name of the method</param>
        /// <param name="handler">Handler function that executes the method</param>
        public void RegisterMethod(string methodName, Func<object[], object> handler)
        {
            if (string.IsNullOrEmpty(methodName))
                throw new ArgumentException("Method name cannot be empty", nameof(methodName));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            _methodHandlers[methodName] = handler;

            // Create method registration link in storage
            var methodNameLink = _stringToLink.Convert(methodName);
            var methodRegistrationLink = _links.GetOrCreate(_methodMarker, methodNameLink);

            Console.WriteLine($"[RPC] Registered method: {methodName} (Link: {methodRegistrationLink})");
        }

        /// <summary>
        /// Processes all pending method calls.
        /// Should be called periodically by executor.
        /// </summary>
        /// <returns>Number of calls processed</returns>
        public int ProcessPendingCalls()
        {
            int processedCount = 0;

            // Find all pending calls: [CallMarker -> [MethodLink, ArgumentsLink]]
            var pendingCalls = FindCallsByState(_pendingMarker);

            foreach (var callLink in pendingCalls)
            {
                try
                {
                    ProcessSingleCall(callLink);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RPC] Error processing call {callLink}: {ex.Message}");
                    SetCallState(callLink, _failedMarker, SerializeError(ex));
                }
            }

            return processedCount;
        }

        private void ProcessSingleCall(TLinkAddress callLink)
        {
            // Update state to Processing
            SetCallState(callLink, _processingMarker, _constants.Null);

            // Extract method name and arguments
            var callData = _links.GetLink(callLink);
            var methodLink = _links.GetSource(callData);
            var argumentsLink = _links.GetTarget(callData);

            // Get method name
            var methodRegistration = _links.GetLink(methodLink);
            var methodNameLink = _links.GetTarget(methodRegistration);
            var methodName = _linkToString.Convert(methodNameLink);

            // Check if method is registered
            if (!_methodHandlers.ContainsKey(methodName))
            {
                throw new InvalidOperationException($"Method not registered: {methodName}");
            }

            // Deserialize arguments
            var arguments = DeserializeArguments(argumentsLink);

            // Execute method
            var handler = _methodHandlers[methodName];
            var result = handler(arguments);

            // Serialize and store result
            var resultLink = SerializeResult(result);

            // Update state to Completed with result
            SetCallState(callLink, _completedMarker, resultLink);

            Console.WriteLine($"[RPC] Executed: {methodName} -> Result: {callLink}");
        }

        #endregion

        #region Method Invocation (Caller Side)

        /// <summary>
        /// Calls a remote method and waits for result.
        /// </summary>
        /// <typeparam name="TResult">Expected return type</typeparam>
        /// <param name="methodName">Name of the method to call</param>
        /// <param name="args">Method arguments</param>
        /// <param name="timeoutMs">Timeout in milliseconds (0 = no timeout)</param>
        /// <returns>Method result</returns>
        public TResult CallMethod<TResult>(string methodName, object[] args, int timeoutMs = 5000)
        {
            var callLink = CreateCall(methodName, args);
            return WaitForResult<TResult>(callLink, timeoutMs);
        }

        /// <summary>
        /// Creates a method call without waiting for result.
        /// </summary>
        /// <param name="methodName">Name of the method to call</param>
        /// <param name="args">Method arguments</param>
        /// <returns>Call link that can be used to retrieve result later</returns>
        public TLinkAddress CreateCall(string methodName, params object[] args)
        {
            // Get or create method link
            var methodNameLink = _stringToLink.Convert(methodName);
            var methodLink = _links.GetOrCreate(_methodMarker, methodNameLink);

            // Serialize arguments
            var argumentsLink = SerializeArguments(args);

            // Create call metadata: [MethodLink, ArgumentsLink]
            var callMetadataLink = _links.Create(methodLink, argumentsLink);

            // Create call with initial state: [CallMarker -> CallMetadata]
            var callLink = _links.Create(_callMarker, callMetadataLink);

            // Set initial state to Pending
            SetCallState(callLink, _pendingMarker, _constants.Null);

            Console.WriteLine($"[RPC] Created call: {methodName} (CallLink: {callLink})");

            return callLink;
        }

        /// <summary>
        /// Waits for call to complete and returns result.
        /// </summary>
        /// <typeparam name="TResult">Expected return type</typeparam>
        /// <param name="callLink">Call link from CreateCall</param>
        /// <param name="timeoutMs">Timeout in milliseconds</param>
        /// <returns>Method result</returns>
        public TResult WaitForResult<TResult>(TLinkAddress callLink, int timeoutMs = 5000)
        {
            var startTime = DateTime.UtcNow;

            while (true)
            {
                var state = GetCallState(callLink, out var resultLink);

                if (Equals(state, _completedMarker))
                {
                    return DeserializeResult<TResult>(resultLink);
                }

                if (Equals(state, _failedMarker))
                {
                    var error = DeserializeError(resultLink);
                    throw new InvalidOperationException($"Remote call failed: {error}");
                }

                if (timeoutMs > 0 && (DateTime.UtcNow - startTime).TotalMilliseconds > timeoutMs)
                {
                    throw new TimeoutException($"Call timed out after {timeoutMs}ms");
                }

                System.Threading.Thread.Sleep(10); // Polling interval
            }
        }

        #endregion

        #region State Management

        private void SetCallState(TLinkAddress callLink, TLinkAddress stateMarker, TLinkAddress resultLink)
        {
            // Create state link: [StateMarker -> Result]
            var stateLink = _links.GetOrCreate(stateMarker, resultLink);

            // Create or update: [CallLink -> StateLink]
            var query = new Link<TLinkAddress>(
                _constants.Any,
                callLink,
                _constants.Any
            );

            var existingStateLinks = new List<TLinkAddress>();
            _links.Each(link => {
                existingStateLinks.Add(link);
                return _constants.Continue;
            }, query);

            // Delete old state links
            foreach (var oldStateLink in existingStateLinks)
            {
                var oldLink = _links.GetLink(oldStateLink);
                var oldTarget = _links.GetTarget(oldLink);
                var oldTargetLink = _links.GetLink(oldTarget);
                var oldState = _links.GetSource(oldTargetLink);

                // Only delete if it's a state link
                if (Equals(oldState, _pendingMarker) ||
                    Equals(oldState, _processingMarker) ||
                    Equals(oldState, _completedMarker) ||
                    Equals(oldState, _failedMarker))
                {
                    _links.Delete(oldStateLink);
                }
            }

            // Create new state link
            _links.Create(callLink, stateLink);
        }

        private TLinkAddress GetCallState(TLinkAddress callLink, out TLinkAddress resultLink)
        {
            resultLink = _constants.Null;

            var query = new Link<TLinkAddress>(
                _constants.Any,
                callLink,
                _constants.Any
            );

            TLinkAddress stateMarker = _constants.Null;

            _links.Each(link => {
                var linkData = _links.GetLink(link);
                var stateLink = _links.GetTarget(linkData);
                var stateLinkData = _links.GetLink(stateLink);
                stateMarker = _links.GetSource(stateLinkData);
                resultLink = _links.GetTarget(stateLinkData);
                return _constants.Break; // Take first match
            }, query);

            return Equals(stateMarker, _constants.Null) ? _pendingMarker : stateMarker;
        }

        private List<TLinkAddress> FindCallsByState(TLinkAddress stateMarker)
        {
            var calls = new List<TLinkAddress>();

            // This is a simplified search - production would use indices
            _links.Each(link => {
                var linkData = _links.GetLink(link);
                var source = _links.GetSource(linkData);

                if (Equals(source, _callMarker))
                {
                    var currentState = GetCallState(link, out _);
                    if (Equals(currentState, stateMarker))
                    {
                        calls.Add(link);
                    }
                }

                return _constants.Continue;
            });

            return calls;
        }

        #endregion

        #region Serialization (Simplified)

        private TLinkAddress SerializeArguments(object[] args)
        {
            if (args == null || args.Length == 0)
                return _constants.Null;

            // Simplified: just serialize as string sequence for now
            var argsString = string.Join(",", args.Select(a => a?.ToString() ?? "null"));
            return _stringToLink.Convert(argsString);
        }

        private object[] DeserializeArguments(TLinkAddress argumentsLink)
        {
            if (Equals(argumentsLink, _constants.Null))
                return Array.Empty<object>();

            // Simplified: deserialize from string
            var argsString = _linkToString.Convert(argumentsLink);
            return argsString.Split(',').Select(s =>
                s == "null" ? null : (object)s
            ).ToArray();
        }

        private TLinkAddress SerializeResult(object result)
        {
            if (result == null)
                return _constants.Null;

            // Simplified: serialize as string
            return _stringToLink.Convert(result.ToString());
        }

        private TResult DeserializeResult<TResult>(TLinkAddress resultLink)
        {
            if (Equals(resultLink, _constants.Null))
                return default;

            var resultString = _linkToString.Convert(resultLink);
            return (TResult)Convert.ChangeType(resultString, typeof(TResult));
        }

        private TLinkAddress SerializeError(Exception ex)
        {
            return _stringToLink.Convert($"Error: {ex.Message}");
        }

        private string DeserializeError(TLinkAddress errorLink)
        {
            return _linkToString.Convert(errorLink);
        }

        #endregion

        #region Helpers

        private TLinkAddress GetOrCreateMarker(string markerName)
        {
            var markerNameLink = _stringToLink.Convert(markerName);
            return _links.GetOrCreate(markerNameLink, markerNameLink);
        }

        #endregion
    }
}
