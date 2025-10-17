# GitHub Issues Relations Analysis

This document reveals the relationships and dependencies between issues in the LinksPlatform repository.

## Most Referenced Issues

These issues are referenced by other issues and may be foundational:

| Issue | References | Title |
|-------|------------|-------|
| #119 | 4 | Translate Everything |
| #662 | 2 | Infinitely precise universal search engine  |
| #596 | 2 | Search engine with public list of all search queries |
| #539 | 2 | Text storage |
| #607 | 2 | Sync everything |
| #216 | 2 | ﻿Links File System |
| #110 | 2 | Patterns |
| #10 | 2 | Triggers |
| #57 | 2 | Traveler/Walker/Visiter/Crawler/Browser |
| #514 | 1 | Universal search engine for complex object with adjustable search form |
| #340 | 1 | GitHub Similar organizations/repositories/issues/file search |
| #661 | 1 | Market with infinitely precise search |
| #121 | 1 | Sequences search using autocomplete |
| #122 | 1 | Constant search concept or goal completion assist |
| #220 | 1 | Continuous search refinement | Непрерывное уточнение поиска |
| #541 | 1 | Convert everything |
| #306 | 1 | Media event comments grouping by sense |
| #599 | 1 | Each table as a virtual links structure |
| #584 | 1 | International phonetic alphabet translator |
| #483 | 1 | Black box for operation systems |

## Issues with Explicit Dependencies

### #665: Better Search Engine (beyond Google)

- **Mentioned**: #121 - Sequences search using autocomplete
- **Mentioned**: #122 - Constant search concept or goal completion assist
- **Mentioned**: #220 - Continuous search refinement | Непрерывное уточнение поиска
- **Mentioned**: #340 - GitHub Similar organizations/repositories/issues/file search
- **Mentioned**: #514 - Universal search engine for complex object with adjustable search form
- **Mentioned**: #539 - Text storage
- **Mentioned**: #596 - Search engine with public list of all search queries
- **Mentioned**: #661 - Market with infinitely precise search
- **Mentioned**: #662 - Infinitely precise universal search engine 

### #661: Market with infinitely precise search

- **Required By**: #662 - Infinitely precise universal search engine 

### #608: Translate/Sync all open-source projects/packages/libraries to all programming languages.

- **Related To**: #119 - Translate Everything
- **Related To**: #607 - Sync everything

### #607: Sync everything

- **Mentioned**: #119 - Translate Everything
- **Mentioned**: #541 - Convert everything

### #606: Translate/Sync all Wikipedia articles to all natural languages and join content of articles with the same meaning

- **Related To**: #119 - Translate Everything
- **Related To**: #607 - Sync everything

### #602: Collection and Aggregation of Comments and Feedback from all public and private platforms in the internet

- **Related To**: #306 - Media event comments grouping by sense

### #600: Links and SQL synchronization

- **Related To**: #599 - Each table as a virtual links structure

### #585: Humanity dictionary

- **Mentioned**: #584 - International phonetic alphabet translator

### #555: Software logs storage

- **Mentioned**: #29 - Links data structure as a binary data protocal (with optional compression using variable length numbers/sequences) (alternative to JSON, XML, ...)
- **Mentioned**: #95 - Compression optimization
- **Mentioned**: #483 - Black box for operation systems

### #540: Code storage

- **Mentioned**: #539 - Text storage

### #539: Text storage

- **Mentioned**: #216 - ﻿Links File System
- **Mentioned**: #540 - Code storage

### #505: Unfake or real users database or identification service

- **Mentioned**: #504 - Requests and responses open knowledge base

### #504: Requests and responses open knowledge base

- **Mentioned**: #503 - Facts open database
- **Mentioned**: #505 - Unfake or real users database or identification service

### #488: Programming languages translation

- **Mentioned**: #119 - Translate Everything
- **Mentioned**: #135 - Inter/Intermediate/Abstract Semantic Language or Linguistics API | Универсальный Язык Смысла
- **Mentioned**: #168 - GitHub extension/app for automatic language translation

### #484: Better recovery system for operating systems

