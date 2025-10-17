#!/usr/bin/env python3
"""
Import Wikipedia XML dump into SQLite database.
"""

import argparse
import sqlite3
import time
from datetime import datetime
from tqdm import tqdm

from common import WikipediaXMLParser, parse_timestamp, format_title


class SQLiteWikipediaImporter:
    """Import Wikipedia dump into SQLite."""

    def __init__(self, database='wikipedia.db'):
        """
        Initialize SQLite importer.

        Args:
            database: Path to SQLite database file
        """
        self.conn = sqlite3.connect(database)
        self.cursor = self.conn.cursor()

        # Enable performance optimizations
        self.cursor.execute('PRAGMA journal_mode = WAL')
        self.cursor.execute('PRAGMA synchronous = NORMAL')
        self.cursor.execute('PRAGMA temp_store = MEMORY')
        self.cursor.execute('PRAGMA mmap_size = 30000000000')
        self.cursor.execute('PRAGMA cache_size = -64000')  # 64MB cache

        # Load schema
        with open('../schemas/sqlite_schema.sql', 'r') as f:
            self.cursor.executescript(f.read())
        self.conn.commit()

        # Statistics
        self.stats = {
            'pages': 0,
            'revisions': 0,
            'links': 0,
            'categories': 0,
            'redirects': 0
        }

    def import_dump(self, dump_file: str, limit: int = None, batch_size: int = 1000):
        """
        Import Wikipedia dump into SQLite.

        Args:
            dump_file: Path to Wikipedia XML dump
            limit: Maximum number of pages to import (None for all)
            batch_size: Number of records to insert in batch
        """
        parser = WikipediaXMLParser(dump_file)

        print(f"Starting import from {dump_file}")
        start_time = time.time()

        # Update stats - import started
        self.cursor.execute(
            "UPDATE import_stats SET import_started = ?, notes = ? WHERE stat_id = 1",
            (datetime.now().isoformat(), f'Import from {dump_file}')
        )
        self.conn.commit()

        # Batch buffers
        pages_batch = []
        revisions_batch = []
        text_batch = []
        links_batch = []
        categories_batch = []
        redirects_batch = []

        text_id_counter = 1

        # Parse and import pages
        for page in tqdm(parser.parse_pages(limit=limit), desc="Importing pages"):
            try:
                page_id = page['page_id']
                namespace = page.get('namespace', 0)
                title = format_title(page['title'])
                is_redirect = 1 if page.get('redirect') else 0

                # Get latest revision
                latest_rev_id = None
                page_len = 0

                if page['revisions']:
                    latest_revision = page['revisions'][-1]
                    latest_rev_id = latest_revision.get('id')
                    page_len = latest_revision.get('text_bytes', 0)

                    # Add page
                    pages_batch.append((
                        page_id, namespace, title, is_redirect,
                        latest_rev_id, page_len, datetime.now().isoformat()
                    ))

                    # Process revisions
                    for rev in page['revisions']:
                        text_content = rev.get('text', '')

                        # Add text
                        text_batch.append((text_id_counter, text_content))

                        # Add revision
                        timestamp = parse_timestamp(rev.get('timestamp', '2000-01-01T00:00:00Z'))
                        revisions_batch.append((
                            rev.get('id'), page_id, text_id_counter,
                            rev.get('comment', ''), 0, rev.get('user_text', ''),
                            timestamp.isoformat(), 0, 0, len(text_content), None
                        ))

                        # Extract links (only from latest revision)
                        if rev == latest_revision and namespace == 0:
                            for link in parser.extract_links(text_content):
                                links_batch.append((
                                    page_id, 0, format_title(link), namespace
                                ))

                            # Extract categories
                            for category in parser.extract_categories(text_content):
                                categories_batch.append((
                                    page_id, format_title(category), title,
                                    datetime.now().isoformat(), 'page'
                                ))

                        text_id_counter += 1

                    # Handle redirects
                    if page.get('redirect'):
                        redirects_batch.append((
                            page_id, 0, format_title(page['redirect']), None, None
                        ))

                # Flush batches
                if len(pages_batch) >= batch_size:
                    self._flush_batches(
                        pages_batch, revisions_batch, text_batch,
                        links_batch, categories_batch, redirects_batch
                    )
                    pages_batch, revisions_batch, text_batch = [], [], []
                    links_batch, categories_batch, redirects_batch = [], [], []

            except Exception as e:
                print(f"Error processing page {page.get('title', 'unknown')}: {e}")
                continue

        # Flush remaining batches
        if pages_batch:
            self._flush_batches(
                pages_batch, revisions_batch, text_batch,
                links_batch, categories_batch, redirects_batch
            )

        # Update final statistics
        elapsed_time = time.time() - start_time
        self._update_final_stats(elapsed_time, dump_file)

        print(f"\nImport completed in {elapsed_time:.2f} seconds")
        print(f"Pages: {self.stats['pages']}")
        print(f"Revisions: {self.stats['revisions']}")
        print(f"Links: {self.stats['links']}")
        print(f"Categories: {self.stats['categories']}")
        print(f"Redirects: {self.stats['redirects']}")

    def _flush_batches(self, pages, revisions, texts, links, categories, redirects):
        """Insert batches into database."""
        try:
            # Insert pages
            if pages:
                self.cursor.executemany(
                    "INSERT OR IGNORE INTO pages (page_id, page_namespace, page_title, "
                    "page_is_redirect, page_latest, page_len, page_touched) "
                    "VALUES (?, ?, ?, ?, ?, ?, ?)",
                    pages
                )
                self.stats['pages'] += len(pages)

            # Insert texts
            if texts:
                self.cursor.executemany(
                    "INSERT OR IGNORE INTO page_text (text_id, text_content) VALUES (?, ?)",
                    texts
                )

            # Insert revisions
            if revisions:
                self.cursor.executemany(
                    "INSERT OR IGNORE INTO revisions (rev_id, rev_page, rev_text_id, "
                    "rev_comment, rev_user, rev_user_text, rev_timestamp, rev_minor_edit, "
                    "rev_deleted, rev_len, rev_parent_id) "
                    "VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                    revisions
                )
                self.stats['revisions'] += len(revisions)

            # Insert links
            if links:
                self.cursor.executemany(
                    "INSERT OR IGNORE INTO pagelinks (pl_from, pl_namespace, pl_title, "
                    "pl_from_namespace) VALUES (?, ?, ?, ?)",
                    links
                )
                self.stats['links'] += len(links)

            # Insert categories
            if categories:
                self.cursor.executemany(
                    "INSERT OR IGNORE INTO categorylinks (cl_from, cl_to, cl_sortkey, "
                    "cl_timestamp, cl_type) VALUES (?, ?, ?, ?, ?)",
                    categories
                )
                self.stats['categories'] += len(categories)

            # Insert redirects
            if redirects:
                self.cursor.executemany(
                    "INSERT OR IGNORE INTO redirects (rd_from, rd_namespace, rd_title, "
                    "rd_fragment, rd_interwiki) VALUES (?, ?, ?, ?, ?)",
                    redirects
                )
                self.stats['redirects'] += len(redirects)

            self.conn.commit()

        except Exception as e:
            print(f"Error flushing batch: {e}")
            self.conn.rollback()

    def _update_final_stats(self, elapsed_time, dump_file):
        """Update import statistics."""
        import os

        # Get database size
        db_size = os.path.getsize(self.conn.execute("PRAGMA database_list").fetchone()[2])

        # Update stats
        self.cursor.execute(
            "UPDATE import_stats SET "
            "import_completed = ?, total_pages = ?, total_revisions = ?, "
            "total_links = ?, total_categories = ?, database_size = ? "
            "WHERE stat_id = 1",
            (
                datetime.now().isoformat(),
                self.stats['pages'],
                self.stats['revisions'],
                self.stats['links'],
                self.stats['categories'],
                db_size
            )
        )
        self.conn.commit()

    def close(self):
        """Close database connection."""
        # Optimize database
        print("Optimizing database...")
        self.cursor.execute('VACUUM')
        self.cursor.execute('ANALYZE')
        self.conn.commit()

        self.cursor.close()
        self.conn.close()


def main():
    parser = argparse.ArgumentParser(description='Import Wikipedia dump to SQLite')
    parser.add_argument('--input', required=True, help='Path to Wikipedia XML dump')
    parser.add_argument('--limit', type=int, help='Limit number of pages to import')
    parser.add_argument('--batch-size', type=int, default=1000, help='Batch size for inserts')
    parser.add_argument('--database', default='wikipedia.db', help='SQLite database file')

    args = parser.parse_args()

    importer = SQLiteWikipediaImporter(database=args.database)

    try:
        importer.import_dump(args.input, limit=args.limit, batch_size=args.batch_size)
    finally:
        importer.close()


if __name__ == '__main__':
    main()
