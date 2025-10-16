# Downloading Old/Dead Pages from the Internet Archive

## Table of Contents
* [Introduction](#introduction)
* [Prerequisites](#prerequisites)
* [Method 1: Wayback Machine Downloader (Recommended)](#method-1-wayback-machine-downloader-recommended)
* [Method 2: wget Command](#method-2-wget-command)
* [Method 3: Manual Download](#method-3-manual-download)
* [Tips and Best Practices](#tips-and-best-practices)

## Introduction

The [Internet Archive Wayback Machine](https://web.archive.org) is a digital archive of the World Wide Web that allows you to access historical versions of websites. This tutorial explains various methods to download old or dead pages from the Wayback Machine for archival or research purposes.

## Prerequisites

Choose the prerequisites based on the method you want to use:

* **For Method 1**: Ruby installed on your system
* **For Method 2**: wget utility (usually pre-installed on Linux/macOS)
* **For Method 3**: Any web browser
* Internet connection
* Basic command-line knowledge (for Methods 1 and 2)

## Method 1: Wayback Machine Downloader (Recommended)

The [Wayback Machine Downloader](https://github.com/hartator/wayback-machine-downloader) is a Ruby gem that provides the most straightforward way to download entire websites from the Wayback Machine.

### Installation

```bash
gem install wayback_machine_downloader
```

### Basic Usage

To download a website:

```bash
wayback_machine_downloader http://example.com
```

### Advanced Options

**Download with multiple concurrent threads** (faster):
```bash
wayback_machine_downloader http://example.com -c 5
```

**Download from a specific timestamp**:
```bash
wayback_machine_downloader http://example.com -t 20150101000000
```

**Download only specific file types**:
```bash
wayback_machine_downloader http://example.com -f html,css,js
```

**Download to a specific directory**:
```bash
wayback_machine_downloader http://example.com -d ./my-archive
```

**Resume interrupted downloads**:
```bash
wayback_machine_downloader http://example.com -d ./my-archive
```
(The tool automatically detects and skips already downloaded files)

## Method 2: wget Command

For users familiar with command-line tools, wget can be used to download archived pages.

### Basic wget Command

```bash
wget -rc --accept-regex '.*web.archive.org/web/[0-9]*/http://example.com.*' \
     https://web.archive.org/web/*/http://example.com
```

### Explanation of Flags

* `-r` - Recursive download
* `-c` - Continue interrupted downloads
* `--accept-regex` - Only download URLs matching the pattern
* The pattern ensures you download archived content, not the Wayback Machine's interface

### Download Specific Snapshot

To download a specific timestamp:

```bash
wget -r -np -N -E -p -k \
     https://web.archive.org/web/20150101000000/http://example.com
```

* `-np` - No parent directories
* `-N` - Only download newer files
* `-E` - Adjust extensions (saves .html files properly)
* `-p` - Download all page requisites (images, CSS, etc.)
* `-k` - Convert links for local viewing

## Method 3: Manual Download

For downloading individual pages or small sites:

1. Visit [https://web.archive.org](https://web.archive.org)
2. Enter the URL of the dead/old website in the search box
3. Browse the calendar to find the snapshot date you want
4. Click on the timestamp to view the archived page
5. Use your browser's "Save Page As" function (usually Ctrl+S or Cmd+S)
6. Choose "Webpage, Complete" to save HTML with assets

## Tips and Best Practices

### Rate Limiting

Be respectful of the Internet Archive's servers:

* Use reasonable concurrent download limits (2-5 threads maximum)
* Add delays between requests if downloading large sites
* Consider downloading during off-peak hours

### Timestamp Selection

* The Wayback Machine may have multiple snapshots per day
* Earlier timestamps may have incomplete captures
* Later timestamps may have better asset coverage
* Use the calendar view to find the most complete snapshot

### File Organization

* Keep downloaded archives in dated folders (e.g., `website-YYYYMMDD/`)
* Maintain a log of what you downloaded and when
* Document the original URL and archive timestamp

### Handling Errors

**Missing Files**: Not all resources may have been archived. The Wayback Machine crawler may have missed some assets.

**JavaScript-Heavy Sites**: Sites relying heavily on JavaScript may not render correctly from archives, as dynamic content may not have been captured.

**Redirect Issues**: Some archive URLs redirect to the most recent snapshot. Specify exact timestamps to avoid this.

### Legal and Ethical Considerations

* Respect copyright and terms of service
* Downloaded content is for personal archival and research
* Do not republish archived content without permission
* Check the site's robots.txt historical policy

## Alternative Tools

* **ArchiveBox**: Self-hosted web archiving system that can import from Wayback Machine
* **WebCite**: Alternative archiving service
* **wget-warc**: Extension of wget for creating WARC files (Web ARChive format)

## Troubleshooting

**Problem**: Download is very slow

**Solution**: Use concurrent downloads (`-c` flag) or try during off-peak hours

---

**Problem**: Getting 429 (Too Many Requests) errors

**Solution**: Reduce concurrent threads and add delays between requests

---

**Problem**: Downloaded site has broken links

**Solution**: Use wget with `-k` flag to convert links for local viewing, or manually adjust relative paths

## Resources

* [Internet Archive Wayback Machine](https://web.archive.org)
* [Wayback Machine Downloader GitHub](https://github.com/hartator/wayback-machine-downloader)
* [wget Manual](https://www.gnu.org/software/wget/manual/)
* [Original Question Reference](http://superuser.com/questions/828907/how-to-download-a-website-from-the-archive-org-wayback-machine)

---

*This tutorial was created to help preserve and access historical web content from the Internet Archive.*
