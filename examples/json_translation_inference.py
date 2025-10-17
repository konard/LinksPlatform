#!/usr/bin/env python3
"""
Data Translation Rules Inference by Example

This module infers translation rules between source and target JSON structures
by analyzing example data. When the source schema changes but data values remain
the same, it can detect field mappings by matching data values.

Example:
    Source: {"name": "John", "age": 30}
    Target: {"first_name": "John", "years": 30}

    Later, source changes to: {"fullname": "John", "age": 30}
    The tool can infer that "fullname" maps to "first_name" by finding "John"
"""

import json
from typing import Any, Dict, List, Optional, Tuple
from dataclasses import dataclass


@dataclass
class FieldMapping:
    """Represents a mapping between source and target fields"""
    source_field: str
    target_field: str
    confidence: float  # 0.0 to 1.0
    matched_values: List[Any]


class JsonTranslationInference:
    """Infers translation rules between JSON structures using example data"""

    def __init__(self):
        self.field_mappings: List[FieldMapping] = []

    def _extract_paths_and_values(self, obj: Any, prefix: str = "") -> Dict[str, Any]:
        """Extract all paths and their values from a nested JSON structure"""
        paths = {}

        if isinstance(obj, dict):
            for key, value in obj.items():
                path = f"{prefix}.{key}" if prefix else key
                if isinstance(value, (dict, list)):
                    paths.update(self._extract_paths_and_values(value, path))
                else:
                    paths[path] = value
        elif isinstance(obj, list):
            for i, item in enumerate(obj):
                path = f"{prefix}[{i}]"
                if isinstance(item, (dict, list)):
                    paths.update(self._extract_paths_and_values(item, path))
                else:
                    paths[path] = item
        else:
            paths[prefix] = obj

        return paths

    def _find_value_in_paths(self, value: Any, paths: Dict[str, Any]) -> List[str]:
        """Find all paths that contain the given value"""
        matching_paths = []
        for path, path_value in paths.items():
            if path_value == value:
                matching_paths.append(path)
        return matching_paths

    def infer_mappings(
        self,
        source_json: Dict[str, Any],
        target_json: Dict[str, Any]
    ) -> List[FieldMapping]:
        """
        Infer field mappings between source and target JSON structures
        by matching data values.

        Args:
            source_json: Source JSON structure with data
            target_json: Target JSON structure with data

        Returns:
            List of FieldMapping objects representing inferred mappings
        """
        source_paths = self._extract_paths_and_values(source_json)
        target_paths = self._extract_paths_and_values(target_json)

        mappings = []
        used_target_fields = set()

        # For each source field, find matching target field by value
        for source_field, source_value in source_paths.items():
            # Skip None values as they're not distinctive
            if source_value is None:
                continue

            # Find target fields with matching values
            matching_target_fields = [
                target_field
                for target_field, target_value in target_paths.items()
                if target_value == source_value and target_field not in used_target_fields
            ]

            if matching_target_fields:
                # Use the first match (could be improved with better heuristics)
                target_field = matching_target_fields[0]
                used_target_fields.add(target_field)

                # Calculate confidence based on uniqueness
                confidence = 1.0 / len(matching_target_fields)

                mappings.append(FieldMapping(
                    source_field=source_field,
                    target_field=target_field,
                    confidence=confidence,
                    matched_values=[source_value]
                ))

        self.field_mappings = mappings
        return mappings

    def apply_mappings(
        self,
        new_source_json: Dict[str, Any],
        mappings: Optional[List[FieldMapping]] = None
    ) -> Dict[str, Any]:
        """
        Apply learned mappings to translate a new source JSON structure

        Args:
            new_source_json: New source JSON to translate
            mappings: Field mappings to apply (uses self.field_mappings if None)

        Returns:
            Translated JSON structure
        """
        if mappings is None:
            mappings = self.field_mappings

        new_source_paths = self._extract_paths_and_values(new_source_json)
        target_json = {}

        for mapping in mappings:
            if mapping.source_field in new_source_paths:
                value = new_source_paths[mapping.source_field]
                # Simple implementation: only handles flat structures
                # Could be extended to handle nested paths
                target_json[mapping.target_field] = value

        return target_json

    def update_mappings_with_schema_change(
        self,
        old_source_json: Dict[str, Any],
        new_source_json: Dict[str, Any],
        existing_mappings: List[FieldMapping]
    ) -> List[FieldMapping]:
        """
        Update mappings when source schema changes but values remain the same.
        Uses data values to find the new field names in the changed schema.

        Args:
            old_source_json: Source JSON with old schema
            new_source_json: Source JSON with new schema (same values, different fields)
            existing_mappings: Previously learned mappings

        Returns:
            Updated mappings with new source field names
        """
        old_paths = self._extract_paths_and_values(old_source_json)
        new_paths = self._extract_paths_and_values(new_source_json)

        updated_mappings = []

        for mapping in existing_mappings:
            old_value = old_paths.get(mapping.source_field)

            if old_value is None:
                continue

            # Find the new field that contains the same value
            new_fields = self._find_value_in_paths(old_value, new_paths)

            if new_fields:
                # Use the first match (could be improved with similarity matching)
                new_field = new_fields[0]
                updated_mappings.append(FieldMapping(
                    source_field=new_field,
                    target_field=mapping.target_field,
                    confidence=mapping.confidence,
                    matched_values=mapping.matched_values + [old_value]
                ))

        return updated_mappings

    def export_mappings_as_rules(self, format: str = "json") -> str:
        """
        Export learned mappings as translation rules

        Args:
            format: Output format ("json" or "readable")

        Returns:
            String representation of translation rules
        """
        if format == "json":
            rules = {
                "mappings": [
                    {
                        "source": m.source_field,
                        "target": m.target_field,
                        "confidence": m.confidence
                    }
                    for m in self.field_mappings
                ]
            }
            return json.dumps(rules, indent=2)
        else:  # readable format
            lines = ["Translation Rules:", "=" * 50]
            for i, mapping in enumerate(self.field_mappings, 1):
                lines.append(
                    f"{i}. {mapping.source_field} → {mapping.target_field} "
                    f"(confidence: {mapping.confidence:.2f})"
                )
            return "\n".join(lines)


