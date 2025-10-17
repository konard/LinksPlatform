# Wikipedia Database DBMS Comparison

This experiment compares the performance and characteristics of storing and querying Wikipedia data across different Database Management Systems (DBMS) and Links Platform's Doublets implementation.

## Objective

To evaluate and compare:
1. **Storage efficiency** - How much space each system requires
2. **Import performance** - How quickly data can be loaded
3. **Query performance** - Speed of common operations (search, retrieval, relationships)
4. **Scalability** - How systems handle growing datasets
5. **Complexity** - Ease of schema design and maintenance

## Systems Under Test

- **PostgreSQL** - Popular open-source relational database
- **MySQL** - Widely-used relational database
- **SQLite** - Lightweight file-based database
- **Links Platform (Doublets)** - Associative memory implementation

## Wikipedia Data Structure

Wikipedia XML dumps contain:
- **Pages** - Articles with title, ID, namespace
- **Revisions** - Edit history with timestamps, contributors
- **Text** - Article content in MediaWiki format
- **Links** - Internal links between articles
- **Categories** - Article categorization

## Experiment Structure

```
experiments/wikipedia-dbms-comparison/
├── README.md                          # This file
├── docker-compose.yml                 # DBMS containers setup
├── schemas/
│   ├── postgresql_schema.sql         # PostgreSQL table definitions
│   ├── mysql_schema.sql              # MySQL table definitions
│   └── sqlite_schema.sql             # SQLite table definitions
├── import/
│   ├── requirements.txt              # Python dependencies
│   ├── import_postgresql.py          # PostgreSQL import script
│   ├── import_mysql.py               # MySQL import script
│   ├── import_sqlite.py              # SQLite import script
│   └── common.py                     # Shared Wikipedia XML parsing
├── benchmarks/
│   ├── benchmark_queries.py          # Standard query benchmarks
│   └── results/                      # Benchmark output
└── docs/
    ├── SETUP.md                      # Setup instructions
    └── METHODOLOGY.md                # Benchmarking methodology
```

## Quick Start

### 1. Prerequisites

- Docker and Docker Compose (for PostgreSQL, MySQL)
- Python 3.8+ with pip
- Wikipedia XML dump (sample or full)

### 2. Setup Databases

```bash
# Start PostgreSQL and MySQL containers
docker-compose up -d

# Wait for databases to be ready
sleep 10
```

### 3. Download Wikipedia Sample

```bash
# Small sample for testing (Simple English Wikipedia)
wget https://dumps.wikimedia.org/simplewiki/latest/simplewiki-latest-pages-articles.xml.bz2

# Or use a specific language/date
# wget https://dumps.wikimedia.org/enwiki/20240101/enwiki-20240101-pages-articles.xml.bz2
```

### 4. Install Python Dependencies

```bash
cd import
pip install -r requirements.txt
```

### 5. Import Data

```bash
# PostgreSQL
python import_postgresql.py --input simplewiki-latest-pages-articles.xml.bz2

# MySQL
python import_mysql.py --input simplewiki-latest-pages-articles.xml.bz2

# SQLite
python import_sqlite.py --input simplewiki-latest-pages-articles.xml.bz2
```

### 6. Run Benchmarks

```bash
cd ../benchmarks
python benchmark_queries.py --all
```

## Comparison Metrics

### Storage Metrics
- Database file/directory size
- Index size
- Compression ratio
- Memory usage during operations

### Performance Metrics
- Import time (total and per-page)
- Query response time:
  - Single page retrieval by ID
  - Page search by title
  - Finding all links from a page
  - Finding all pages linking to a page
  - Category queries
- Concurrent query performance
- Memory consumption

### Complexity Metrics
- Lines of schema definition
- Number of tables/indices
- Query complexity (SQL vs Links Platform API)

## Expected Outcomes

This comparison will help answer:

1. **When is Links Platform more efficient?**
   - Heavily interconnected data
   - Graph-like queries
   - Dynamic schema requirements

2. **When are traditional DBMS better?**
   - Structured tabular data
   - Complex SQL queries
   - Existing SQL ecosystem integration

3. **Trade-offs**
   - Development complexity vs performance
   - Storage efficiency vs query flexibility
   - Learning curve considerations

## Contributing

To add more database systems or improve benchmarks:
1. Add schema in `schemas/`
2. Create import script in `import/`
3. Update `benchmark_queries.py` with new system
4. Document in `docs/METHODOLOGY.md`

## References

- [Wikipedia Database Schema](https://www.mediawiki.org/wiki/Manual:Database_layout)
- [Wikipedia XML Dump Format](https://www.mediawiki.org/wiki/Help:Export)
- [Links Platform Documentation](https://linksplatform.github.io/)
- [Associative Model of Data](https://en.wikipedia.org/wiki/Associative_model_of_data)
