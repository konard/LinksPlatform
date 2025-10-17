# Rare Word Detection Feature

## Overview
This feature automatically detects rare/uncommon words in chat messages and provides Wikipedia links for them. It complements the existing "What is X?" functionality by proactively helping users when technical or uncommon terms are used.

## Implementation Details

### Configuration (config.py)
Added the following configuration options:

```python
# Rare word detection configuration
RARE_WORD_DETECTION_ENABLED = True
RARE_WORD_MIN_LENGTH = 6  # Minimum word length to consider
RARE_WORD_BLACKLIST = [
    # Common programming/technical terms that shouldn't trigger Wikipedia
    'github', 'python', 'javascript', 'typescript', 'database', 'programming',
    'function', 'method', 'class', 'object', 'variable', 'string', 'integer',
    'boolean', 'array', 'list', 'dictionary', 'algorithm', 'compile', 'runtime'
]
# Chats where rare word detection is enabled
CHATS_RARE_WORD_WHITELIST = [
    2000000001,
    2000000011
]
```

### Core Functions (modules/utils.py)

#### `extract_words(text: str) -> List[str]`
Extracts words from text while filtering out:
- URLs
- VK mentions (e.g., `[id123|@username]`)
- Special characters
- Words shorter than 3 characters

#### `is_rare_word(word: str) -> bool`
Determines if a word is rare using heuristics:
- Must be at least 6 characters long
- Not in the blacklist
- Not in common English/Russian word lists
- Matches one of these patterns:
  - Length >= 10 characters (likely technical/scientific)
  - Contains 3+ consecutive consonants (unusual pattern)

#### `detect_rare_words(text: str) -> Optional[str]`
Scans message text and returns the first rare word found, or None.

### Integration (modules/commands.py)

#### Modified `what_is()` method
Refactored to use a shared `_search_and_send_wikipedia()` helper method.

#### New `check_rare_words()` method
Called automatically when no explicit command matches. Checks if:
- Chat is in the whitelist
- Message contains rare words
- If yes, automatically searches Wikipedia and sends results

#### Updated `process()` method
Added rare word detection as a fallback when no command matches:

```python
for cmd, action in Commands.cmds.items():
    self.match_command(cmd)
    if self.matched:
        action()
        return

# If no command matched, check for rare words
self.check_rare_words()
```

## Testing

Added comprehensive unit tests in `tests.py`:
- `test_extract_words()` - Verifies word extraction and filtering
- `test_is_rare_word()` - Tests rare word detection logic
- `test_detect_rare_words()` - Tests end-to-end detection in messages

## Usage Examples

### Manual "What is X?" Query
User: "What is photosynthesis?"
Bot: "Photosynthesis is a process... en.wikipedia.org/wiki/Photosynthesis"

### Automatic Rare Word Detection
User: "I'm studying electromagnetism for my physics exam"
Bot: [Automatically detects "electromagnetism" and sends Wikipedia info]

## Benefits

1. **Proactive Help**: Users don't need to explicitly ask "What is X?"
2. **Educational**: Helps users learn about technical terms naturally
3. **Configurable**: Easy to adjust sensitivity via config settings
4. **Non-intrusive**: Only triggers in whitelisted chats
5. **Smart Filtering**: Avoids common false positives through blacklisting

## Future Improvements

1. Use actual word frequency databases (e.g., Google NGrams, NLTK)
2. Machine learning-based rarity detection
3. User preference settings per chat
4. Cooldown mechanism to avoid spam
5. Support for multi-word technical terms
