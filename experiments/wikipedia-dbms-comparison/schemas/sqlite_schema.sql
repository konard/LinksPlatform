-- SQLite Schema for Wikipedia Data
-- Based on MediaWiki database structure, simplified for comparison

-- Pages table: stores Wikipedia articles/pages
CREATE TABLE IF NOT EXISTS pages (
    page_id INTEGER PRIMARY KEY,
    page_namespace INTEGER NOT NULL DEFAULT 0,
    page_title TEXT NOT NULL,
    page_is_redirect INTEGER NOT NULL DEFAULT 0,
    page_latest INTEGER,
    page_len INTEGER NOT NULL DEFAULT 0,
    page_touched TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(page_namespace, page_title)
);

CREATE INDEX IF NOT EXISTS idx_page_title ON pages(page_title);
CREATE INDEX IF NOT EXISTS idx_page_namespace ON pages(page_namespace);

-- Revisions table: stores edit history
CREATE TABLE IF NOT EXISTS revisions (
    rev_id INTEGER PRIMARY KEY,
    rev_page INTEGER NOT NULL,
    rev_text_id INTEGER,
    rev_comment TEXT,
    rev_user INTEGER DEFAULT 0,
    rev_user_text TEXT DEFAULT '',
    rev_timestamp TEXT NOT NULL,
    rev_minor_edit INTEGER NOT NULL DEFAULT 0,
    rev_deleted INTEGER NOT NULL DEFAULT 0,
    rev_len INTEGER,
    rev_parent_id INTEGER,
    FOREIGN KEY (rev_page) REFERENCES pages(page_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_rev_page ON revisions(rev_page);
CREATE INDEX IF NOT EXISTS idx_rev_timestamp ON revisions(rev_timestamp);
CREATE INDEX IF NOT EXISTS idx_rev_user ON revisions(rev_user);

-- Page text content
CREATE TABLE IF NOT EXISTS page_text (
    text_id INTEGER PRIMARY KEY AUTOINCREMENT,
    text_content TEXT NOT NULL
);

-- Page links: internal links between Wikipedia pages
CREATE TABLE IF NOT EXISTS pagelinks (
    pl_from INTEGER NOT NULL,
    pl_namespace INTEGER NOT NULL DEFAULT 0,
    pl_title TEXT NOT NULL,
    pl_from_namespace INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (pl_from, pl_namespace, pl_title),
    FOREIGN KEY (pl_from) REFERENCES pages(page_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_pl_from ON pagelinks(pl_from);
CREATE INDEX IF NOT EXISTS idx_pl_title ON pagelinks(pl_title);
CREATE INDEX IF NOT EXISTS idx_pl_namespace ON pagelinks(pl_namespace);

-- Categories table
CREATE TABLE IF NOT EXISTS categories (
    cat_id INTEGER PRIMARY KEY AUTOINCREMENT,
    cat_title TEXT NOT NULL UNIQUE,
    cat_pages INTEGER NOT NULL DEFAULT 0,
    cat_subcats INTEGER NOT NULL DEFAULT 0,
    cat_files INTEGER NOT NULL DEFAULT 0
);

CREATE INDEX IF NOT EXISTS idx_cat_title ON categories(cat_title);

-- Category links: connects pages to categories
CREATE TABLE IF NOT EXISTS categorylinks (
    cl_from INTEGER NOT NULL,
    cl_to TEXT NOT NULL,
    cl_sortkey TEXT NOT NULL DEFAULT '',
    cl_timestamp TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    cl_type TEXT NOT NULL DEFAULT 'page',
    PRIMARY KEY (cl_from, cl_to),
    FOREIGN KEY (cl_from) REFERENCES pages(page_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_cl_from ON categorylinks(cl_from);
CREATE INDEX IF NOT EXISTS idx_cl_to ON categorylinks(cl_to);
CREATE INDEX IF NOT EXISTS idx_cl_type ON categorylinks(cl_type);

-- External links table
CREATE TABLE IF NOT EXISTS externallinks (
    el_id INTEGER PRIMARY KEY AUTOINCREMENT,
    el_from INTEGER NOT NULL,
    el_to TEXT NOT NULL,
    el_index TEXT NOT NULL,
    FOREIGN KEY (el_from) REFERENCES pages(page_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_el_from ON externallinks(el_from);
CREATE INDEX IF NOT EXISTS idx_el_to ON externallinks(el_to);

-- Redirects table
CREATE TABLE IF NOT EXISTS redirects (
    rd_from INTEGER PRIMARY KEY,
    rd_namespace INTEGER NOT NULL DEFAULT 0,
    rd_title TEXT NOT NULL,
    rd_fragment TEXT,
    rd_interwiki TEXT,
    FOREIGN KEY (rd_from) REFERENCES pages(page_id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_rd_title ON redirects(rd_title);

-- Statistics and metadata
CREATE TABLE IF NOT EXISTS import_stats (
    stat_id INTEGER PRIMARY KEY AUTOINCREMENT,
    import_started TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
    import_completed TEXT,
    total_pages INTEGER DEFAULT 0,
    total_revisions INTEGER DEFAULT 0,
    total_links INTEGER DEFAULT 0,
    total_categories INTEGER DEFAULT 0,
    database_size INTEGER,
    notes TEXT
);

-- Insert initial stats record
INSERT INTO import_stats (notes) VALUES ('Initial schema creation');

-- View for common queries
CREATE VIEW IF NOT EXISTS page_summary AS
SELECT
    p.page_id,
    p.page_title,
    p.page_namespace,
    COUNT(DISTINCT pl.pl_title) as outgoing_links,
    COUNT(DISTINCT r.rev_id) as revision_count,
    MAX(r.rev_timestamp) as last_modified
FROM pages p
LEFT JOIN pagelinks pl ON p.page_id = pl.pl_from
LEFT JOIN revisions r ON p.page_id = r.rev_page
GROUP BY p.page_id, p.page_title, p.page_namespace;

-- Enable foreign keys (important for SQLite)
PRAGMA foreign_keys = ON;

-- Performance optimizations for SQLite
PRAGMA journal_mode = WAL;
PRAGMA synchronous = NORMAL;
PRAGMA temp_store = MEMORY;
PRAGMA mmap_size = 30000000000;
PRAGMA page_size = 4096;
