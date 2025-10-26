"""
Doublets RPC - Python Implementation

This module provides a Python implementation of RPC over Doublets storage,
enabling cross-language method invocation.

Note: This is a conceptual reference implementation. Production use requires:
- Actual Doublets Python bindings
- Proper type marshalling
- Error handling
- Security features
"""

import time
from typing import Any, Callable, Dict, List, Optional, Tuple
from dataclasses import dataclass


@dataclass
class Link:
    """Represents a Doublet link with source and target."""
    index: int
    source: int
    target: int


class DoubletsLinks:
    """
    Simplified interface to Doublets storage.

    In production, this would be replaced with actual Platform.Data.Doublets
    Python bindings.
    """

    def __init__(self, filepath: str):
        self.filepath = filepath
        self.links: List[Link] = []
        self.next_index = 1
        self.constants_null = 0
        self.constants_any = -1
        self.constants_continue = True
        self.constants_break = False

    def create(self, source: int, target: int) -> int:
        """Creates a new link."""
        index = self.next_index
        self.next_index += 1
        self.links.append(Link(index, source, target))
        return index

    def get_or_create(self, source: int, target: int) -> int:
        """Gets existing link or creates new one."""
        for link in self.links:
            if link.source == source and link.target == target:
                return link.index
        return self.create(source, target)

    def get_link(self, index: int) -> Optional[Link]:
        """Retrieves link by index."""
        for link in self.links:
            if link.index == index:
                return link
        return None

    def get_source(self, link_or_index) -> int:
        """Gets source of a link."""
        if isinstance(link_or_index, Link):
            return link_or_index.source
        link = self.get_link(link_or_index)
        return link.source if link else self.constants_null

    def get_target(self, link_or_index) -> int:
        """Gets target of a link."""
        if isinstance(link_or_index, Link):
            return link_or_index.target
        link = self.get_link(link_or_index)
        return link.target if link else self.constants_null

    def delete(self, index: int) -> None:
        """Deletes a link."""
        self.links = [link for link in self.links if link.index != index]

    def each(self, handler: Callable[[int], bool], query = None) -> None:
        """Iterates over links matching query."""
        for link in self.links:
            if query:
                # Simple query matching
                if query.get('source') and query['source'] != self.constants_any:
                    if link.source != query['source']:
                        continue
                if query.get('target') and query['target'] != self.constants_any:
                    if link.target != query['target']:
                        continue

            if not handler(link.index):
                break


