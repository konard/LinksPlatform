#!/usr/bin/env python3
"""
Unit tests for the Wikipedia Fact-Checking Bot
"""

import unittest
from unittest.mock import Mock, patch, MagicMock
from fact_checker import (
    WikipediaFactChecker,
    Citation,
    Reference,
    FactCheckResult
)


class TestWikipediaFactChecker(unittest.TestCase):
    """Test cases for WikipediaFactChecker class."""

    def setUp(self):
        """Set up test fixtures."""
        self.checker = WikipediaFactChecker(verbose=False)

    def test_extract_url(self):
        """Test URL extraction from reference content."""
        ref_content = "{{cite web|url=https://example.com|title=Test}}"
        url = self.checker._extract_url(ref_content)
        self.assertEqual(url, "https://example.com")

        # Test raw URL
        ref_content = "https://example.org/page"
        url = self.checker._extract_url(ref_content)
        self.assertEqual(url, "https://example.org/page")

        # Test no URL
        ref_content = "{{cite book|title=Test Book}}"
        url = self.checker._extract_url(ref_content)
        self.assertIsNone(url)

    def test_extract_title(self):
        """Test title extraction from reference content."""
        ref_content = "{{cite web|url=https://example.com|title=Test Title}}"
        title = self.checker._extract_title(ref_content)
        self.assertEqual(title, "Test Title")

        # Test no title
        ref_content = "{{cite web|url=https://example.com}}"
        title = self.checker._extract_title(ref_content)
        self.assertIsNone(title)

    def test_calculate_similarity(self):
        """Test text similarity calculation."""
        # Identical texts
        text1 = "The quick brown fox"
        text2 = "The quick brown fox"
        similarity = self.checker.calculate_similarity(text1, text2)
        self.assertEqual(similarity, 1.0)

        # Completely different texts
        text1 = "apple banana orange"
        text2 = "car truck motorcycle"
        similarity = self.checker.calculate_similarity(text1, text2)
        self.assertEqual(similarity, 0.0)

        # Partial overlap
        text1 = "apple banana orange"
        text2 = "apple pear grape"
        similarity = self.checker.calculate_similarity(text1, text2)
        self.assertGreater(similarity, 0.0)
        self.assertLess(similarity, 1.0)

    def test_extract_citations(self):
        """Test citation extraction from wikitext."""
        wikitext = '''
Some text before citation.<ref name="ref1">Reference content here</ref>

More text with another citation.<ref>{{cite web|url=https://example.com}}</ref>

Text with named reference use.<ref name="ref1" />
'''

        citations = self.checker.extract_citations(wikitext)

        # Should find citations
        self.assertGreater(len(citations), 0)

        # Check that citations have required fields
        for citation in citations:
            self.assertIsNotNone(citation.text)
            self.assertIsInstance(citation.position, int)

    def test_extract_references(self):
        """Test reference extraction from wikitext."""
        wikitext = '''
Some article text.<ref name="source1">{{cite web|url=https://example.com|title=Example}}</ref>

More text.<ref name="source2">Another reference</ref>
'''

        references = self.checker.extract_references(wikitext)

        # Should find references
        self.assertIn("source1", references)
        self.assertIn("source2", references)

        # Check reference properties
        self.assertEqual(references["source1"].name, "source1")
        self.assertEqual(references["source1"].url, "https://example.com")
        self.assertEqual(references["source1"].title, "Example")

    @patch('fact_checker.requests.Session.get')
    def test_fetch_article_success(self, mock_get):
        """Test successful article fetching."""
        mock_response = Mock()
        mock_response.status_code = 200
        mock_response.json.return_value = {
            'query': {
                'pages': [{
                    'revisions': [{
                        'slots': {
                            'main': {
                                'content': 'Article wikitext content'
                            }
                        }
                    }]
                }]
            }
        }
        mock_get.return_value = mock_response

        content = self.checker.fetch_article("Test Article")

        self.assertIsNotNone(content)
        self.assertEqual(content, 'Article wikitext content')

    @patch('fact_checker.requests.Session.get')
    def test_fetch_article_failure(self, mock_get):
        """Test article fetching failure."""
        mock_get.side_effect = Exception("Network error")

        content = self.checker.fetch_article("Test Article")

        self.assertIsNone(content)

    @patch('fact_checker.requests.Session.get')
    def test_fetch_reference_content(self, mock_get):
        """Test fetching content from reference URL."""
        mock_response = Mock()
        mock_response.status_code = 200
        mock_response.content = b'<html><body><p>Reference content here</p></body></html>'
        mock_get.return_value = mock_response

        content = self.checker.fetch_reference_content("https://example.com")

        self.assertIsNotNone(content)
        self.assertIn("Reference content here", content)

    def test_verify_citation_no_reference(self):
        """Test citation verification when reference is not found."""
        citation = Citation(
            text="Some claim in the article",
            ref_name="missing_ref",
            ref_content="",
            position=100
        )

        result = self.checker.verify_citation(citation, None)

        self.assertEqual(result.status, 'reference_not_found')
        self.assertEqual(result.similarity_score, 0.0)

    def test_verify_citation_no_url(self):
        """Test citation verification when reference has no URL."""
        citation = Citation(
            text="Some claim in the article",
            ref_name="ref1",
            ref_content="",
            position=100
        )

        reference = Reference(
            name="ref1",
            url=None,
            title="Test Title",
            content="Reference content"
        )

        result = self.checker.verify_citation(citation, reference)

        self.assertEqual(result.status, 'unverified')
        self.assertIn('no URL', result.details)

    @patch.object(WikipediaFactChecker, 'fetch_reference_content')
    def test_verify_citation_with_url(self, mock_fetch):
        """Test citation verification with URL."""
        citation = Citation(
            text="The capital of France is Paris",
            ref_name="ref1",
            ref_content="",
            position=100
        )

        reference = Reference(
            name="ref1",
            url="https://example.com",
            title="France Facts",
            content="Reference"
        )

        # Mock reference content that matches the citation
        mock_fetch.return_value = "Paris is the capital and largest city of France"

        result = self.checker.verify_citation(citation, reference)

        self.assertIn(result.status, ['verified', 'unverified'])
        self.assertIsInstance(result.similarity_score, float)
        self.assertGreaterEqual(result.similarity_score, 0.0)
        self.assertLessEqual(result.similarity_score, 1.0)

    def test_generate_report(self):
        """Test report generation."""
        citation = Citation(
            text="Test citation text",
            ref_name="ref1",
            ref_content="",
            position=0
        )

        reference = Reference(
            name="ref1",
            url="https://example.com",
            title="Test",
            content="Test content"
        )

        results = [
            FactCheckResult(
                citation=citation,
                reference=reference,
                status='verified',
                similarity_score=0.85,
                details='High similarity'
            )
        ]

        report = self.checker.generate_report(results)

        # Check report contains expected sections
        self.assertIn('FACT-CHECK REPORT', report)
        self.assertIn('SUMMARY', report)
        self.assertIn('verified', report.lower())
        self.assertIn('85', report)  # Similarity percentage


