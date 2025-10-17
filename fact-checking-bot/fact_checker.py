#!/usr/bin/env python3
"""
Fact-Checking Bot for Wikipedia Articles

This bot scans Wikipedia articles to verify that references actually contain
the facts cited in the article text. It helps identify potential errors where
a reference does not support the claim it's supposed to verify.
"""

import re
import requests
from typing import List, Dict, Tuple, Optional
from urllib.parse import urlparse, unquote
import logging
from dataclasses import dataclass
from bs4 import BeautifulSoup
import mwparserfromhell

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)


@dataclass
class Citation:
    """Represents a citation in a Wikipedia article."""
    text: str  # The text being cited
    ref_name: Optional[str]  # Reference name if it has one
    ref_content: str  # Content of the reference (URL, title, etc.)
    position: int  # Position in article


@dataclass
class Reference:
    """Represents a reference in a Wikipedia article."""
    name: Optional[str]
    url: Optional[str]
    title: Optional[str]
    content: str  # Full HTML content of the reference


@dataclass
class FactCheckResult:
    """Result of fact-checking a single citation."""
    citation: Citation
    reference: Optional[Reference]
    status: str  # 'verified', 'unverified', 'reference_not_found', 'error'
    similarity_score: float  # 0.0 to 1.0
    details: str