- **Related To**: #216 - ﻿Links File System

### #483: Black box for operation systems

- **Related To**: #164 - Vectorization

### #474: The database is a log, not a snapshot (approach)

- **Mentioned**: #466 - "Create-only" or "no deletion" mode

### #472: From every issue on GitHub it is possible to extract the pattern that can be automatically applied to all repositories

- **Mentioned**: #471 - Fix the typo everywhere (TypoBot)

### #456: Attempt to implement data layer for CMS systems

- **Related To**: #455 - Become a part of .NET Foundation

### #220: Continuous search refinement | Непрерывное уточнение поиска

- **Mentioned**: #596 - Search engine with public list of all search queries

### #211: Decentralized computational network (grid) with code+knowledge encyclopedia included that anyone can edit

- **Related To**: #127 - Integrate with everything

### #209: Make Links parts number (2 for pairs, 3 for triples) changable (as setting or option)

- **Mentioned**: #62 - Reimplement triple (tuple) links implementation using C#

### #207: Random access links API

- **Mentioned**: #208 - Manage Links memory using ranges list instead of just linked list

### #204: Is this a word?

- **Mentioned**: #179 - Reference collections for each small data type backed by github using LiNo
- **Mentioned**: #203 - Make an algorithm to find all basic worlds of any explanatory dictionary of any language

### #187: Flexibility (functionality) and maximum performance at the same time for ILinks

- **Mentioned**: #105 - Virtual Programmer GitHub Bot (repository as intermediate solution, issues as user requests/requirements description or decision making)

### #155: Package C/C++ libraries with one of the package managers

- **Related To**: #66 - Publish Links Platform Library as a NuGet

### #138: Data importers

- **Mentioned**: #158 - Implement parser/generator for LiNo (Links Notation) (like JSON for Links)

### #126: Complete wiki documentation

- **Mentioned**: #198 - Try to build DocFX website for Links Platform

### #121: Sequences search using autocomplete

- **Mentioned**: #110 - Patterns

### #111: Object or pattern?

- **Mentioned**: #78 - Objects (static type system & dynamic type system)
- **Mentioned**: #100 - Sets
- **Mentioned**: #110 - Patterns

### #101: Undo/Revert mechanism for transactions

- **Mentioned**: #37 - Durability / Transaction Log
- **Mentioned**: #40 - Atomicity

### #95: Compression optimization

- **Mentioned**: #9 - Implement stable algorithm for optimal subsequence groupings (minification of total sequences size) aka Compression

### #86: Floating point numbers

- **Mentioned**: #10 - Triggers

### #85: Virtual links

- **Mentioned**: #10 - Triggers
- **Related To**: #57 - Traveler/Walker/Visiter/Crawler/Browser

### #77: Function to convert Link to Array

- **Mentioned**: #57 - Traveler/Walker/Visiter/Crawler/Browser

### #59: Add event log (show changes in real time) to MasterServer

- **Related To**: #19 - Test out Evented IO style for API of Links (CRUD, native-triggers on CRUD events)

### #55: UDP socket cannot be sent to itself to stop the worker thread of UdpReceiver (on mono)

- **Mentioned**: #54 - Console.Terminal.exe does not receive first character in line (on mono)

### #51: Implement "In Memory" Memory or Heap Memory abstraction to test out Pairs in Memory

- **Mentioned**: #64 - Make all system modules more slim, modular, loosely coupled (later on this should allow fully configurable links environment)

## Issues Grouped by Labels

### Confirmed Bug

- #552: Update Web Terminal to the latest version of ASP.NET Core
- #55: UDP socket cannot be sent to itself to stop the worker thread of UdpReceiver (on mono)
- #54: Console.Terminal.exe does not receive first character in line (on mono)

### High Priority

