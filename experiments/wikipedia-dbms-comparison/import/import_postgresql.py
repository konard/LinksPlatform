#!/usr/bin/env python3
"""
Import Wikipedia XML dump into PostgreSQL database.
"""

import argparse
import time
from datetime import datetime
from tqdm import tqdm
import psycopg2
from psycopg2.extras import execute_batch

from common import WikipediaXMLParser, parse_timestamp, format_title


class PostgreSQLWikipediaImporter:
    """Import Wikipedia dump into PostgreSQL."""

    def __init__(self, host='localhost', port=5432, database='wikipedia',
                 user='wikiuser', password='wikipass'):
        """
        Initialize PostgreSQL importer.

        Args:
            host: Database host
            port: Database port
            database: Database name
            user: Database user
            password: Database password
        """
        self.conn = psycopg2.connect(
            host=host,
            port=port,
            database=database,
            user=user,
            password=password
        )
        self.conn.autocommit = False
        self.cursor = self.conn.cursor()

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
        Import Wikipedia dump into PostgreSQL.

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
            "UPDATE import_stats SET import_started = %s, notes = %s WHERE stat_id = 1",
            (datetime.now(), f'Import from {dump_file}')
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
                        page_id,
                        namespace,
                        title,
                        is_redirect,
                        latest_rev_id,
                        page_len,
                        datetime.now()
                    ))

                    # Process revisions
                    for rev in page['revisions']:
                        text_content = rev.get('text', '')

                        # Add text
                        text_batch.append((text_id_counter, text_content))

                        # Add revision
                        revisions_batch.append((
                            rev.get('id'),
                            page_id,
                            text_id_counter,
                            rev.get('comment', ''),
                            0,  # rev_user (anonymous)
                            rev.get('user_text', ''),
                            parse_timestamp(rev.get('timestamp', '2000-01-01T00:00:00Z')),
                            0,  # minor edit
                            0,  # deleted
                            len(text_content),
                            None  # parent_id
                        ))

                        # Extract links (only from latest revision to save time)
                        if rev == latest_revision and namespace == 0:  # Main namespace
                            for link in parser.extract_links(text_content):
                                links_batch.append((
                                    page_id,
                                    0,  # namespace
                                    format_title(link),
                                    namespace
                                ))

                            # Extract categories
                            for category in parser.extract_categories(text_content):
                                categories_batch.append((
                                    page_id,
                                    format_title(category),
                                    title,
                                    datetime.now(),
                                    'page'
                                ))

                        text_id_counter += 1

                    # Handle redirects
                    if page.get('redirect'):
                        redirects_batch.append((
                            page_id,
                            0,  # namespace
                            format_title(page['redirect']),
                            None,  # fragment
                            None   # interwiki
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
        self._update_final_stats(elapsed_time)

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
                execute_batch(
                    self.cursor,
                    "INSERT INTO pages (page_id, page_namespace, page_title, page_is_redirect, "
                    "page_latest, page_len, page_touched) VALUES (%s, %s, %s, %s, %s, %s, %s) "
                    "ON CONFLICT (page_id) DO NOTHING",
                    pages
                )
                self.stats['pages'] += len(pages)

            # Insert texts
            if texts:
                execute_batch(
                    self.cursor,
                    "INSERT INTO page_text (text_id, text_content) VALUES (%s, %s) "
                    "ON CONFLICT (text_id) DO NOTHING",
                    texts
                )

            # Insert revisions
            if revisions:
                execute_batch(
                    self.cursor,
                    "INSERT INTO revisions (rev_id, rev_page, rev_text_id, rev_comment, "
                    "rev_user, rev_user_text, rev_timestamp, rev_minor_edit, rev_deleted, "
                    "rev_len, rev_parent_id) VALUES (%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s) "
                    "ON CONFLICT (rev_id) DO NOTHING",
                    revisions
                )
                self.stats['revisions'] += len(revisions)

            # Insert links
            if links:
                execute_batch(
                    self.cursor,
                    "INSERT INTO pagelinks (pl_from, pl_namespace, pl_title, pl_from_namespace) "
                    "VALUES (%s, %s, %s, %s) ON CONFLICT DO NOTHING",
                    links
                )
                self.stats['links'] += len(links)

            # Insert categories
            if categories:
                execute_batch(
                    self.cursor,
                    "INSERT INTO categorylinks (cl_from, cl_to, cl_sortkey, cl_timestamp, cl_type) "
                    "VALUES (%s, %s, %s, %s, %s) ON CONFLICT DO NOTHING",
                    categories
                )
                self.stats['categories'] += len(categories)

            # Insert redirects
            if redirects:
                execute_batch(
                    self.cursor,
                    "INSERT INTO redirects (rd_from, rd_namespace, rd_title, rd_fragment, rd_interwiki) "
                    "VALUES (%s, %s, %s, %s, %s) ON CONFLICT DO NOTHING",
                    redirects
                )
                self.stats['redirects'] += len(redirects)

            self.conn.commit()

        except Exception as e:
            print(f"Error flushing batch: {e}")
            self.conn.rollback()

    def _update_final_stats(self, elapsed_time):
        """Update import statistics."""
        # Get database size
        self.cursor.execute(
            "SELECT pg_database_size(%s)",
            (self.conn.info.dbname,)
        )
        db_size = self.cursor.fetchone()[0]

        # Update stats
        self.cursor.execute(
            "UPDATE import_stats SET "
            "import_completed = %s, "
            "total_pages = %s, "
            "total_revisions = %s, "
            "total_links = %s, "
            "total_categories = %s, "
            "database_size = %s "
            "WHERE stat_id = 1",
            (
                datetime.now(),
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
        self.cursor.close()
        self.conn.close()


def main():
    parser = argparse.ArgumentParser(description='Import Wikipedia dump to PostgreSQL')
    parser.add_argument('--input', required=True, help='Path to Wikipedia XML dump')
    parser.add_argument('--limit', type=int, help='Limit number of pages to import')
    parser.add_argument('--batch-size', type=int, default=1000, help='Batch size for inserts')
    parser.add_argument('--host', default='localhost', help='PostgreSQL host')
    parser.add_argument('--port', type=int, default=5432, help='PostgreSQL port')
    parser.add_argument('--database', default='wikipedia', help='Database name')
    parser.add_argument('--user', default='wikiuser', help='Database user')
    parser.add_argument('--password', default='wikipass', help='Database password')

    args = parser.parse_args()

    importer = PostgreSQLWikipediaImporter(
        host=args.host,
        port=args.port,
        database=args.database,
        user=args.user,
        password=args.password
    )

    try:
        importer.import_dump(args.input, limit=args.limit, batch_size=args.batch_size)
    finally:
        importer.close()


if __name__ == '__main__':
    main()