class WikipediaFactChecker:
    """Main fact-checking bot for Wikipedia articles."""

    def __init__(self, verbose: bool = False):
        """Initialize the fact checker.

        Args:
            verbose: Enable verbose logging
        """
        self.verbose = verbose
        if verbose:
            logger.setLevel(logging.DEBUG)

        self.session = requests.Session()
        self.session.headers.update({
            'User-Agent': 'Wikipedia-Fact-Checker-Bot/1.0 (Educational Purpose)'
        })

    def fetch_article(self, article_title: str) -> Optional[str]:
        """Fetch Wikipedia article content.

        Args:
            article_title: Title of the Wikipedia article

        Returns:
            Article wikitext or None if error
        """
        logger.info(f"Fetching article: {article_title}")

        url = "https://en.wikipedia.org/w/api.php"
        params = {
            'action': 'query',
            'prop': 'revisions',
            'titles': article_title,
            'rvprop': 'content',
            'rvslots': 'main',
            'format': 'json',
            'formatversion': '2'
        }

        try:
            response = self.session.get(url, params=params, timeout=10)
            response.raise_for_status()
            data = response.json()

            if 'query' in data and 'pages' in data['query']:
                page = data['query']['pages'][0]
                if 'revisions' in page:
                    content = page['revisions'][0]['slots']['main']['content']
                    logger.debug(f"Successfully fetched article (length: {len(content)})")
                    return content

            logger.error("Article not found or no content available")
            return None

        except Exception as e:
            logger.error(f"Error fetching article: {e}")
            return None

    def extract_citations(self, wikitext: str) -> List[Citation]:
        """Extract citations from Wikipedia wikitext.

        Args:
            wikitext: Wikipedia article in wikitext format

        Returns:
            List of Citation objects
        """
        logger.info("Extracting citations from article")
        citations = []

        try:
            wikicode = mwparserfromhell.parse(wikitext)

            # Find all text with inline citations
            # Pattern: text<ref>reference content</ref> or text<ref name="..."/>
            ref_pattern = re.compile(r'([^<]{10,}?)<ref([^>]*)>(.*?)</ref>', re.DOTALL)

            for match in ref_pattern.finditer(str(wikicode)):
                text = match.group(1).strip()
                ref_attrs = match.group(2)
                ref_content = match.group(3)

                # Extract reference name if present
                name_match = re.search(r'name\s*=\s*["\']?([^"\'>\s]+)', ref_attrs)
                ref_name = name_match.group(1) if name_match else None

                if text and (ref_content or ref_name):
                    citation = Citation(
                        text=text[-200:],  # Last 200 chars before citation
                        ref_name=ref_name,
                        ref_content=ref_content,
                        position=match.start()
                    )
                    citations.append(citation)

            # Also find named reference uses: <ref name="..." />
            named_ref_pattern = re.compile(r'([^<]{10,}?)<ref\s+name\s*=\s*["\']?([^"\'>\s]+)["\']?\s*/>', re.DOTALL)

            for match in named_ref_pattern.finditer(str(wikicode)):
                text = match.group(1).strip()
                ref_name = match.group(2)

                citation = Citation(
                    text=text[-200:],
                    ref_name=ref_name,
                    ref_content='',  # Will be looked up from references section
                    position=match.start()
                )
                citations.append(citation)

            logger.info(f"Found {len(citations)} citations")

        except Exception as e:
            logger.error(f"Error extracting citations: {e}")

        return citations

    def extract_references(self, wikitext: str) -> Dict[str, Reference]:
        """Extract reference definitions from Wikipedia wikitext.

        Args:
            wikitext: Wikipedia article in wikitext format

        Returns:
            Dictionary mapping reference names to Reference objects
        """
        logger.info("Extracting reference definitions")
        references = {}

        try:
            # Find all <ref name="...">content</ref> tags
            ref_pattern = re.compile(r'<ref\s+name\s*=\s*["\']?([^"\'>\s]+)["\']?[^>]*>(.*?)</ref>', re.DOTALL | re.IGNORECASE)

            for match in ref_pattern.finditer(wikitext):
                ref_name = match.group(1)
                ref_content = match.group(2)

                # Extract URL from reference content
                url = self._extract_url(ref_content)
                title = self._extract_title(ref_content)

                reference = Reference(
                    name=ref_name,
                    url=url,
                    title=title,
                    content=ref_content
                )
                references[ref_name] = reference

            logger.info(f"Found {len(references)} reference definitions")

        except Exception as e:
            logger.error(f"Error extracting references: {e}")

        return references

    def _extract_url(self, ref_content: str) -> Optional[str]:
        """Extract URL from reference content."""
        # Try to find url= parameter
        url_match = re.search(r'url\s*=\s*([^\s\|}\]]+)', ref_content)
        if url_match:
            return url_match.group(1).strip()

        # Try to find raw URL
        url_match = re.search(r'https?://[^\s\|}\]]+', ref_content)
        if url_match:
            return url_match.group(0).strip()

        return None

    def _extract_title(self, ref_content: str) -> Optional[str]:
        """Extract title from reference content."""
        title_match = re.search(r'title\s*=\s*([^\|}\]]+)', ref_content)
        if title_match:
            return title_match.group(1).strip()
        return None

    def fetch_reference_content(self, url: str) -> Optional[str]:
        """Fetch content from a reference URL.

        Args:
            url: URL of the reference

        Returns:
            Text content of the reference or None if error
        """
        logger.debug(f"Fetching reference content from: {url}")

        try:
            response = self.session.get(url, timeout=10, allow_redirects=True)
            response.raise_for_status()

            # Parse HTML and extract text
            soup = BeautifulSoup(response.content, 'html.parser')

            # Remove script and style elements
            for script in soup(["script", "style"]):
                script.decompose()

            # Get text
            text = soup.get_text(separator=' ', strip=True)
            logger.debug(f"Fetched {len(text)} characters from reference")

            return text

        except Exception as e:
            logger.warning(f"Error fetching reference content: {e}")
            return None

    def calculate_similarity(self, text1: str, text2: str) -> float:
        """Calculate similarity between two texts.

        Simple word overlap similarity. In production, this could use
        more sophisticated NLP techniques.

        Args:
            text1: First text
            text2: Second text

        Returns:
            Similarity score from 0.0 to 1.0
        """
        # Normalize texts
        words1 = set(re.findall(r'\w+', text1.lower()))
        words2 = set(re.findall(r'\w+', text2.lower()))

        if not words1 or not words2:
            return 0.0

        # Calculate Jaccard similarity
        intersection = words1.intersection(words2)
        union = words1.union(words2)

        similarity = len(intersection) / len(union) if union else 0.0

        return similarity

    def verify_citation(self, citation: Citation, reference: Optional[Reference]) -> FactCheckResult:
        """Verify a single citation against its reference.

        Args:
            citation: Citation to verify
            reference: Reference to check against

        Returns:
            FactCheckResult with verification details
        """
        if not reference:
            return FactCheckResult(
                citation=citation,
                reference=None,
                status='reference_not_found',
                similarity_score=0.0,
                details='Reference definition not found'
            )

        # If reference has a URL, fetch and check content
        if reference.url:
            ref_content = self.fetch_reference_content(reference.url)

            if ref_content is None:
                return FactCheckResult(
                    citation=citation,
                    reference=reference,
                    status='error',
                    similarity_score=0.0,
                    details=f'Could not fetch reference from URL: {reference.url}'
                )

            # Calculate similarity between citation text and reference content
            similarity = self.calculate_similarity(citation.text, ref_content)

            # Also check against reference title if available
            if reference.title:
                title_similarity = self.calculate_similarity(citation.text, reference.title)
                similarity = max(similarity, title_similarity)

            status = 'verified' if similarity > 0.1 else 'unverified'
            details = f'Similarity score: {similarity:.2%}'

            if similarity <= 0.1:
                details += ' - Reference content may not support the claim'

            return FactCheckResult(
                citation=citation,
                reference=reference,
                status=status,
                similarity_score=similarity,
                details=details
            )

        else:
            # No URL available, can't fully verify
            return FactCheckResult(
                citation=citation,
                reference=reference,
                status='unverified',
                similarity_score=0.0,
                details='Reference has no URL to verify against'
            )

    def check_article(self, article_title: str, max_citations: int = 10) -> List[FactCheckResult]:
        """Check an entire Wikipedia article for citation accuracy.

        Args:
            article_title: Title of the Wikipedia article
            max_citations: Maximum number of citations to check (to avoid rate limiting)

        Returns:
            List of FactCheckResult objects
        """
        logger.info(f"Starting fact-check of article: {article_title}")

        # Fetch article
        wikitext = self.fetch_article(article_title)
        if not wikitext:
            logger.error("Could not fetch article")
            return []

        # Extract citations and references
        citations = self.extract_citations(wikitext)
        references = self.extract_references(wikitext)

        # Limit citations to check
        citations_to_check = citations[:max_citations]
        logger.info(f"Checking {len(citations_to_check)} citations (out of {len(citations)} total)")

        results = []
        for i, citation in enumerate(citations_to_check, 1):
            logger.info(f"Checking citation {i}/{len(citations_to_check)}")

            # Find corresponding reference
            reference = None
            if citation.ref_name:
                reference = references.get(citation.ref_name)
            elif citation.ref_content:
                # Create temporary reference from inline content
                url = self._extract_url(citation.ref_content)
                title = self._extract_title(citation.ref_content)
                reference = Reference(
                    name=None,
                    url=url,
                    title=title,
                    content=citation.ref_content
                )

            # Verify citation
            result = self.verify_citation(citation, reference)
            results.append(result)

        logger.info(f"Fact-check complete. Checked {len(results)} citations")
        return results

    def generate_report(self, results: List[FactCheckResult]) -> str:
        """Generate a human-readable report of fact-check results.

        Args:
            results: List of FactCheckResult objects

        Returns:
            Formatted report as string
        """
        report = ["=" * 80]
        report.append("WIKIPEDIA FACT-CHECK REPORT")
        report.append("=" * 80)
        report.append("")

        # Summary statistics
        total = len(results)
        verified = sum(1 for r in results if r.status == 'verified')
        unverified = sum(1 for r in results if r.status == 'unverified')
        not_found = sum(1 for r in results if r.status == 'reference_not_found')
        errors = sum(1 for r in results if r.status == 'error')

        report.append("SUMMARY:")
        report.append(f"  Total citations checked: {total}")
        report.append(f"  Verified: {verified} ({verified/total*100:.1f}%)" if total > 0 else "  Verified: 0")
        report.append(f"  Unverified: {unverified} ({unverified/total*100:.1f}%)" if total > 0 else "  Unverified: 0")
        report.append(f"  Reference not found: {not_found}")
        report.append(f"  Errors: {errors}")
        report.append("")
        report.append("=" * 80)
        report.append("")

        # Detailed results
        for i, result in enumerate(results, 1):
            report.append(f"CITATION #{i}")
            report.append("-" * 80)
            report.append(f"Text: {result.citation.text[:150]}...")
            report.append(f"Status: {result.status.upper()}")
            report.append(f"Similarity: {result.similarity_score:.2%}")

            if result.reference:
                if result.reference.name:
                    report.append(f"Reference: {result.reference.name}")
                if result.reference.url:
                    report.append(f"URL: {result.reference.url}")
                if result.reference.title:
                    report.append(f"Title: {result.reference.title}")

            report.append(f"Details: {result.details}")
            report.append("")

        report.append("=" * 80)

        return "\n".join(report)


def main():
    """Main entry point for the fact-checking bot."""
    import argparse

    parser = argparse.ArgumentParser(
        description='Wikipedia Fact-Checking Bot - Verify citations against references'
    )
    parser.add_argument(
        'article',
        help='Wikipedia article title to check'
    )
    parser.add_argument(
        '--max-citations',
        type=int,
        default=10,
        help='Maximum number of citations to check (default: 10)'
    )
    parser.add_argument(
        '--verbose',
        action='store_true',
        help='Enable verbose logging'
    )
    parser.add_argument(
        '--output',
        help='Output file for report (default: print to stdout)'
    )

    args = parser.parse_args()

    # Create fact checker
    checker = WikipediaFactChecker(verbose=args.verbose)

    # Check article
    results = checker.check_article(args.article, max_citations=args.max_citations)

    # Generate report
    report = checker.generate_report(results)

    # Output report
    if args.output:
        with open(args.output, 'w', encoding='utf-8') as f:
            f.write(report)
        print(f"Report saved to: {args.output}")
    else:
        print(report)


if __name__ == '__main__':
    main()
