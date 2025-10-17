#!/usr/bin/env python3
"""
Unit tests for JSON Translation Rules Inference

Tests the functionality of the JsonTranslationInference class
"""

import unittest
import json
from json_translation_inference import JsonTranslationInference, FieldMapping


class TestJsonTranslationInference(unittest.TestCase):
    """Test cases for JSON translation inference"""

    def setUp(self):
        """Set up test fixtures"""
        self.inferencer = JsonTranslationInference()

    def test_extract_paths_flat(self):
        """Test path extraction for flat JSON structures"""
        obj = {"name": "John", "age": 30}
        paths = self.inferencer._extract_paths_and_values(obj)

        self.assertEqual(len(paths), 2)
        self.assertEqual(paths["name"], "John")
        self.assertEqual(paths["age"], 30)

    def test_extract_paths_nested(self):
        """Test path extraction for nested JSON structures"""
        obj = {
            "user": {
                "name": "John",
                "profile": {
                    "age": 30
                }
            }
        }
        paths = self.inferencer._extract_paths_and_values(obj)

        self.assertIn("user.name", paths)
        self.assertIn("user.profile.age", paths)
        self.assertEqual(paths["user.name"], "John")
        self.assertEqual(paths["user.profile.age"], 30)

    def test_extract_paths_with_arrays(self):
        """Test path extraction for JSON with arrays"""
        obj = {
            "users": [
                {"name": "John"},
                {"name": "Jane"}
            ]
        }
        paths = self.inferencer._extract_paths_and_values(obj)

        self.assertIn("users[0].name", paths)
        self.assertIn("users[1].name", paths)
        self.assertEqual(paths["users[0].name"], "John")
        self.assertEqual(paths["users[1].name"], "Jane")

    def test_infer_simple_mappings(self):
        """Test basic mapping inference"""
        source = {"name": "John", "age": 30}
        target = {"first_name": "John", "years": 30}

        mappings = self.inferencer.infer_mappings(source, target)

        self.assertEqual(len(mappings), 2)

        # Find mappings by source field
        name_mapping = next(m for m in mappings if m.source_field == "name")
        age_mapping = next(m for m in mappings if m.source_field == "age")

        self.assertEqual(name_mapping.target_field, "first_name")
        self.assertEqual(age_mapping.target_field, "years")
        self.assertEqual(name_mapping.confidence, 1.0)
        self.assertEqual(age_mapping.confidence, 1.0)

    def test_infer_with_duplicate_values(self):
        """Test mapping inference with duplicate values"""
        source = {"field1": "value", "field2": "value"}
        target = {"target1": "value", "target2": "value"}

        mappings = self.inferencer.infer_mappings(source, target)

        # Should still create mappings but with lower confidence
        self.assertEqual(len(mappings), 2)
        self.assertLessEqual(mappings[0].confidence, 1.0)

    def test_infer_with_none_values(self):
        """Test that None values are skipped"""
        source = {"name": "John", "email": None}
        target = {"first_name": "John", "contact": None}

        mappings = self.inferencer.infer_mappings(source, target)

        # Should only map "name" to "first_name", not the None values
        self.assertEqual(len(mappings), 1)
        self.assertEqual(mappings[0].source_field, "name")
        self.assertEqual(mappings[0].target_field, "first_name")

    def test_apply_mappings(self):
        """Test applying learned mappings to new data"""
        source = {"name": "John", "age": 30}
        target = {"first_name": "John", "years": 30}

        self.inferencer.infer_mappings(source, target)

        new_source = {"name": "Jane", "age": 25}
        translated = self.inferencer.apply_mappings(new_source)

        self.assertEqual(translated["first_name"], "Jane")
        self.assertEqual(translated["years"], 25)

    def test_apply_mappings_with_missing_fields(self):
        """Test applying mappings when source is missing some fields"""
        source = {"name": "John", "age": 30, "email": "john@example.com"}
        target = {"first_name": "John", "years": 30, "contact": "john@example.com"}

        self.inferencer.infer_mappings(source, target)

        # New source is missing the email field
        new_source = {"name": "Jane", "age": 25}
        translated = self.inferencer.apply_mappings(new_source)

        self.assertEqual(translated["first_name"], "Jane")
        self.assertEqual(translated["years"], 25)
        self.assertNotIn("contact", translated)

    def test_update_mappings_with_schema_change(self):
        """Test updating mappings when source schema changes"""
        old_source = {"name": "John", "age": 30}
        target = {"first_name": "John", "years": 30}

        initial_mappings = self.inferencer.infer_mappings(old_source, target)

        # Schema changes: "name" becomes "fullname"
        new_source = {"fullname": "John", "age": 30}

        updated_mappings = self.inferencer.update_mappings_with_schema_change(
            old_source, new_source, initial_mappings
        )

        self.assertEqual(len(updated_mappings), 2)

        # Find the updated name mapping
        name_mapping = next(m for m in updated_mappings if "name" in m.source_field.lower())
        age_mapping = next(m for m in updated_mappings if m.source_field == "age")

        self.assertEqual(name_mapping.source_field, "fullname")
        self.assertEqual(name_mapping.target_field, "first_name")
        self.assertEqual(age_mapping.source_field, "age")
        self.assertEqual(age_mapping.target_field, "years")

    def test_update_mappings_multiple_schema_changes(self):
        """Test updating mappings with multiple field name changes"""
        old_source = {"name": "John", "age": 30, "email": "john@example.com"}
        target = {"first_name": "John", "years": 30, "contact": "john@example.com"}

        initial_mappings = self.inferencer.infer_mappings(old_source, target)

        # Multiple schema changes
        new_source = {
            "fullname": "John",
            "years_old": 30,
            "email_address": "john@example.com"
        }

        updated_mappings = self.inferencer.update_mappings_with_schema_change(
            old_source, new_source, initial_mappings
        )

        self.assertEqual(len(updated_mappings), 3)

        # Verify all mappings were updated
        source_fields = {m.source_field for m in updated_mappings}
        self.assertIn("fullname", source_fields)
        self.assertIn("years_old", source_fields)
        self.assertIn("email_address", source_fields)

    def test_export_mappings_json(self):
        """Test exporting mappings in JSON format"""
        source = {"name": "John", "age": 30}
        target = {"first_name": "John", "years": 30}

        self.inferencer.infer_mappings(source, target)
        exported = self.inferencer.export_mappings_as_rules(format="json")

        # Verify it's valid JSON
        rules = json.loads(exported)
        self.assertIn("mappings", rules)
        self.assertEqual(len(rules["mappings"]), 2)

    def test_export_mappings_readable(self):
        """Test exporting mappings in readable format"""
        source = {"name": "John", "age": 30}
        target = {"first_name": "John", "years": 30}

        self.inferencer.infer_mappings(source, target)
        exported = self.inferencer.export_mappings_as_rules(format="readable")

        # Verify readable format contains expected elements
        self.assertIn("Translation Rules:", exported)
        self.assertIn("→", exported)
        self.assertIn("confidence:", exported)

    def test_complex_nested_structure(self):
        """Test with more complex nested JSON structures"""
        source = {
            "user": {
                "personal": {
                    "name": "John",
                    "age": 30
                },
                "contact": {
                    "email": "john@example.com"
                }
            }
        }

        target = {
            "person": {
                "info": {
                    "fullname": "John",
                    "years": 30
                },
                "communications": {
                    "mail": "john@example.com"
                }
            }
        }

        mappings = self.inferencer.infer_mappings(source, target)

        # Verify nested paths are correctly mapped
        self.assertEqual(len(mappings), 3)

        # Check that all source paths have corresponding target paths
        source_fields = {m.source_field for m in mappings}
        self.assertIn("user.personal.name", source_fields)
        self.assertIn("user.personal.age", source_fields)
        self.assertIn("user.contact.email", source_fields)


class TestFieldMapping(unittest.TestCase):
    """Test the FieldMapping dataclass"""

    def test_field_mapping_creation(self):
        """Test creating a FieldMapping object"""
        mapping = FieldMapping(
            source_field="name",
            target_field="first_name",
            confidence=1.0,
            matched_values=["John"]
        )

        self.assertEqual(mapping.source_field, "name")
        self.assertEqual(mapping.target_field, "first_name")
        self.assertEqual(mapping.confidence, 1.0)
        self.assertEqual(mapping.matched_values, ["John"])


def run_tests():
    """Run all tests and print results"""
    loader = unittest.TestLoader()
    suite = loader.loadTestsFromModule(__import__(__name__))
    runner = unittest.TextTestRunner(verbosity=2)
    result = runner.run(suite)

    return result.wasSuccessful()


if __name__ == "__main__":
    import sys
    success = run_tests()
    sys.exit(0 if success else 1)
