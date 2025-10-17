# TypoBot

Automated typo detection and fixing bot for GitHub repositories.

## Overview

TypoBot is a tool that searches for common typos across GitHub repositories and automatically creates pull requests to fix them. It was created to address [issue #471](https://github.com/konard/LinksPlatform/issues/471).

## Features

- 🔍 **Search GitHub** - Find repositories containing specific typos
- 🔧 **Automatic Fixing** - Replace typos with correct spelling
- 🤖 **PR Creation** - Automatically create pull requests with fixes
- 📝 **Configurable** - Define custom typos via JSON configuration
- 🧪 **Dry Run Mode** - Test without creating actual PRs
- ⚡ **GitHub Actions** - Run automatically on schedule or manually

## Installation

### Local Usage

1. Clone this repository
2. Install GitHub CLI: https://cli.github.com/
3. Authenticate with GitHub:
   ```bash
   gh auth login
   ```
4. Configure typos in `typos.json`

### GitHub Actions

The bot can run automatically via GitHub Actions:

1. Enable the workflow in `.github/workflows/typo-bot.yml`
2. Run manually from Actions tab or wait for scheduled run
3. Configure via workflow inputs

## Usage

### Command Line

```bash
# Dry run (search only, no PRs)
python3 typo_fixer.py --dry-run

# Fix typos in up to 10 repositories
python3 typo_fixer.py --max-repos 10

# Use custom config file
python3 typo_fixer.py --config my-typos.json

# Custom search query
python3 typo_fixer.py --search "teh" --max-repos 5
```

### Options

- `--config` - Path to typos configuration file (default: `typos.json`)
- `--dry-run` - Search and report without creating PRs
- `--max-repos` - Maximum number of repositories to process (default: 10)
- `--search` - Custom search query (default: first typo from config)

### GitHub Actions

Trigger manually from the Actions tab with custom parameters:

- **max_repos** - Number of repositories to process
- **dry_run** - Set to `false` to create actual PRs

## Configuration

Edit `typos.json` to define typos to fix:

```json
{
  "typos": [
    {
      "incorrect": "внезависимости",
      "correct": "независимости",
      "description": "Russian word for independence"
    }
  ]
}
```

## How It Works

1. **Search** - Uses GitHub CLI to search for repositories containing the typo
2. **Clone** - Clones each repository temporarily
3. **Scan** - Searches for typos in text files
4. **Fix** - Replaces incorrect text with correct spelling
5. **PR** - Creates a pull request with the fixes
6. **Cleanup** - Removes temporary files

## Example Workflow

```bash
# 1. Search for typos (dry run)
python3 typo_fixer.py --dry-run --max-repos 5

# Output:
# Found typos in 3 repositories:
#   - user1/repo1: 2 files
#   - user2/repo2: 1 file
#   - user3/repo3: 5 files

# 2. Create PRs for real
python3 typo_fixer.py --max-repos 5

# Output:
# ✓ Created PR for user1/repo1
# ✓ Created PR for user2/repo2
# ✓ Created PR for user3/repo3
```

## Safety Features

- **Rate limiting** - Adds delays between operations
- **Dry run mode** - Test before making changes
- **File type filtering** - Only processes text files
- **Error handling** - Gracefully handles failures
- **Temporary clones** - Automatic cleanup after processing

## Requirements

- Python 3.7+
- GitHub CLI (`gh`)
- Git
- GitHub account with appropriate permissions

## Limitations

- Requires GitHub CLI authentication
- Cannot create PRs to repositories you don't have access to
- Respects GitHub rate limits
- Simple string replacement (no regex support yet)

## Related Projects

- [Probot Ideas - Typo Correction Bot](https://github.com/probot/ideas/issues/56)
- [Orthographic Pedant](https://github.com/thoppe/orthographic-pedant)

## License

See [LICENSE](../LICENSE) file in the repository root.

## Contributing

This is part of the LinksPlatform project. See the main repository for contributing guidelines.

## Author

Created to solve [issue #471](https://github.com/konard/LinksPlatform/issues/471) - Fix the typo everywhere (TypoBot).
