#!/usr/bin/env python3
"""
Benchmark common Wikipedia queries across different DBMS.
"""

import argparse
import time
import statistics
import sqlite3
import psycopg2
import json
from datetime import datetime
from typing import Dict, List, Callable


class DatabaseBenchmark:
    """Base class for database benchmarks."""

    def __init__(self, name: str):
        self.name = name
        self.results = []

    def run_query(self, query_name: str, query_func: Callable, iterations: int = 10):
        """
        Run a query multiple times and record statistics.

        Args:
            query_name: Name of the query
            query_func: Function that executes the query
            iterations: Number of times to run the query

        Returns:
            Dictionary with benchmark results
        """
        times = []

        for i in range(iterations):
            start = time.perf_counter()
            result = query_func()
            end = time.perf_counter()
            times.append((end - start) * 1000)  # Convert to milliseconds

        result_dict = {
            'query': query_name,
            'database': self.name,
            'iterations': iterations,
            'min_ms': min(times),
            'max_ms': max(times),
            'mean_ms': statistics.mean(times),
            'median_ms': statistics.median(times),
            'stdev_ms': statistics.stdev(times) if len(times) > 1 else 0,
            'result_count': len(result) if isinstance(result, (list, tuple)) else 1
        }

        self.results.append(result_dict)
        return result_dict


class PostgreSQLBenchmark(DatabaseBenchmark):
    """PostgreSQL benchmark."""

    def __init__(self, host='localhost', port=5432, database='wikipedia',
                 user='wikiuser', password='wikipass'):
        super().__init__('PostgreSQL')
        self.conn = psycopg2.connect(
            host=host, port=port, database=database, user=user, password=password
        )
        self.cursor = self.conn.cursor()

    def get_page_by_id(self, page_id: int):
        """Get page by ID."""
        self.cursor.execute(
            "SELECT page_id, page_title, page_namespace, page_len FROM pages WHERE page_id = %s",
            (page_id,)
        )
        return self.cursor.fetchall()

    def search_pages_by_title(self, title_prefix: str):
        """Search pages by title prefix."""
        self.cursor.execute(
            "SELECT page_id, page_title FROM pages WHERE page_title LIKE %s LIMIT 100",
            (f'{title_prefix}%',)
        )
        return self.cursor.fetchall()

    def get_page_links(self, page_id: int):
        """Get all outgoing links from a page."""
        self.cursor.execute(
            "SELECT pl_title FROM pagelinks WHERE pl_from = %s",
            (page_id,)
        )
        return self.cursor.fetchall()

    def get_backlinks(self, title: str):
        """Get all pages linking to a given page."""
        self.cursor.execute(
            "SELECT p.page_id, p.page_title FROM pages p "
            "INNER JOIN pagelinks pl ON p.page_id = pl.pl_from "
            "WHERE pl.pl_title = %s",
            (title,)
        )
        return self.cursor.fetchall()

    def get_page_categories(self, page_id: int):
        """Get categories for a page."""
        self.cursor.execute(
            "SELECT cl_to FROM categorylinks WHERE cl_from = %s",
            (page_id,)
        )
        return self.cursor.fetchall()

    def count_pages_by_namespace(self):
        """Count pages by namespace."""
        self.cursor.execute(
            "SELECT page_namespace, COUNT(*) FROM pages GROUP BY page_namespace"
        )
        return self.cursor.fetchall()

    def close(self):
        self.cursor.close()
        self.conn.close()


class SQLiteBenchmark(DatabaseBenchmark):
    """SQLite benchmark."""

    def __init__(self, database='wikipedia.db'):
        super().__init__('SQLite')
        self.conn = sqlite3.connect(database)
        self.cursor = self.conn.cursor()

    def get_page_by_id(self, page_id: int):
        """Get page by ID."""
        self.cursor.execute(
            "SELECT page_id, page_title, page_namespace, page_len FROM pages WHERE page_id = ?",
            (page_id,)
        )
        return self.cursor.fetchall()

    def search_pages_by_title(self, title_prefix: str):
        """Search pages by title prefix."""
        self.cursor.execute(
            "SELECT page_id, page_title FROM pages WHERE page_title LIKE ? LIMIT 100",
            (f'{title_prefix}%',)
        )
        return self.cursor.fetchall()

    def get_page_links(self, page_id: int):
        """Get all outgoing links from a page."""
        self.cursor.execute(
            "SELECT pl_title FROM pagelinks WHERE pl_from = ?",
            (page_id,)
        )
        return self.cursor.fetchall()

    def get_backlinks(self, title: str):
        """Get all pages linking to a given page."""
        self.cursor.execute(
            "SELECT p.page_id, p.page_title FROM pages p "
            "INNER JOIN pagelinks pl ON p.page_id = pl.pl_from "
            "WHERE pl.pl_title = ?",
            (title,)
        )
        return self.cursor.fetchall()

    def get_page_categories(self, page_id: int):
        """Get categories for a page."""
        self.cursor.execute(
            "SELECT cl_to FROM categorylinks WHERE cl_from = ?",
            (page_id,)
        )
        return self.cursor.fetchall()

    def count_pages_by_namespace(self):
        """Count pages by namespace."""
        self.cursor.execute(
            "SELECT page_namespace, COUNT(*) FROM pages GROUP BY page_namespace"
        )
        return self.cursor.fetchall()

    def close(self):
        self.cursor.close()
        self.conn.close()


