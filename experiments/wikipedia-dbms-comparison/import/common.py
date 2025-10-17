"""
Common utilities for parsing Wikipedia XML dumps and extracting data.
"""

import re
import bz2
import gzip
from typing import Iterator, Dict, Any, Optional
from datetime import datetime
import xml.etree.ElementTree as ET


class WikipediaXMLParser:
    """Parse Wikipedia XML dumps and extract page, revision, and link data."""

    # Wikipedia XML namespaces
    NAMESPACES = {
        'mw': 'http://www.mediawiki.org/xml/export-0.10/'
    }

    def __init__(self, dump_file: str):
        """
        Initialize parser with Wikipedia XML dump file.

        Args:
            dump_file: Path to Wikipedia XML dump (.xml, .xml.bz2, or .xml.gz)
        """
        self.dump_file = dump_file

    def _open_file(self):
        """Open dump file, handling compression."""
        if self.dump_file.endswith('.bz2'):
            return bz2.open(self.dump_file, 'rt', encoding='utf-8')
        elif self.dump_file.endswith('.gz'):
            return gzip.open(self.dump_file, 'rt', encoding='utf-8')
        else:
            return open(self.dump_file, 'r', encoding='utf-8')

    def parse_pages(self, limit: Optional[int] = None) -> Iterator[Dict[str, Any]]:
        """
        Parse pages from Wikipedia dump.

        Args:
            limit: Maximum number of pages to parse (None for all)

        Yields:
            Dictionary with page data including:
                - page_id: Page ID
                - namespace: Page namespace (0 for articles)
                - title: Page title
                - redirect: Redirect target (if page is redirect)
                - revisions: List of revision dicts
        """
        count = 0

        with self._open_file() as f:
            # Use iterparse for memory efficiency
            context = ET.iterparse(f, events=('start', 'end'))
            context = iter(context)

            # Get root element
            event, root = next(context)

            current_page = None
            current_revision = None
            current_text = []
            in_text = False

            for event, elem in context:
                tag = elem.tag.replace('{http://www.mediawiki.org/xml/export-0.10/}', '')

                if event == 'start':
                    if tag == 'page':
                        current_page = {
                            'revisions': [],
                            'redirect': None
                        }
                    elif tag == 'revision':
                        current_revision = {}
                    elif tag == 'text':
                        in_text = True
                        current_text = []

                elif event == 'end':
                    if tag == 'page' and current_page is not None:
                        yield current_page
                        count += 1
                        if limit and count >= limit:
                            break
                        current_page = None
                        # Clear processed elements to save memory
                        elem.clear()
                        root.clear()

                    elif tag == 'title' and current_page is not None:
                        current_page['title'] = elem.text or ''

                    elif tag == 'ns' and current_page is not None:
                        current_page['namespace'] = int(elem.text or 0)

                    elif tag == 'id':
                        if current_revision is not None:
                            current_revision['id'] = int(elem.text)
                        elif current_page is not None and 'page_id' not in current_page:
                            current_page['page_id'] = int(elem.text)

                    elif tag == 'redirect' and current_page is not None:
                        current_page['redirect'] = elem.get('title', '')

                    elif tag == 'timestamp' and current_revision is not None:
                        current_revision['timestamp'] = elem.text

                    elif tag == 'comment' and current_revision is not None:
                        current_revision['comment'] = elem.text or ''

                    elif tag == 'username' and current_revision is not None:
                        current_revision['user_text'] = elem.text or ''

                    elif tag == 'text' and current_revision is not None:
                        in_text = False
                        current_revision['text'] = ''.join(current_text)
                        current_revision['text_bytes'] = len(current_revision['text'])
                        current_text = []

                    elif tag == 'revision' and current_revision is not None:
                        current_page['revisions'].append(current_revision)
                        current_revision = None

                # Collect text content while in text element
                if in_text and elem.text:
                    current_text.append(elem.text)

    @staticmethod
    def extract_links(wikitext: str) -> list:
        """
        Extract internal links from Wikipedia wikitext.

        Args:
            wikitext: Wikipedia article text in wikitext format

        Returns:
            List of linked page titles
        """
        # Match [[Page Title]] or [[Page Title|Display Text]]
        link_pattern = r'\[\[([^\]|]+)(?:\|[^\]]+)?\]\]'
        matches = re.findall(link_pattern, wikitext)

        # Clean up links
        links = []
        for match in matches:
            # Remove section anchors
            link = match.split('#')[0].strip()
            # Skip empty links and file/image links
            if link and not link.lower().startswith(('file:', 'image:', 'category:')):
                links.append(link)

        return list(set(links))  # Remove duplicates

    @staticmethod
    def extract_categories(wikitext: str) -> list:
        """
        Extract categories from Wikipedia wikitext.

        Args:
            wikitext: Wikipedia article text in wikitext format

        Returns:
            List of category names
        """
        # Match [[Category:Name]] or [[Category:Name|Sort Key]]
        cat_pattern = r'\[\[Category:([^\]|]+)(?:\|[^\]]+)?\]\]'
        matches = re.findall(cat_pattern, wikitext, re.IGNORECASE)

        return [cat.strip() for cat in matches]


def parse_timestamp(timestamp_str: str) -> datetime:
    """
    Parse MediaWiki timestamp format.

    Args:
        timestamp_str: Timestamp in format 'YYYY-MM-DDTHH:MM:SSZ'

    Returns:
        datetime object
    """
    return datetime.fromisoformat(timestamp_str.replace('Z', '+00:00'))


def format_title(title: str) -> str:
    """
    Normalize Wikipedia page title.

    Args:
        title: Raw page title

    Returns:
        Normalized title
    """
    # Replace underscores with spaces
    title = title.replace('_', ' ')
    # Capitalize first letter
    if title:
        title = title[0].upper() + title[1:]
    return title.strip()


def get_namespace_name(namespace_id: int) -> str:
    """
    Get namespace name from namespace ID.

    Args:
        namespace_id: Wikipedia namespace ID

    Returns:
        Namespace name
    """
    namespaces = {
        0: 'Main',
        1: 'Talk',
        2: 'User',
        3: 'User talk',
        4: 'Wikipedia',
        5: 'Wikipedia talk',
        6: 'File',
        7: 'File talk',
        8: 'MediaWiki',
        9: 'MediaWiki talk',
        10: 'Template',
        11: 'Template talk',
        12: 'Help',
        13: 'Help talk',
        14: 'Category',
        15: 'Category talk',
    }
    return namespaces.get(namespace_id, f'Namespace {namespace_id}')
