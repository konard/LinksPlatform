-- PostgreSQL Schema for Wikipedia Data
-- Based on MediaWiki database structure, simplified for comparison

-- Pages table: stores Wikipedia articles/pages
CREATE TABLE IF NOT EXISTS pages (
    page_id INTEGER PRIMARY KEY,
    page_namespace INTEGER NOT NULL DEFAULT 0,
    page_title VARCHAR(255) NOT NULL,
    page_is_redirect BOOLEAN NOT NULL DEFAULT FALSE,
    page_latest INTEGER,  -- Latest revision ID
    page_len INTEGER NOT NULL DEFAULT 0,
    page_touched TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(page_namespace, page_title)
);

CREATE INDEX idx_page_title ON pages(page_title);
CREATE INDEX idx_page_namespace ON pages(page_namespace);

-- Revisions table: stores edit history
CREATE TABLE IF NOT EXISTS revisions (
    rev_id INTEGER PRIMARY KEY,
    rev_page INTEGER NOT NULL REFERENCES pages(page_id) ON DELETE CASCADE,
    rev_text_id INTEGER,
    rev_comment TEXT,
    rev_user INTEGER DEFAULT 0,
    rev_user_text VARCHAR(255) DEFAULT '',
    rev_timestamp TIMESTAMP NOT NULL,
    rev_minor_edit BOOLEAN NOT NULL DEFAULT FALSE,
    rev_deleted BOOLEAN NOT NULL DEFAULT FALSE,
    rev_len INTEGER,
    rev_parent_id INTEGER
);

CREATE INDEX idx_rev_page ON revisions(rev_page);
CREATE INDEX idx_rev_timestamp ON revisions(rev_timestamp);
CREATE INDEX idx_rev_user ON revisions(rev_user);

-- Page text content
CREATE TABLE IF NOT EXISTS page_text (
    text_id SERIAL PRIMARY KEY,
    text_content TEXT NOT NULL
);

-- Page links: internal links between Wikipedia pages
CREATE TABLE IF NOT EXISTS pagelinks (
    pl_from INTEGER NOT NULL REFERENCES pages(page_id) ON DELETE CASCADE,
    pl_namespace INTEGER NOT NULL DEFAULT 0,
    pl_title VARCHAR(255) NOT NULL,
    pl_from_namespace INTEGER NOT NULL DEFAULT 0,
    PRIMARY KEY (pl_from, pl_namespace, pl_title)
);

CREATE INDEX idx_pl_from ON pagelinks(pl_from);
CREATE INDEX idx_pl_title ON pagelinks(pl_title);
CREATE INDEX idx_pl_namespace ON pagelinks(pl_namespace);

-- Categories table
CREATE TABLE IF NOT EXISTS categories (
    cat_id SERIAL PRIMARY KEY,
    cat_title VARCHAR(255) NOT NULL UNIQUE,
    cat_pages INTEGER NOT NULL DEFAULT 0,
    cat_subcats INTEGER NOT NULL DEFAULT 0,
    cat_files INTEGER NOT NULL DEFAULT 0
);

CREATE INDEX idx_cat_title ON categories(cat_title);

-- Category links: connects pages to categories
CREATE TABLE IF NOT EXISTS categorylinks (
    cl_from INTEGER NOT NULL REFERENCES pages(page_id) ON DELETE CASCADE,
    cl_to VARCHAR(255) NOT NULL,
    cl_sortkey VARCHAR(230) NOT NULL DEFAULT '',
    cl_timestamp TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    cl_type VARCHAR(20) NOT NULL DEFAULT 'page',
    PRIMARY KEY (cl_from, cl_to)
);

CREATE INDEX idx_cl_from ON categorylinks(cl_from);
CREATE INDEX idx_cl_to ON categorylinks(cl_to);
CREATE INDEX idx_cl_type ON categorylinks(cl_type);

-- External links table
CREATE TABLE IF NOT EXISTS externallinks (
    el_id SERIAL PRIMARY KEY,
    el_from INTEGER NOT NULL REFERENCES pages(page_id) ON DELETE CASCADE,
    el_to TEXT NOT NULL,
    el_index TEXT NOT NULL
);

CREATE INDEX idx_el_from ON externallinks(el_from);
CREATE INDEX idx_el_to ON externallinks(el_to(255));

-- Redirects table
CREATE TABLE IF NOT EXISTS redirects (
    rd_from INTEGER PRIMARY KEY REFERENCES pages(page_id) ON DELETE CASCADE,
    rd_namespace INTEGER NOT NULL DEFAULT 0,
    rd_title VARCHAR(255) NOT NULL,
    rd_fragment VARCHAR(255),
    rd_interwiki VARCHAR(32)
);

CREATE INDEX idx_rd_title ON redirects(rd_title);

-- Statistics and metadata
CREATE TABLE IF NOT EXISTS import_stats (
    stat_id SERIAL PRIMARY KEY,
    import_started TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    import_completed TIMESTAMP,
    total_pages INTEGER DEFAULT 0,
    total_revisions INTEGER DEFAULT 0,
    total_links INTEGER DEFAULT 0,
    total_categories INTEGER DEFAULT 0,
    database_size BIGINT,
    notes TEXT
);

-- Insert initial stats record
INSERT INTO import_stats (notes) VALUES ('Initial schema creation');

-- Views for common queries
CREATE OR REPLACE VIEW page_summary AS
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

COMMENT ON TABLE pages IS 'Stores Wikipedia articles and pages';
COMMENT ON TABLE revisions IS 'Stores edit history for all pages';
COMMENT ON TABLE pagelinks IS 'Stores internal links between Wikipedia pages';
COMMENT ON TABLE categories IS 'Stores category information';
COMMENT ON TABLE categorylinks IS 'Links pages to their categories';