- #580: Remove Xml Importer/Exporter files from Platform/Platform.Examples folder
- #563: Code transformer that can add or remove MethImpl AgressiveInlining attribute to all methods in C# code
- #553: Update Coding conventions
- #549: Strings storage service based on Sequences
- #510: Compare Doublets with Redis
- #497: Fix bug when scrolling WebTerminal
- #496: Fix bugs with Server and Terminal
- #217: Update WebTerminal to support .NET Core RTM
- #198: Try to build DocFX website for Links Platform
- #195: (ORM) EntityFramework Database Provider for Links
- #187: Flexibility (functionality) and maximum performance at the same time for ILinks
- #158: Implement parser/generator for LiNo (Links Notation) (like JSON for Links)
- #155: Package C/C++ libraries with one of the package managers
- #65: Telegram Bot interface
- #57: Traveler/Walker/Visiter/Crawler/Browser

### Idea

- #641: Recursive path can be written as recursive links network structure
- #620: Stored queries
- #588: Multiple types with doublets at a fraction of the cost of triplet
- #221: Object extensions and context system
- #187: Flexibility (functionality) and maximum performance at the same time for ILinks
- #169: SensorSync: use multiple sensors inputs to make single one synchronized stream
- #151: Try port Platform.Tests to NUnit
- #139: Альтернативные варианты последовательностей
- #132: Multiple threads on event loop (one resource - one thread guarantee - no waste on wait)
- #122: Constant search concept or goal completion assist
- #104: Requirement of Desicion/More Input/Algorithm Change instead of Error/Exception

### Internationalization

- #157: Internationalization for error messages
- #83: C# XML documentation multilingual support (aka internationalization)

### Java

- #125: Triple Links example (using Java)
- #88: Java integration using jni4net

### Possible Bug

- #103: Check saved transaction log timestamps are all in the past
- #101: Undo/Revert mechanism for transactions

### Python

- #120: Triple Links example (using Python)

### Question

- #223: The emptiness/nothing. The one, or infinitelly many parts of same emptiness? Is a part of emptiness is the same as whole emptiness?
- #139: Альтернативные варианты последовательностей
- #113: Rename project to Linkform?
- #111: Object or pattern?
- #42: Should 0-link (null-link) be allowed for reference?
- #26: Decide what means 0 (as an amount links in database)
- #13: Do we allow to change cascade behaviour on update and delete, and even create?
- #2: Terminology

### Side Project

