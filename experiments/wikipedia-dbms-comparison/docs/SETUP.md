# Setup Guide for Wikipedia DBMS Comparison

This guide will help you set up the Wikipedia database comparison experiment.

## Prerequisites

### Software Requirements

- **Docker** and **Docker Compose** (for PostgreSQL and MySQL)
  - Install from: https://docs.docker.com/get-docker/
- **Python 3.8+**
  - Check version: `python3 --version`
- **pip** (Python package manager)
- At least **10 GB** free disk space (for small Wikipedia dump)
- For full Wikipedia dumps: **100+ GB** free space

### Optional

- **PostgreSQL client** tools (`psql`) for manual database inspection
- **MySQL client** tools (`mysql`) for manual database inspection
- **SQLite** client (`sqlite3`) for manual database inspection

## Step-by-Step Setup

### 1. Clone the Repository

```bash
git clone https://github.com/konard/LinksPlatform.git
cd LinksPlatform/experiments/wikipedia-dbms-comparison
```

### 2. Start Database Containers

Start PostgreSQL and MySQL using Docker Compose:

```bash
docker-compose up -d
```

Wait for databases to initialize (about 10-30 seconds):

```bash
# Check if containers are running
docker-compose ps

# Check logs to ensure initialization completed
docker-compose logs postgresql
docker-compose logs mysql
```

### 3. Install Python Dependencies

```bash
cd import
pip install -r requirements.txt
```

Or using a virtual environment (recommended):

```bash
cd import
python3 -m venv venv
source venv/bin/activate  # On Windows: venv\Scripts\activate
pip install -r requirements.txt
```

### 4. Download Wikipedia Dump

#### Option A: Small Test Dump (Recommended for Testing)

Simple English Wikipedia (smaller, faster to process):

```bash
# Create data directory
mkdir -p ../data
cd ../data

# Download Simple English Wikipedia (compressed ~200 MB, ~1.5 GB uncompressed)
wget https://dumps.wikimedia.org/simplewiki/latest/simplewiki-latest-pages-articles.xml.bz2
```

#### Option B: Full Wikipedia Dump (For Real Benchmarking)

English Wikipedia (much larger, ~20 GB compressed):

```bash
mkdir -p ../data
cd ../data

# Download latest English Wikipedia dump
# Check available dumps at: https://dumps.wikimedia.org/enwiki/
wget https://dumps.wikimedia.org/enwiki/latest/enwiki-latest-pages-articles.xml.bz2
```

#### Option C: Specific Language

Choose your language from https://dumps.wikimedia.org/ and download `*-pages-articles.xml.bz2`.

### 5. Verify Setup

Check that everything is ready:

```bash
# Check PostgreSQL
docker exec wikipedia_postgres psql -U wikiuser -d wikipedia -c "SELECT version();"

# Check MySQL
docker exec wikipedia_mysql mysql -uwikiuser -pwikipass wikipedia -e "SELECT version();"

# Check Python packages
python3 -c "import psycopg2, pymysql, lxml, mwxml; print('All packages installed')"
```

## Next Steps

Once setup is complete, proceed to import Wikipedia data:

1. **Import to PostgreSQL**: See [IMPORTING.md](IMPORTING.md#postgresql)
2. **Import to MySQL**: See [IMPORTING.md](IMPORTING.md#mysql)
3. **Import to SQLite**: See [IMPORTING.md](IMPORTING.md#sqlite)
4. **Run Benchmarks**: See [BENCHMARKING.md](BENCHMARKING.md)

## Troubleshooting

### Database Connection Issues

**PostgreSQL connection refused:**
```bash
# Wait longer for initialization
docker-compose logs -f postgresql

# Restart container if needed
docker-compose restart postgresql
```

**MySQL connection refused:**
```bash
# Check logs
docker-compose logs -f mysql

# MySQL takes longer to initialize, wait 1-2 minutes
docker-compose restart mysql
```

### Python Package Installation Issues

**psycopg2 build errors:**
```bash
# Install PostgreSQL development libraries
# Ubuntu/Debian:
sudo apt-get install libpq-dev python3-dev

# macOS:
brew install postgresql

# Then retry:
pip install psycopg2-binary
```

**lxml build errors:**
```bash
# Install XML libraries
# Ubuntu/Debian:
sudo apt-get install libxml2-dev libxslt-dev

# macOS:
brew install libxml2 libxslt

# Then retry:
pip install lxml
```

### Disk Space Issues

Check available space:
```bash
df -h .
```

If running low:
- Use Simple English Wikipedia instead of full dump
- Use `--limit` parameter when importing (e.g., `--limit 10000` for 10,000 pages)
- Clean up Docker volumes: `docker-compose down -v`

### Memory Issues

If import crashes with memory errors:
- Reduce `--batch-size` parameter (default: 1000, try: 100)
- Close other applications
- For large dumps, consider using a machine with more RAM

## Environment Variables

You can customize database connections using environment variables:

```bash
# PostgreSQL
export PGHOST=localhost
export PGPORT=5432
export PGDATABASE=wikipedia
export PGUSER=wikiuser
export PGPASSWORD=wikipass

# MySQL
export MYSQL_HOST=localhost
export MYSQL_PORT=3306
export MYSQL_DATABASE=wikipedia
export MYSQL_USER=wikiuser
export MYSQL_PASSWORD=wikipass
```

## Cleanup

To remove all data and start fresh:

```bash
# Stop and remove containers (preserves data volumes)
docker-compose down

# Stop and remove containers AND data
docker-compose down -v

# Remove SQLite database
rm -f import/wikipedia.db
```

## Resource Requirements Summary

| Wikipedia Edition | Compressed Size | Uncompressed | Pages | RAM Recommended | Disk Space |
|-------------------|----------------|--------------|-------|-----------------|------------|
| Simple English    | ~200 MB        | ~1.5 GB      | ~200K | 4 GB            | 10 GB      |
| English (full)    | ~20 GB         | ~90 GB       | ~6M   | 16 GB           | 200 GB     |

## Next Steps

After completing setup, continue with:
- [Data Import Guide](IMPORTING.md)
- [Benchmarking Guide](BENCHMARKING.md)
- [Comparing with Links Platform](LINKS_PLATFORM.md)
