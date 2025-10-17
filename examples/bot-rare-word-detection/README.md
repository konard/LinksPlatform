# Wikipedia Chat Bot - Rare Word Detection Implementation

This directory contains the implementation for issue #516: "What is X?" chat bot with automatic rare word detection.

## Status

✅ **"What is X?" functionality** - Already implemented in [linksplatform/Bot](https://github.com/linksplatform/Bot/blob/main/python/modules/commands.py#L311-L331)

✅ **Automatic rare word detection** - NEW implementation provided here

## Files

- **RARE_WORD_DETECTION.md** - Comprehensive documentation of the feature
- **config_additions.py** - Configuration additions for `config.py`
- **utils_additions.py** - New utility functions for `modules/utils.py`

## Implementation Overview

The implementation adds automatic detection of rare/uncommon words in chat messages and provides Wikipedia information about them without requiring an explicit "What is X?" question.

### Key Features

1. **Heuristic-based detection**: Uses word length, character patterns, and frequency lists
2. **Configurable blacklist**: Prevents false positives for common technical terms
3. **Chat whitelisting**: Only active in designated chats
4. **Non-intrusive**: Works as a fallback when no command matches
5. **Bilingual support**: Works with both English and Russian

### How It Works

```
User: "I'm studying photosynthesis for my exam"
  ↓
Bot detects "photosynthesis" as rare word
  ↓
Bot searches Wikipedia
  ↓
Bot: "Photosynthesis is a process used by plants... en.wikipedia.org/wiki/Photosynthesis"
```

## Integration Steps

To integrate this into the linksplatform/Bot repository:

1. Add configuration from `config_additions.py` to `python/config.py`
2. Add utility functions from `utils_additions.py` to `python/modules/utils.py`
3. Update `python/modules/__init__.py` to export new functions
4. Modify `python/modules/commands.py`:
   - Import `detect_rare_words`
   - Refactor `what_is()` to use `_search_and_send_wikipedia()`
   - Add `check_rare_words()` method
   - Call `check_rare_words()` in `process()` method
5. Add tests from `tests.py` for the new functionality

## Testing

Unit tests are provided to verify:
- Word extraction from messages
- Rare word detection logic
- End-to-end rare word detection

Run tests with:
```bash
cd python
python3 -m unittest tests.Test4RareWordDetection
```

## Related Links

- Original Issue: https://github.com/konard/LinksPlatform/issues/516
- Bot Repository: https://github.com/linksplatform/Bot
- Existing Implementation: [commands.py](https://github.com/linksplatform/Bot/blob/main/python/modules/commands.py)

## Next Steps

This implementation is ready to be submitted as a pull request to the linksplatform/Bot repository.