- #684: Sequence of events
- #683: Convert HTML to Markdown to convert it to openapi.yaml
- #682: JSON samples to JSON schema conversion
- #681: Issue description to related code
- #680: Better Changes | Code evolution
- #679: Error formatting tool
- #678: Words | WordLinks
- #677: Self play VR
- #674: Use GPT-4 to summarize key agreement conditions
- #673: Program error search
- #672: TTD driven version of stack-overflow
- #671: wiki-assistant / wiki-qa
- #670: Bug reason binary search
- #669: Better than git: source control that is not based on files but classes/functions.
- #668: Easy breaking changes
- #667: Dependency Unhell
- #666: C++ package manager
- #665: Better Search Engine (beyond Google)
- #642: Server side text activator
- #624: StackOverflow but not only for humans, also for bots and automated systems and non-profit
- #623: Thingrary (предметотека) a library, but for things (non-profit)
- #622: Non-profit bank
- #619: Remote procedure or class methods calls in any programming languages synchronized via Doublets storage
- #618: Artificial neural network inside viewer
- #616: Code completion based on the code under free software license
- #614: Algorithm generation as searching of path though type transitions
- #609: A better translator architecture prototype
- #608: Translate/Sync all open-source projects/packages/libraries to all programming languages.
- #607: Sync everything
- #606: Translate/Sync all Wikipedia articles to all natural languages and join content of articles with the same meaning
- #604: Open-data movement (Collection of Public Domain Data)
- #602: Collection and Aggregation of Comments and Feedback from all public and private platforms in the internet
- #597: Direct search of needs and offers
- #596: Search engine with public list of all search queries
- #595: Neural Network patterns extraction
- #594: Use StorJ as distributed transaction log storage
- #579: GitHub bot that ensures all text files have line break at the end of the file
- #574: Decentralized Public Database
- #573: Decentralized Web Archive
- #569: Schema inference
- #568: Data translation rules inference by example
- #565: GitHub bot that ensures all text files use the same style of line breaks
- #564: Libraries friend projects finder
- #563: Code transformer that can add or remove MethImpl AgressiveInlining attribute to all methods in C# code
- #562: "Anonymous" editable search results (Combination of models of Google and Wikipedia/StackOverflow)
- #561: Store internationalization data inside links file (one file for all languages)
- #560: Bugwiki (Encyclopedia of bugs and software errors, fails and so on)
- #559: Bug fixing by analogy
- #558: Grammar inference from examples
- #557: Integration with Hasura (as an replacement for PostgreSQL)
- #556: Compare to neo4j
- #555: Software logs storage
- #554: Create an examples of Links storage solution as a replacement for other database engines in real world open-source application
- #551: Lifetime companion agent
- #550: Carbon footprint calculator
- #549: Strings storage service based on Sequences
- #546: Use the phone as a microphone or speaker for the computer
- #544: Compare to MySQL
- #543: Compare to PostgreSQL
- #542: Recreate Wikipedia database in different DBMS to compare with Links Platforms' implementaion
- #541: Convert everything
- #540: Code storage
- #539: Text storage
- #533: Extract changes patterns from Git history
- #527: Map price history with events
- #525: Сollective decision making, collective Q&A system
- #524: Model with infinite number of special dimensions
- #523: Automatic interface implementation using code generation
- #519: WakaTime local proxy server
- #518: Graph package manager
- #517: Code snippets encyclopedia/knowledgebase
- #516: "What is X?" chat bot
- #515: Interrepository merging or merging by content
- #514: Universal search engine for complex object with adjustable search form
- #512: Fact-checking bot
- #511: Ways in which any project can be done better or indicators that is worth doing
- #510: Compare Doublets with Redis
- #509: Automatic bug fixing via interfered code detection
- #508: Non-profit service for recurrent donations
- #507: Service for finding best immigration option
- #506: Automatic driver code generation
- #505: Unfake or real users database or identification service
- #504: Requests and responses open knowledge base
- #503: Facts open database
- #498: Crowdfunding platform with ability to donate resources, such as work time or physical devices
- #491: Automatic tests generation to maximize test coverage
- #487: A tool to create automatic breaking changes update in all user code of any updated library
- #484: Better recovery system for operating systems
- #483: Black box for operation systems
- #482: Discover patterns related to the same description of changes on GitHub
- #481: Trade solutions on the problem descriptions
- #480: Vulnerability scanner as a GitHub Bot
- #479: GitHub bot that ensures all text files are encoded in UTF-8
- #478: Synchronous debug of two functions with the same logic but in different languages
- #477: Code translator bot
- #476: Discover substitution patterns as you code, and repeat if pattern is well formed
- #473: Better class name service
- #471: Fix the typo everywhere (TypoBot)
- #462: Data transformation code snippets database
- #454: Compare with RavenDB
- #453: Automatic video recording editing (cut out frames with no action)
- #340: GitHub Similar organizations/repositories/issues/file search
- #335: The ultimate source of information finding software
- #334: Markdown text to wiki-annotated markdown text
- #333: Is where a Wikipedia/Wiktionary page for word or phrase?
- #306: Media event comments grouping by sense
- #304: An even better fight with dublication on IPFS, BitTorrent and so on. "The fragment Id service".
- #303: Global data dependency graph or global data version control
- #229: Global Research Database (storage) and Dynamic Research System (continuous computation)
- #228: Make every internet service open-source, free (but with an ability to be public or private)
- #227: Any/All Account/Profile Sync/Backup as way to own your data
- #226: Global Genealogical Tree (OpenSource, Free, Public)
- #220: Continuous search refinement | Непрерывное уточнение поиска
- #216: ﻿Links File System
- #215: Links Operation System
- #214: Links to MongoDB synchronization
- #212: The most precise world's model (physical + knowledge)
- #211: Decentralized computational network (grid) with code+knowledge encyclopedia included that anyone can edit
- #206: Matrix text (symbols) or links editor
- #205: Binary mapper
- #204: Is this a word?
- #203: Make an algorithm to find all basic worlds of any explanatory dictionary of any language
- #202: Sorted sequence as a unique set of values
- #195: (ORM) EntityFramework Database Provider for Links
- #194: Universal Links String file format
- #177: Subject, predicate (verb + object) detector (Sequences -> Sentences)
- #176: Tag file system
- #175: Test (example) data collection resource
- #174: Question to Query translator (question solver)
- #173: Global unified database
- #169: SensorSync: use multiple sensors inputs to make single one synchronized stream
- #165: Linq to Links
- #164: Vectorization
- #162: P2P + Blockchain + Links as alternative to centralized Wikipedia for global knowledge base
- #161: ChatCrawler
- #158: Implement parser/generator for LiNo (Links Notation) (like JSON for Links)
- #154: Find the better way to run assembly tests (xunit)
- #150: Compare Links Store to Microsoft SQL Server Compact and SQLite
- #146: Convertion unstructured text (human readable) instructions to scripts (code)
- #135: Inter/Intermediate/Abstract Semantic Language or Linguistics API | Универсальный Язык Смысла
- #134: Extract personal code preferencies from published code by the developer
- #131: Download oldest/dead pages from the Internet Archive
- #130: Caching WebProxy for personal use
- #127: Integrate with everything
- #122: Constant search concept or goal completion assist
- #119: Translate Everything
- #112: Comment Everything
- #105: Virtual Programmer GitHub Bot (repository as intermediate solution, issues as user requests/requirements description or decision making)
- #104: Requirement of Desicion/More Input/Algorithm Change instead of Error/Exception
- #102: Test Everything
- #97: Global Collection of Objects Structure, Behaviour, Standards, APIs
- #94: API (Edge/Bridge) to Everything (or any API)
- #91: WebCrawler
- #90: Test/Execute Everything (any example, instruction, recipe and so one)
- #89: Compare Everything
- #79: Compare performance with Google's LevelDB
- #73: Swagger API Implementation Generator
- #65: Telegram Bot interface
- #35: All World Databases Comparison
- #33: Neural Network example
- #16: Build an real life application example