def run_benchmarks(databases: List[DatabaseBenchmark], iterations: int = 10):
    """
    Run standard benchmark queries on all databases.

    Args:
        databases: List of database benchmark instances
        iterations: Number of iterations per query

    Returns:
        List of all benchmark results
    """
    all_results = []

    # Standard test parameters
    test_page_id = 1
    test_title_prefix = 'A'
    test_link_target = 'Wikipedia'

    for db in databases:
        print(f"\n{'=' * 60}")
        print(f"Benchmarking {db.name}")
        print(f"{'=' * 60}\n")

        # Query 1: Get page by ID
        result = db.run_query(
            "Get page by ID",
            lambda: db.get_page_by_id(test_page_id),
            iterations
        )
        print(f"Get page by ID: {result['mean_ms']:.2f} ms (±{result['stdev_ms']:.2f})")

        # Query 2: Search pages by title prefix
        result = db.run_query(
            "Search pages by title prefix",
            lambda: db.search_pages_by_title(test_title_prefix),
            iterations
        )
        print(f"Search by title: {result['mean_ms']:.2f} ms (±{result['stdev_ms']:.2f})")

        # Query 3: Get outgoing links
        result = db.run_query(
            "Get outgoing links from page",
            lambda: db.get_page_links(test_page_id),
            iterations
        )
        print(f"Get outgoing links: {result['mean_ms']:.2f} ms (±{result['stdev_ms']:.2f})")

        # Query 4: Get backlinks
        result = db.run_query(
            "Get pages linking to target",
            lambda: db.get_backlinks(test_link_target),
            iterations
        )
        print(f"Get backlinks: {result['mean_ms']:.2f} ms (±{result['stdev_ms']:.2f})")

        # Query 5: Get page categories
        result = db.run_query(
            "Get page categories",
            lambda: db.get_page_categories(test_page_id),
            iterations
        )
        print(f"Get categories: {result['mean_ms']:.2f} ms (±{result['stdev_ms']:.2f})")

        # Query 6: Count pages by namespace
        result = db.run_query(
            "Count pages by namespace",
            lambda: db.count_pages_by_namespace(),
            iterations
        )
        print(f"Count by namespace: {result['mean_ms']:.2f} ms (±{result['stdev_ms']:.2f})")

        all_results.extend(db.results)

    return all_results


def print_comparison(results: List[Dict]):
    """Print comparison table of results."""
    print(f"\n{'=' * 80}")
    print("BENCHMARK COMPARISON")
    print(f"{'=' * 80}\n")

    # Group by query
    queries = {}
    for result in results:
        query = result['query']
        if query not in queries:
            queries[query] = []
        queries[query].append(result)

    # Print comparison for each query
    for query, query_results in queries.items():
        print(f"\n{query}:")
        print(f"  {'Database':<15} {'Mean (ms)':<12} {'Median (ms)':<12} {'Min (ms)':<12} {'Max (ms)':<12}")
        print(f"  {'-' * 70}")

        for result in sorted(query_results, key=lambda x: x['mean_ms']):
            print(f"  {result['database']:<15} "
                  f"{result['mean_ms']:>10.2f}  "
                  f"{result['median_ms']:>10.2f}  "
                  f"{result['min_ms']:>10.2f}  "
                  f"{result['max_ms']:>10.2f}")


def save_results(results: List[Dict], filename: str):
    """Save results to JSON file."""
    output = {
        'timestamp': datetime.now().isoformat(),
        'results': results
    }

    with open(filename, 'w') as f:
        json.dump(output, f, indent=2)

    print(f"\nResults saved to {filename}")


def main():
    parser = argparse.ArgumentParser(description='Benchmark Wikipedia databases')
    parser.add_argument('--iterations', type=int, default=10, help='Number of iterations per query')
    parser.add_argument('--output', default='benchmark_results.json', help='Output file for results')
    parser.add_argument('--postgresql', action='store_true', help='Benchmark PostgreSQL')
    parser.add_argument('--sqlite', action='store_true', help='Benchmark SQLite')
    parser.add_argument('--all', action='store_true', help='Benchmark all databases')

    args = parser.parse_args()

    databases = []

    # Initialize databases to benchmark
    if args.all or args.postgresql:
        try:
            databases.append(PostgreSQLBenchmark())
            print("PostgreSQL benchmark initialized")
        except Exception as e:
            print(f"Could not connect to PostgreSQL: {e}")

    if args.all or args.sqlite:
        try:
            databases.append(SQLiteBenchmark())
            print("SQLite benchmark initialized")
        except Exception as e:
            print(f"Could not connect to SQLite: {e}")

    if not databases:
        print("No databases to benchmark. Use --all, --postgresql, or --sqlite")
        return

    # Run benchmarks
    results = run_benchmarks(databases, iterations=args.iterations)

    # Print comparison
    print_comparison(results)

    # Save results
    save_results(results, args.output)

    # Close connections
    for db in databases:
        db.close()


if __name__ == '__main__':
    main()
