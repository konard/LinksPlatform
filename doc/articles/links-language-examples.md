# Links Language: Visual Examples

This document provides visual examples of Links Language structures, illustrating how questions and interrogative relationships are represented in the Links Platform.

## Basic Question Patterns

### Example 1: Simple "What?" Question

```
   (Entity)  ------------>  ( ? )
              зачем?
              куда?
              с какай целью?
```

This represents a question asking "For what purpose?", "Where to?", or "With what goal?" - the fundamental interrogative relationship between an entity and an unknown target.

In English:
```
   (Entity)  ------------>  ( ? )
              for what?
              why?
              to what?
              to where?
              to who?
```

This pattern is the foundation of Links Language, showing how any entity can be connected to an unknown through an interrogative link.

### Example 2: Reverse Question - "From What?"

```
    ( ! )   ------------>  (Entity)
            на каком основании?
            почему?
            откуда?
```

This represents questions about origin, reason, or basis: "On what basis?", "Why?", "From where?"

In English:
```
    ( ! )   ------------>  (Entity)
            from what?
            from where?
            from who?
            from which?
```

The exclamation mark represents the unknown source or origin being questioned.

### Example 3: Three-Way Relationship Question

```
       (Entity1)
            |
            |
            v
       ( ? )  ------------>  (Entity2)
            как?
            каким образом?
            чем?
```

This structure represents questions about the relationship or method connecting two entities: "How?", "In what manner?", "By what means?"

In English:
```
       (Entity1)
            |
            |
            v
       ( ? )  ------------>  (Entity2)
                        how?
```

The unknown in the middle represents the method, manner, or relationship that needs to be determined.

## Natural Language Question Representations

### Question Words in Russian

The Links Language naturally accommodates Russian interrogative structures:

| Russian | Transliteration | English | Link Pattern |
|---------|----------------|---------|--------------|
| Что? | Chto? | What? | `[X] ---[Что?]---> [?]` |
| Почему? | Pochemu? | Why? | `[X] ---[Почему?]---> [?]` |
| Зачем? | Zachem? | For what purpose? | `[X] ---[Зачем?]---> [?]` |
| Как? | Kak? | How? | `[X] ---[Как?]---> [?]` |
| Где? | Gde? | Where? | `[X] ---[Где?]---> [?]` |
| Куда? | Kuda? | To where? | `[X] ---[Куда?]---> [?]` |
| Откуда? | Otkuda? | From where? | `[?] ---[Откуда?]---> [X]` |
| Когда? | Kogda? | When? | `[X] ---[Когда?]---> [?]` |
| Кто? | Kto? | Who? | `[X] ---[Кто?]---> [?]` |
| Какой? | Kakoy? | Which/What kind? | `[X] ---[Какой?]---> [?]` |

### Question Words in English

Corresponding English interrogative structures:

| English | Link Pattern | Description |
|---------|-------------|-------------|
| What? | `[X] ---[What?]---> [?]` | Identity/Nature |
| Why? | `[X] ---[Why?]---> [?]` | Reason/Cause |
| How? | `[X] ---[How?]---> [?]` | Method/Manner |
| Where? | `[X] ---[Where?]---> [?]` | Location |
| When? | `[X] ---[When?]---> [?]` | Time |
| Who? | `[X] ---[Who?]---> [?]` | Person/Agent |
| Which? | `[X] ---[Which?]---> [?]` | Selection |

## Complex Examples

### Example 4: Question Chain

```
[Entity] ---[What?]---> [?1] ---[Why?]---> [?2]
```

This represents a chain of questions: "What is it, and why is it that way?"

### Example 5: Contextual Question

```
    [Context1]      [Context2]
         |              |
         v              v
    [Entity] ---[How related?]---> [?]
```

This shows a question about how an entity relates to multiple contexts.

### Example 6: Conditional Question

```
[Condition] ---[If]---> [Entity] ---[Then what?]---> [?]
```

This represents a conditional question: "If condition applies to entity, then what?"

## Practical Scenarios

### Scenario 1: Database Query

Question: "What data is stored in this database?"

```
[This Database] ---[What?]---> [?]
                    |
                    v
               [Contains?]
```

### Scenario 2: Process Understanding

Question: "How does this process work?"

```
[This Process] ---[How?]---> [?]
                   |
                   v
              [Functions?]
```

### Scenario 3: Causal Investigation

Question: "Why did this error occur?"

```
[This Error] ---[Why?]---> [?]
                 |
                 v
            [Root Cause?]
```

### Scenario 4: Origin Tracing

Question: "Where does this data come from?"

```
[?] ---[From where?]---> [This Data]
```

## Diagram Representations

### Circular Node Notation

In Links Language diagrams, circular nodes represent entities:

```
   ○ = Known Entity
  (?) = Unknown/Query Target
  (!) = Unknown Source/Origin
```

### Arrow Notation

Arrows represent directed links with interrogative labels:

```
  ○ ----[Question]----> (?)

  Examples:
  ○ ----[What?]-------> (?)
  ○ ----[Why?]--------> (?)
  ○ ----[How?]--------> (?)
```

### Multi-Node Structures

Complex relationships use multiple nodes and arrows:

```
     ○
     |
     | [Through?]
     v
    (?)  ----[To?]----> ○
```

## Integration Examples

### Example 7: Natural Language to Links Language

Original question: "С какой целью руга?"
(Translation: "For what purpose/goal?")

Links representation:
```
[рука/hand] ---[зачем?/for what?]---> [?]
[рука/hand] ---[куда?/to where?]----> [?]
[рука/hand] ---[с какой целью?/with what goal?]---> [?]
```

### Example 8: Multiple Perspectives

Question from different angles:

Russian perspective:
```
[Объект] ---[На каком основании?]---> [?]
          (On what basis?)
```

English perspective:
```
[Object] ---[On what foundation?]---> [?]
```

Both represent the same interrogative relationship structure.

## Answer Representation

When a question is answered, the unknown is replaced:

Before (Question):
```
[Entity] ---[What?]---> [?]
```

After (Answer):
```
[Entity] ---[Is]---> [Answer]
```

The question link type changes from interrogative to declarative.

## Multilingual Support

Links Language naturally supports multiple languages by treating interrogative words as link type identifiers:

```
[X] ---[What?/Что?/Quoi?/Was?]---> [?]
```

All variations map to the same underlying interrogative link type in the system.

## Implementation Notes

### Creating Question Links

In pseudocode:
```
function CreateQuestion(entity, questionType):
    unknown = CreateUnknownMarker()
    link = CreateLink(entity, questionType, unknown)
    return link
```

### Resolving Questions

In pseudocode:
```
function AnswerQuestion(questionLink, answer):
    sourceEntity = questionLink.source
    questionType = questionLink.type
    answerLink = CreateLink(sourceEntity, ConvertToDeclarative(questionType), answer)
    return answerLink
```

## Visual Summary

The six diagrams from Issue #34 demonstrate:

1. **Basic forward question**: Entity asking about unknown target
2. **Basic reverse question**: Unknown source asking about entity
3. **Three-way relationship**: Question about connection between entities
4. **Multiple question types**: Various ways to ask about the same relationship
5. **Origin questions**: Asking about source/origin
6. **Method questions**: Asking about process/manner

All these patterns form the core vocabulary of Links Language for representing interrogative relationships in the Links Platform.

## Conclusion

These visual examples illustrate the flexibility and power of Links Language for representing questions across languages and contexts. The system's graph-based nature naturally accommodates the relational structure of interrogative statements, making it suitable for knowledge representation, semantic search, and automated reasoning applications.