### Visualization

- #538: Read and write UI modes principle
- #534: Links visualization with write access
- #206: Matrix text (symbols) or links editor
- #159: CSV exporter
- #45: A tool or function for converting Transaction Log file to Gexf Dynamic Graph
- #31: Synchronize features of Terminal and WebTerminal
- #23: Alternative visualization for web terminal

## Dependency Graph

```mermaid
graph TD
    I665[#665: Better Search Engine (beyond Google)]
    I514[#514: Universal search engine for complex obje]
    I665 --> I514
    I340[#340: GitHub Similar organizations/repositorie]
    I665 --> I340
    I661[#661: Market with infinitely precise search]
    I665 --> I661
    I662[#662: Infinitely precise universal search engi]
    I665 --> I662
    I596[#596: Search engine with public list of all se]
    I665 --> I596
    I121[#121: Sequences search using autocomplete]
    I665 --> I121
    I122[#122: Constant search concept or goal completi]
    I665 --> I122
    I539[#539: Text storage]
    I665 --> I539
    I220[#220: Continuous search refinement | Непрерывн]
    I665 --> I220
    I661 --> I662
    I608[#608: Translate/Sync all open-source projects/]
    I607[#607: Sync everything]
    I608 --> I607
    I119[#119: Translate Everything]
    I608 --> I119
    I541[#541: Convert everything]
    I607 --> I541
    I607 --> I119
    I606[#606: Translate/Sync all Wikipedia articles to]
    I606 --> I607
    I606 --> I119
    I602[#602: Collection and Aggregation of Comments a]
    I306[#306: Media event comments grouping by sense]
    I602 --> I306
    I600[#600: Links and SQL synchronization]
    I599[#599: Each table as a virtual links structure]
    I600 --> I599
    I585[#585: Humanity dictionary]
    I584[#584: International phonetic alphabet translat]
    I585 --> I584
    I555[#555: Software logs storage]
    I483[#483: Black box for operation systems]
    I555 --> I483
    I29[#29: Links data structure as a binary data pr]
    I555 --> I29
    I95[#95: Compression optimization]
    I555 --> I95
    I540[#540: Code storage]
    I540 --> I539
    I216[#216: ﻿Links File System]
    I539 --> I216
    I539 --> I540
    I505[#505: Unfake or real users database or identif]
    I504[#504: Requests and responses open knowledge ba]
    I505 --> I504
    I504 --> I505
    I503[#503: Facts open database]
    I504 --> I503
    I488[#488: Programming languages translation]
    I135[#135: Inter/Intermediate/Abstract Semantic Lan]
    I488 --> I135
    I168[#168: GitHub extension/app for automatic langu]
    I488 --> I168
    I488 --> I119
    I484[#484: Better recovery system for operating sys]
    I484 --> I216
    I164[#164: Vectorization]
    I483 --> I164
    I474[#474: The database is a log, not a snapshot (a]
    I466[#466: "Create-only" or "no deletion" mode]
    I474 --> I466
    I472[#472: From every issue on GitHub it is possibl]
    I471[#471: Fix the typo everywhere (TypoBot)]
    I472 --> I471
    I456[#456: Attempt to implement data layer for CMS ]
    I455[#455: Become a part of .NET Foundation]
    I456 --> I455
    I220 --> I596
    I211[#211: Decentralized computational network (gri]
    I127[#127: Integrate with everything]
    I211 --> I127
    I209[#209: Make Links parts number (2 for pairs, 3 ]
    I62[#62: Reimplement triple (tuple) links impleme]
    I209 --> I62
    I207[#207: Random access links API]
    I208[#208: Manage Links memory using ranges list in]
    I207 --> I208
    I204[#204: Is this a word?]
    I179[#179: Reference collections for each small dat]
    I204 --> I179
    I203[#203: Make an algorithm to find all basic worl]
    I204 --> I203
    I187[#187: Flexibility (functionality) and maximum ]
    I105[#105: Virtual Programmer GitHub Bot (repositor]
    I187 --> I105
    I155[#155: Package C/C++ libraries with one of the ]
    I66[#66: Publish Links Platform Library as a NuGe]
    I155 --> I66
    I138[#138: Data importers]
    I158[#158: Implement parser/generator for LiNo (Lin]
    I138 --> I158
    I126[#126: Complete wiki documentation]
    I198[#198: Try to build DocFX website for Links Pla]
    I126 --> I198
    I110[#110: Patterns]
    I121 --> I110
    I111[#111: Object or pattern?]
    I111 --> I110
    I100[#100: Sets]
    I111 --> I100
    I78[#78: Objects (static type system & dynamic ty]
    I111 --> I78
```

