# Pattern Extraction Experiments

This directory contains experiments and examples for the GitHub Automation Pattern extraction, as described in issue [#472](https://github.com/Konard/LinksPlatform/issues/472).

## Purpose

Demonstrates how to extract reusable automation patterns from GitHub issues and apply them across repositories.

## Files

- **typo-bot-example.sh** - Example implementation of the TypoBot pattern from issue #471
- This demonstrates all four steps of the automation pattern:
  1. Problem Discovery
  2. Scale Assessment
  3. Manual Fix Limitation
  4. Automation Solution

## Running the Example

```bash
cd experiments/pattern-extraction
./typo-bot-example.sh
```

This is a dry-run example that demonstrates the pattern without making actual changes.

## Pattern Documentation

For the full pattern documentation, see:
- [GitHub Automation Pattern](../../doc/articles/github-automation-pattern.md)
- [Issue Template](../../.github/ISSUE_TEMPLATE/automation-pattern.md)
- [Workflow Template](../../.github/workflows/pattern-automation-template.yml.example)

## Applying the Pattern

To apply this pattern to a new automation:

1. Identify a problem in your repository
2. Search GitHub to assess scale (use GitHub Search API)
3. Confirm manual fixing is impractical
4. Design automation using the templates provided
5. Test locally using experiments
6. Deploy using GitHub Actions or a bot framework

## Related Issues

- [#471: Fix the typo everywhere (TypoBot)](https://github.com/Konard/LinksPlatform/issues/471) - Original example
- [#472: Pattern extraction from issues](https://github.com/Konard/LinksPlatform/issues/472) - This issue
- [#646: Pattern inference for GitHub bot or any automation](https://github.com/Konard/LinksPlatform/issues/646)
- [#482: Discover patterns related to changes on GitHub](https://github.com/Konard/LinksPlatform/issues/482)