class TestDataClasses(unittest.TestCase):
    """Test cases for data classes."""

    def test_citation_creation(self):
        """Test Citation dataclass creation."""
        citation = Citation(
            text="Test text",
            ref_name="ref1",
            ref_content="Content",
            position=100
        )

        self.assertEqual(citation.text, "Test text")
        self.assertEqual(citation.ref_name, "ref1")
        self.assertEqual(citation.ref_content, "Content")
        self.assertEqual(citation.position, 100)

    def test_reference_creation(self):
        """Test Reference dataclass creation."""
        reference = Reference(
            name="ref1",
            url="https://example.com",
            title="Test Title",
            content="Full content"
        )

        self.assertEqual(reference.name, "ref1")
        self.assertEqual(reference.url, "https://example.com")
        self.assertEqual(reference.title, "Test Title")
        self.assertEqual(reference.content, "Full content")

    def test_fact_check_result_creation(self):
        """Test FactCheckResult dataclass creation."""
        citation = Citation("text", "ref", "content", 0)
        reference = Reference("ref", "url", "title", "content")

        result = FactCheckResult(
            citation=citation,
            reference=reference,
            status='verified',
            similarity_score=0.9,
            details='Test details'
        )

        self.assertEqual(result.status, 'verified')
        self.assertEqual(result.similarity_score, 0.9)
        self.assertEqual(result.details, 'Test details')


if __name__ == '__main__':
    unittest.main()