## Suggested Implementation Priority

Based on dependency analysis, these issues should be prioritized:

### High Priority (Foundational Issues)

1. #119: Translate Everything (referenced 4 times)
1. #662: Infinitely precise universal search engine  (referenced 2 times)
1. #596: Search engine with public list of all search queries (referenced 2 times)
1. #539: Text storage (referenced 2 times)
1. #607: Sync everything (referenced 2 times)
1. #216: ﻿Links File System (referenced 2 times)
1. #110: Patterns (referenced 2 times)
1. #10: Triggers (referenced 2 times)
1. #57: Traveler/Walker/Visiter/Crawler/Browser (referenced 2 times)
1. #514: Universal search engine for complex object with adjustable search form (referenced 1 times)

### Medium Priority (Dependent Issues)

- #665: Better Search Engine (beyond Google) (depends on 9 other issues)
- #661: Market with infinitely precise search (depends on 1 other issues)
- #608: Translate/Sync all open-source projects/packages/libraries to all programming languages. (depends on 2 other issues)
- #606: Translate/Sync all Wikipedia articles to all natural languages and join content of articles with the same meaning (depends on 2 other issues)
- #602: Collection and Aggregation of Comments and Feedback from all public and private platforms in the internet (depends on 1 other issues)
- #600: Links and SQL synchronization (depends on 1 other issues)
- #585: Humanity dictionary (depends on 1 other issues)
- #555: Software logs storage (depends on 3 other issues)
- #540: Code storage (depends on 1 other issues)