def main():
    """Example usage demonstrating the translation inference"""

    # Example 1: Initial learning
    print("Example 1: Learning initial mappings\n")

    source = {
        "name": "John",
        "age": 30,
        "email": "john@example.com"
    }

    target = {
        "first_name": "John",
        "years": 30,
        "contact": "john@example.com"
    }

    inferencer = JsonTranslationInference()
    mappings = inferencer.infer_mappings(source, target)

    print("Learned mappings:")
    for mapping in mappings:
        print(f"  {mapping.source_field} → {mapping.target_field}")

    # Example 2: Applying learned mappings
    print("\n\nExample 2: Applying mappings to new data\n")

    new_source = {
        "name": "Jane",
        "age": 25,
        "email": "jane@example.com"
    }

    translated = inferencer.apply_mappings(new_source)
    print(f"Translated: {json.dumps(translated, indent=2)}")

    # Example 3: Schema change detection
    print("\n\nExample 3: Handling schema changes\n")

    old_source = {
        "name": "John",
        "age": 30,
        "email": "john@example.com"
    }

    new_source_schema = {
        "fullname": "John",  # Changed from "name" to "fullname"
        "age": 30,
        "email_address": "john@example.com"  # Changed from "email" to "email_address"
    }

    updated_mappings = inferencer.update_mappings_with_schema_change(
        old_source, new_source_schema, mappings
    )

    print("Updated mappings after schema change:")
    for mapping in updated_mappings:
        print(f"  {mapping.source_field} → {mapping.target_field}")

    # Example 4: Export rules
    print("\n\nExample 4: Exporting translation rules\n")
    print(inferencer.export_mappings_as_rules(format="readable"))


if __name__ == "__main__":
    main()
