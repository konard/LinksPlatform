# -*- coding: utf-8 -*-
from typing import NoReturn, List, Optional
import requests
import re
import config


def get_default_programming_language(
    language: str
) -> str:
    """Returns default appearance of language
    """
    language = language.lower()
    for lang in config.DEFAULT_PROGRAMMING_LANGUAGES:
        if lang.replace('\\', '').lower() == language:
            return lang
    return ""


def contains_string(
    strings: List[str],
    matched_string: str,
    ignore_case: bool
) -> bool:
    """Returns True if `matched_string` in `strings`.

    :param strings: list of strings where contains matched string.
    :param matched_string: source string
    """
    if ignore_case:
        matched_string = matched_string.lower()
        for string in strings:
            if string.lower() == matched_string:
                return True
    else:
        return matched_string in strings
    return False


def contains_all_strings(
    strings: List[str],
    matched_strings: List[str],
    ignore_case: bool
) -> bool:
    """Returns True if `strings` in `matched_strings`.
    """
    matched_strings_count = len(matched_strings)
    for string in strings:
        if contains_string(matched_strings, string, ignore_case):
            matched_strings_count -= 1
            if matched_strings_count == 0:
                return True
    return False


def karma_limit(karma: int) -> int:
    """Returns karma hours limit.
    """
    for limit_item in config.KARMA_LIMIT_HOURS:
        if not limit_item["min_karma"] or karma >= limit_item["min_karma"]:
            if not limit_item["max_karma"] or karma < limit_item["max_karma"]:
                return limit_item["limit"]
    return 168  # hours (a week)


def is_available_ghpage(
    profile: str
) -> bool:
    """Returns True if github profile is available.
    """
    return requests.get(f'https://github.com/{profile}').status_code == 200


def extract_words(text: str) -> List[str]:
    """Extracts words from text, filtering out common words and special characters.

    :param text: message text
    :return: list of words
    """
    # Remove URLs
    text = re.sub(r'https?://\S+', '', text)
    # Remove mentions like [id123|@username]
    text = re.sub(r'\[id\d+\|@\w+\]', '', text)
    # Extract words (alphabetic characters only, 3+ chars)
    words = re.findall(r'\b[a-zA-Zа-яёА-ЯЁ]{3,}\b', text)
    return [w.lower() for w in words]


def is_rare_word(word: str) -> bool:
    """Determines if a word is rare/uncommon enough to warrant Wikipedia lookup.

    :param word: word to check
    :return: True if word appears to be rare/technical
    """
    # Skip if word is too short
    if len(word) < config.RARE_WORD_MIN_LENGTH:
        return False

    # Skip if in blacklist
    if word.lower() in config.RARE_WORD_BLACKLIST:
        return False

    # Common English words (very basic frequency list)
    common_words = {
        'people', 'would', 'which', 'their', 'there', 'about', 'think', 'really',
        'because', 'before', 'through', 'should', 'system', 'something', 'someone',
        'another', 'important', 'between', 'without', 'however', 'example', 'different',
        'problem', 'question', 'program', 'project', 'working', 'present', 'possible',
        'general', 'several', 'message', 'current', 'process', 'development'
    }

    # Common Russian words
    common_words_ru = {
        'потому', 'которые', 'сейчас', 'потому', 'можно', 'нужно', 'только', 'очень',
        'всего', 'может', 'всегда', 'никогда', 'больше', 'меньше', 'другие', 'другой',
        'например', 'конечно', 'поэтому', 'сделать', 'работа', 'проект', 'вопрос'
    }

    if word.lower() in common_words or word.lower() in common_words_ru:
        return False

    # Heuristics for rare words:
    # 1. Words with capital letters in the middle (likely proper nouns/technical terms)
    # 2. Words longer than 10 characters (often technical/scientific)
    # 3. Words with uncommon character patterns

    if len(word) >= 10:
        return True

    # Words with unusual patterns (3+ consonants in a row, scientific terms)
    if re.search(r'[bcdfghjklmnpqrstvwxz]{3,}', word.lower()):
        return True

    return False


def detect_rare_words(text: str) -> Optional[str]:
    """Detects rare/uncommon words in text that might warrant Wikipedia lookup.

    :param text: message text
    :return: first rare word found, or None
    """
    if not config.RARE_WORD_DETECTION_ENABLED:
        return None

    words = extract_words(text)
    for word in words:
        if is_rare_word(word):
            return word
    return None
