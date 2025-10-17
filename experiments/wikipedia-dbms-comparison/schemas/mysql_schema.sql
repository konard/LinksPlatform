-- MySQL Schema for Wikipedia Data
-- Based on MediaWiki database structure, simplified for comparison

-- Pages table: stores Wikipedia articles/pages
CREATE TABLE IF NOT EXISTS pages (
    page_id INT PRIMARY KEY,
    page_namespace INT NOT NULL DEFAULT 0,
    page_title VARCHAR(255) NOT NULL,
    page_is_redirect TINYINT(1) NOT NULL DEFAULT 0,
    page_latest INT,
    page_len INT NOT NULL DEFAULT 0,
    page_touched TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY (page_namespace, page_title),
    KEY idx_page_title (page_title),
    KEY idx_page_namespace (page_namespace)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Revisions table: stores edit history
CREATE TABLE IF NOT EXISTS revisions (
    rev_id INT PRIMARY KEY,
    rev_page INT NOT NULL,
    rev_text_id INT,
    rev_comment TEXT,
    rev_user INT DEFAULT 0,
    rev_user_text VARCHAR(255) DEFAULT '',
    rev_timestamp TIMESTAMP NOT NULL,
    rev_minor_edit TINYINT(1) NOT NULL DEFAULT 0,
    rev_deleted TINYINT(1) NOT NULL DEFAULT 0,
    rev_len INT,
    rev_parent_id INT,
    KEY idx_rev_page (rev_page),
    KEY idx_rev_timestamp (rev_timestamp),
    KEY idx_rev_user (rev_user),
    FOREIGN KEY (rev_page) REFERENCES pages(page_id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Page text content
CREATE TABLE IF NOT EXISTS page_text (
    text_id INT AUTO_INCREMENT PRIMARY KEY,
    text_content MEDIUMTEXT NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Page links: internal links between Wikipedia pages
CREATE TABLE IF NOT EXISTS pagelinks (
    pl_from INT NOT NULL,
    pl_namespace INT NOT NULL DEFAULT 0,
    pl_title VARCHAR(255) NOT NULL,
    pl_from_namespace INT NOT NULL DEFAULT 0,
    PRIMARY KEY (pl_from, pl_namespace, pl_title),
    KEY idx_pl_from (pl_from),
    KEY idx_pl_title (pl_title),
    KEY idx_pl_namespace (pl_namespace),
    FOREIGN KEY (pl_from) REFERENCES pages(page_id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Categories table
CREATE TABLE IF NOT EXISTS categories (
    cat_id INT AUTO_INCREMENT PRIMARY KEY,
    cat_title VARCHAR(255) NOT NULL UNIQUE,
    cat_pages INT NOT NULL DEFAULT 0,
    cat_subcats INT NOT NULL DEFAULT 0,
    cat_files INT NOT NULL DEFAULT 0,
    KEY idx_cat_title (cat_title)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Category links: connects pages to categories
CREATE TABLE IF NOT EXISTS categorylinks (
    cl_from INT NOT NULL,
    cl_to VARCHAR(255) NOT NULL,
    cl_sortkey VARCHAR(230) NOT NULL DEFAULT '',
    cl_timestamp TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    cl_type VARCHAR(20) NOT NULL DEFAULT 'page',
    PRIMARY KEY (cl_from, cl_to),
    KEY idx_cl_from (cl_from),
    KEY idx_cl_to (cl_to),
    KEY idx_cl_type (cl_type),
    FOREIGN KEY (cl_from) REFERENCES pages(page_id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- External links table
CREATE TABLE IF NOT EXISTS externallinks (
    el_id INT AUTO_INCREMENT PRIMARY KEY,
    el_from INT NOT NULL,
    el_to TEXT NOT NULL,
    el_index VARCHAR(255) NOT NULL,
    KEY idx_el_from (el_from),
    KEY idx_el_to (el_to(255)),
    FOREIGN KEY (el_from) REFERENCES pages(page_id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Redirects table
CREATE TABLE IF NOT EXISTS redirects (
    rd_from INT PRIMARY KEY,
    rd_namespace INT NOT NULL DEFAULT 0,
    rd_title VARCHAR(255) NOT NULL,
    rd_fragment VARCHAR(255),
    rd_interwiki VARCHAR(32),
    KEY idx_rd_title (rd_title),
    FOREIGN KEY (rd_from) REFERENCES pages(page_id) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Statistics and metadata
CREATE TABLE IF NOT EXISTS import_stats (
    stat_id INT AUTO_INCREMENT PRIMARY KEY,
    import_started TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    import_completed TIMESTAMP NULL,
    total_pages INT DEFAULT 0,
    total_revisions INT DEFAULT 0,
    total_links INT DEFAULT 0,
    total_categories INT DEFAULT 0,
    database_size BIGINT,
    notes TEXT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Insert initial stats record
INSERT INTO import_stats (notes) VALUES ('Initial schema creation');

-- View for common queries
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
