# -*- coding: utf-8 -*-
"""Provides config around bot.
"""

NEGATIVE_OPERATORS = ['-', 'False', 'No', '👎']
POSITIVE_OPERATORS = ['Thank you', 'Спасибо', 'Благодарю', '+', 'True', 'Yes', '👍']

# Bot's VK group id (change it to your group id)
BOT_GROUP_ID = 190877945

# Pluses and minuses will be removed in these chats
CHATS_DELETING = [
    #2000000001,
    #2000000006
]

# Check your search line, when you`re in the needed chat.
# Then copy it`s id after "vk.com/im?peers=c"
USERBOT_CHATS = {
    #2000000001: 477,
    #2000000006: 423
}

# Chats where you can change reputation of other users
CHATS_KARMA_WHITELIST = [
    2000000001,
    2000000011
]

POSITIVE_VOTES_PER_KARMA = 2
NEGATIVE_VOTES_PER_KARMA = 3

KARMA_LIMIT_HOURS = [
    { "min_karma": None, "max_karma": -19,  "limit": 8 },
    { "min_karma": -19,  "max_karma": -1,   "limit": 4 },
    { "min_karma": -1,  "max_karma": 2,   "limit": 2 },
    { "min_karma": 2,  "max_karma": 20,   "limit": 1 },
    { "min_karma": 20,   "max_karma": None, "limit": 0.5 },
]

DEFAULT_PROGRAMMING_LANGUAGES = [
    r"Assembler",
    r"JavaScript",
    r"TypeScript",
    r"Java",
    r"Python",
    r"PHP",
    r"Ruby",
    r"C\+\+",
    r"C",
    r"Shell",
    r"C#",
    r"Objective\-C",
    r"R",
    r"VimL",
    r"Go",
    r"Perl",
    r"CoffeeScript",
    r"TeX",
    r"Swift",
    r"Kotlin",
    r"F#",
    r"Scala",
    r"Scheme",
    r"Emacs Lisp",
    r"Lisp",
    r"Haskell",
    r"Lua",
    r"Clojure",
    r"TLA\+",
    r"PlusCal",
    r"Matlab",
    r"Groovy",
    r"Puppet",
    r"Rust",
    r"PowerShell",
    r"Pascal",
    r"Delphi",
    r"SQL",
    r"Nim",
    r"1С",
    r"КуМир",
    r"Scratch",
    r"Prolog",
    r"GLSL",
    r"HLSL",
    r"Whitespace",
    r"Basic",
    r"Visual Basic",
    r"Parser",
    r"Erlang",
    r"Wolfram",
    r"Brainfuck",
    r"Pawn",
    r"Cobol",
    r"Fortran",
    r"Arduino",
    r"Makefile",
    r"CMake",
    r"D",
    r"Forth",
    r"Dart",
    r"Ada",
    r"Julia",
    r"Malbolge",
    r"Лого",
    r"Verilog",
    r"VHDL",
    r"Altera",
    r"Processing",
    r"MetaQuotes",
    r"Algol",
    r"Piet",
    r"Shakespeare",
    r"G\-code",
    r"Whirl",
    r"Chef",
    r"BIT",
    r"Ook",
    r"MoonScript",
    r"PureScript",
    r"Idris",
    r"Elm",
    r"Minecraft",
    r"Crystal",
    r"C\-\-",
    r"Go\!",
    r"Tcl",
    r"Solidity",
    r"AssemblyScript",
    r"Vimscript",
    r"Pony",
    r"LOLCODE",
    r"Elixir",
    r"X#",
    r"NVPTX",
    r"Nemerle",
]

GITHUB_COPILOT_LANGUAGES = {
    r'Python': ['.py', 'python'],  # file extension, pastebin name
    r'JavaScript': ['.js', 'javascript'],
    r'TypeScript': ['.ts', 'typescript'],
    r'C#': ['.cs', 'csharp'],
    r'Go': ['.go', 'go'],
    r'Java': ['.java', 'java'],
    r'Kotlin': ['.kt', 'kotlin'],
    r'Ruby': ['.rb', 'ruby'],
    r'PHP': ['.php', 'php'],
    r'C': ['.c', 'c'],
    r'C\+\+': ['.cpp', 'cpp'],
}
GITHUB_COPILOT_RUN_COMMAND = 'bash -c "./copilot.sh {input_file} {output_file}"'
GITHUB_COPILOT_TIMEOUT = 120  # seconds

DEFAULT_PROGRAMMING_LANGUAGES_PATTERN_STRING = "|".join(DEFAULT_PROGRAMMING_LANGUAGES)
GITHUB_COPILOT_LANGUAGES_PATTERN_STRING = "|".join([i for i in GITHUB_COPILOT_LANGUAGES.keys()])

# Rare word detection configuration
RARE_WORD_DETECTION_ENABLED = True
RARE_WORD_MIN_LENGTH = 6  # Minimum word length to consider for rare word detection
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
