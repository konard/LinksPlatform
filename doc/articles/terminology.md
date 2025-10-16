# Links Platform Terminology Dictionary

This document provides definitions for specific terms used throughout the Links Platform project.

## Core Concepts

### Link (Связь)
A unified data type that combines Element and Link from the original associative model. A link is the fundamental unit of data in Links Platform that can reference other links. A link that references itself is called a point or element.

**Synonyms**: Connection, Relation, Association

### Address (Адрес)
A unique identifier or location reference for a link in the associative memory storage. The address allows direct access to a specific link.

**Synonyms**: Identifier, Location

### Index (Индекс)
A numeric position or reference used to access links. In the context of Links Platform, index typically refers to the unique address/identifier of a link in storage.

**Synonyms**: Position, Reference number

### ID
See Address. A unique identifier for a link.

**Synonyms**: Address, Identifier

### Reference (Ссылка)
A pointer or connection from one link to another link. Each link contains one or more references to other links (or itself).

**Synonyms**: Pointer, Connection

## Link Structure

### Source (Начало, Источник, Подлежащее)
The first reference in a link structure. In a doublet (two-part link), this is the beginning or subject of the relationship. In directed links, this is where the connection originates.

**Synonyms**: Beginning, Subject, Origin, From

### Target (Конец, Цель, Сказуемое, Дополнение)
The second reference in a link structure. In a doublet (two-part link), this is the end, predicate, or object of the relationship. In directed links, this is where the connection points to.

**Synonyms**: End, Predicate, Object, Destination, To

### Linker (Связка, Глагол, Тип, Предикат)
In triplet (three-part) links, the middle component that describes the type or nature of the relationship between source and target. This provides semantic meaning to the connection.

**Synonyms**: Type, Predicate, Verb, Relationship Type

## Data Structures

### Doublet (Дуплет)
A link structure consisting of exactly two references: Source and Target. This is the primary implementation in Links Platform, providing an untyped connection between two links.

**Related**: Pair, Binary Link

### Triplet (Триплет)
A link structure consisting of three references: Source, Linker, and Target. This provides a typed connection where the Linker specifies the relationship type.

**Related**: Triple, Typed Link, RDF-like structure

### Point (Точка)
A special case of a link where all references point to itself. A point represents an atomic element or self-referential entity.

**Related**: Element, Self-reference, Node

### Sequence (Последовательность)
An ordered collection of links built from doublets, structured as a binary tree where the left subtree represents the beginning and the right subtree represents the end of the sequence.

**Related**: List, Ordered collection

## Memory and Storage

### Associative Memory (Ассоциативная память)
A data storage model where information is organized as links/associations between entities rather than in tables or hierarchies. Links Platform implements this model.

**Related**: Associative model of data, Network storage

### Any (Любой)
A constant representing any link address or no constraint on a link address. Used in queries to indicate that a particular field can match any value.

**Synonyms**: Wildcard, No constraint

### Constants (Константы)
Predefined link addresses or values used throughout the system, such as `Any` (no constraint) and `Continue` (continue iteration).

## Operations

### Create (Создать)
Operation to create a new link in the storage.

### Update (Обновить)
Operation to modify the references (source/target) of an existing link.

### Delete (Удалить)
Operation to remove a link from storage.

### Each (Для каждого)
Operation to iterate through links matching specified criteria, executing a handler function for each match.

**Synonyms**: Iterate, ForEach, Query

### Query (Запрос)
A set of constraints used to search for links. Arguments are interpreted as restrictions on link properties (index, source, target).

## Related Concepts

### Format
A method to convert a link or sequence of links into a human-readable string representation.

### Handler (Обработчик)
A function or callback executed for each link during iteration operations.

### Trigger (Триггер)
An event-driven mechanism that executes transformations automatically when certain conditions are met in the associative memory.

### Compression Degree (Степень сжатия)
A measure of how efficiently sequences are stored by reusing existing links/pairs. Higher compression means more efficient storage through link reuse.

### External Reference (Внешняя ссылка)
A reference to data or entities outside the primary associative memory storage system.

## Implementation Terms

### UnitedMemoryLinks
A unified memory links implementation that combines different access patterns into a single storage type.

### File-mapped
Storage backed by a file on disk, allowing persistence of link data between program executions.

### In-memory
Storage kept entirely in RAM for faster access, but lost when the program terminates unless saved.

---

*This terminology dictionary is maintained to ensure consistent understanding and communication across the Links Platform project.*