class DoubletsRPC:
    """
    Remote Procedure Call implementation over Doublets storage.

    Enables cross-language method invocation using Links as communication medium.
    """

    def __init__(self, filepath: str):
        self.links = DoubletsLinks(filepath)
        self.method_handlers: Dict[str, Callable] = {}

        # Initialize RPC protocol markers
        self._rpc_namespace_marker = self._get_or_create_marker("RPC")
        self._method_marker = self._get_or_create_marker("RPC.Method")
        self._call_marker = self._get_or_create_marker("RPC.Call")
        self._state_marker = self._get_or_create_marker("RPC.State")
        self._pending_marker = self._get_or_create_marker("RPC.State.Pending")
        self._processing_marker = self._get_or_create_marker("RPC.State.Processing")
        self._completed_marker = self._get_or_create_marker("RPC.State.Completed")
        self._failed_marker = self._get_or_create_marker("RPC.State.Failed")
        self._result_marker = self._get_or_create_marker("RPC.Result")

    def register(self, method_name: str) -> Callable:
        """
        Decorator to register a method for remote calls.

        Example:
            @rpc.register("CalculateSum")
            def calculate_sum(a: int, b: int) -> int:
                return a + b
        """
        def decorator(func: Callable) -> Callable:
            self.register_method(method_name, func)
            return func
        return decorator

    def register_method(self, method_name: str, handler: Callable) -> None:
        """
        Registers a method that can be called remotely.

        Args:
            method_name: Name of the method
            handler: Function to execute when method is called
        """
        if not method_name:
            raise ValueError("Method name cannot be empty")
        if not callable(handler):
            raise ValueError("Handler must be callable")

        self.method_handlers[method_name] = handler

        # Create method registration link in storage
        method_name_link = self._string_to_link(method_name)
        method_registration_link = self.links.get_or_create(
            self._method_marker,
            method_name_link
        )

        print(f"[RPC] Registered method: {method_name} (Link: {method_registration_link})")

    def process_pending_calls(self) -> int:
        """
        Processes all pending method calls.
        Should be called periodically by executor.

        Returns:
            Number of calls processed
        """
        processed_count = 0

        # Find all pending calls
        pending_calls = self._find_calls_by_state(self._pending_marker)

        for call_link in pending_calls:
            try:
                self._process_single_call(call_link)
                processed_count += 1
            except Exception as ex:
                print(f"[RPC] Error processing call {call_link}: {ex}")
                self._set_call_state(
                    call_link,
                    self._failed_marker,
                    self._serialize_error(str(ex))
                )

        return processed_count

    def _process_single_call(self, call_link: int) -> None:
        """Processes a single method call."""
        # Update state to Processing
        self._set_call_state(call_link, self._processing_marker, self.links.constants_null)

        # Extract method name and arguments
        call_data = self.links.get_link(call_link)
        method_link = self.links.get_source(call_data)
        arguments_link = self.links.get_target(call_data)

        # Get method name
        method_registration = self.links.get_link(method_link)
        method_name_link = self.links.get_target(method_registration)
        method_name = self._link_to_string(method_name_link)

        # Check if method is registered
        if method_name not in self.method_handlers:
            raise ValueError(f"Method not registered: {method_name}")

        # Deserialize arguments
        arguments = self._deserialize_arguments(arguments_link)

        # Execute method
        handler = self.method_handlers[method_name]
        result = handler(*arguments)

        # Serialize and store result
        result_link = self._serialize_result(result)

        # Update state to Completed with result
        self._set_call_state(call_link, self._completed_marker, result_link)

        print(f"[RPC] Executed: {method_name} -> Result: {call_link}")

    def create_call(self, method_name: str, *args) -> int:
        """
        Creates a method call without waiting for result.

        Args:
            method_name: Name of the method to call
            *args: Method arguments

        Returns:
            Call link that can be used to retrieve result later
        """
        # Get or create method link
        method_name_link = self._string_to_link(method_name)
        method_link = self.links.get_or_create(self._method_marker, method_name_link)

        # Serialize arguments
        arguments_link = self._serialize_arguments(args)

        # Create call metadata
        call_metadata_link = self.links.create(method_link, arguments_link)

        # Create call with initial state
        call_link = self.links.create(self._call_marker, call_metadata_link)

        # Set initial state to Pending
        self._set_call_state(call_link, self._pending_marker, self.links.constants_null)

        print(f"[RPC] Created call: {method_name} (CallLink: {call_link})")

        return call_link

    def wait_for_result(self, call_link: int, timeout_ms: int = 5000) -> Any:
        """
        Waits for call to complete and returns result.

        Args:
            call_link: Call link from create_call
            timeout_ms: Timeout in milliseconds

        Returns:
            Method result
        """
        start_time = time.time()

        while True:
            state, result_link = self._get_call_state(call_link)

            if state == self._completed_marker:
                return self._deserialize_result(result_link)

            if state == self._failed_marker:
                error = self._deserialize_error(result_link)
                raise RuntimeError(f"Remote call failed: {error}")

            if timeout_ms > 0:
                elapsed_ms = (time.time() - start_time) * 1000
                if elapsed_ms > timeout_ms:
                    raise TimeoutError(f"Call timed out after {timeout_ms}ms")

            time.sleep(0.01)  # Polling interval

    def call_method(self, method_name: str, *args, timeout_ms: int = 5000) -> Any:
        """
        Calls a remote method and waits for result.

        Args:
            method_name: Name of the method to call
            *args: Method arguments
            timeout_ms: Timeout in milliseconds

        Returns:
            Method result
        """
        call_link = self.create_call(method_name, *args)
        return self.wait_for_result(call_link, timeout_ms)

    # State Management

    def _set_call_state(self, call_link: int, state_marker: int, result_link: int) -> None:
        """Sets the state of a call."""
        # Create state link: [StateMarker -> Result]
        state_link = self.links.get_or_create(state_marker, result_link)

        # Find and delete old state links
        def check_and_delete(link_index: int) -> bool:
            link = self.links.get_link(link_index)
            if self.links.get_source(link) == call_link:
                target = self.links.get_target(link)
                target_link = self.links.get_link(target)
                if target_link:
                    state = self.links.get_source(target_link)
                    if state in [self._pending_marker, self._processing_marker,
                                self._completed_marker, self._failed_marker]:
                        self.links.delete(link_index)
            return self.links.constants_continue

        self.links.each(check_and_delete)

        # Create new state link
        self.links.create(call_link, state_link)

    def _get_call_state(self, call_link: int) -> Tuple[int, int]:
        """Gets the current state of a call."""
        state_marker = self.links.constants_null
        result_link = self.links.constants_null

        def find_state(link_index: int) -> bool:
            nonlocal state_marker, result_link
            link = self.links.get_link(link_index)
            if self.links.get_source(link) == call_link:
                state_link_index = self.links.get_target(link)
                state_link = self.links.get_link(state_link_index)
                if state_link:
                    state_marker = state_link.source
                    result_link = state_link.target
                    return self.links.constants_break
            return self.links.constants_continue

        self.links.each(find_state)

        if state_marker == self.links.constants_null:
            state_marker = self._pending_marker

        return state_marker, result_link

    def _find_calls_by_state(self, state_marker: int) -> List[int]:
        """Finds all calls with given state."""
        calls = []

        def check_call(link_index: int) -> bool:
            link = self.links.get_link(link_index)
            if self.links.get_source(link) == self._call_marker:
                current_state, _ = self._get_call_state(link_index)
                if current_state == state_marker:
                    calls.append(link_index)
            return self.links.constants_continue

        self.links.each(check_call)

        return calls

    # Serialization (Simplified)

    def _serialize_arguments(self, args: tuple) -> int:
        """Serializes method arguments."""
        if not args:
            return self.links.constants_null

        # Simplified: serialize as comma-separated string
        args_string = ",".join(str(arg) if arg is not None else "null" for arg in args)
        return self._string_to_link(args_string)

    def _deserialize_arguments(self, arguments_link: int) -> List[Any]:
        """Deserializes method arguments."""
        if arguments_link == self.links.constants_null:
            return []

        # Simplified: deserialize from string
        args_string = self._link_to_string(arguments_link)
        return [None if s == "null" else s for s in args_string.split(",")]

    def _serialize_result(self, result: Any) -> int:
        """Serializes method result."""
        if result is None:
            return self.links.constants_null
        return self._string_to_link(str(result))

    def _deserialize_result(self, result_link: int) -> Any:
        """Deserializes method result."""
        if result_link == self.links.constants_null:
            return None
        return self._link_to_string(result_link)

    def _serialize_error(self, error: str) -> int:
        """Serializes an error message."""
        return self._string_to_link(f"Error: {error}")

    def _deserialize_error(self, error_link: int) -> str:
        """Deserializes an error message."""
        return self._link_to_string(error_link)

    # Helpers

    def _get_or_create_marker(self, marker_name: str) -> int:
        """Creates or retrieves a marker link."""
        marker_name_link = self._string_to_link(marker_name)
        return self.links.get_or_create(marker_name_link, marker_name_link)

    def _string_to_link(self, text: str) -> int:
        """
        Converts string to link.

        In production, this would use actual UnicodeSequences from Doublets.
        For now, simplified by creating a unique link for each string.
        """
        # Simple hash-based approach for demonstration
        return abs(hash(text)) % 1000000

    def _link_to_string(self, link: int) -> str:
        """
        Converts link to string.

        In production, this would decode UnicodeSequences from Doublets.
        For now, simplified by returning string representation.
        """
        # In real implementation, would traverse sequence and decode Unicode
        return f"Link_{link}"


# Example usage
if __name__ == "__main__":
    # Create RPC instance
    rpc = DoubletsRPC("example.links")

    # Register methods
    @rpc.register("Add")
    def add(a, b):
        return int(a) + int(b)

    @rpc.register("Multiply")
    def multiply(a, b):
        return int(a) * int(b)

    @rpc.register("Greet")
    def greet(name):
        return f"Hello, {name}!"

    print("[Example] Methods registered")
    print("[Example] Processing calls every second...")
    print("[Example] (In real scenario, C# or other language would create calls)")

    # Simulate processing loop
    while True:
        count = rpc.process_pending_calls()
        if count > 0:
            print(f"[Example] Processed {count} calls")
        time.sleep(1)
