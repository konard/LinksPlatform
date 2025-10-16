# Design Decision: 0-Link (Null-Link) Reference Policy

## Issue Reference
[Issue #42: Should 0-link (null-link) be allowed for reference?](https://github.com/konard/LinksPlatform/issues/42)

## Problem Statement

Should the Links platform allow references to the 0-link (null-link), which represents emptiness or the absence of a link? This is a fundamental conceptual question about whether it's possible to "touch emptiness" or "make something with nothing."

## Historical Context

In the original implementation (circa 2010), the `Link.Create()` method explicitly prohibited null references:

```csharp
static public Link Create(Link source, Link linker, Link target)
{
    if (source == null)
    {
        throw new ArgumentNullException("Источник связи (начало) должен быть указан.");
    }
    if (linker == null)
    {
        throw new ArgumentNullException("Связчик должен быть указан.");
    }
    if (target == null)
    {
        throw new ArgumentNullException("Цель связи (конец) должна быть указана.");
    }
    // ...
}
```

This strict validation made null references impossible at the system level.

## Discussion Analysis

Through extensive discussion (2015), several perspectives emerged:

### Arguments for Prohibiting 0-Link References

1. **Simplicity**: Disallowing 0-links avoids conceptual complexity
2. **Workaround Available**: Any other link can be assigned with "0-empty" meaning
3. **Duplication Concerns**: Multiple references to null could introduce inconsistencies

### Arguments for Allowing 0-Link References

1. **Self-Reference Option**: 0 could reference itself in a packed/compacted database
2. **Solving Contradictions**: Having one "real" reference to an existing link and one reference to null could help resolve the "point-pair contradiction"
3. **Domain-Specific Needs**: Different problem domains may have different requirements
4. **Logical Representation**: In some domains, 0 could represent FALSE in boolean logic (with negation of 0 being 1/TRUE)

## Design Decision

**The 0-link reference policy should be configurable and domain-dependent.**

### Rationale

1. **Domain Specificity**: Each problem domain may have its own interpretation of emptiness and null
2. **Flexibility**: A configurable approach allows the system to adapt to different use cases
3. **User Control**: Implementation decisions should be left to users who understand their specific domain
4. **No Universal Answer**: There is no single "correct" answer that applies to all scenarios

### Implementation Recommendations

The Links platform should provide:

1. **Configuration Interface**: A way for users to specify whether 0-link references are allowed in their specific instance
2. **Default Behavior**: A sensible default (recommended: disallow 0-links for safety)
3. **Documentation**: Clear explanation of the implications of each choice
4. **Validation Layer**: Enforce the configured policy at link creation time

### Example Use Cases

#### Domain 1: Strict Graph Theory
- **Policy**: Prohibit 0-links
- **Reason**: Every edge must connect actual vertices

#### Domain 2: Philosophical or Conceptual Modeling
- **Policy**: Allow 0-links
- **Reason**: Need to represent "nothing," "absence," or "void" as a concept

#### Domain 3: Boolean Logic System
- **Policy**: Allow 0-links
- **Reason**: 0 represents FALSE; may need to reference it explicitly

## Theoretical Considerations

From the links theory document (links-theory.md), empty links (пустые связи) are defined as:

> "Пустые связи или связи состоящие из нуля ссылок на связи, это связи представляющие собой ничто, или отсутствие этих связей."
>
> (Empty links or links consisting of zero references to links are links representing nothing, or the absence of these links.)

The document acknowledges that:
- Empty parts can represent zero, one, or infinite empty links
- Empty links, if they exist, can be placed in any volume of space, even infinitesimally small
- The absence of links or one link is semantically equivalent

This philosophical foundation supports the configurability approach: the meaning of "emptiness" depends on context.

## Conclusion

Issue #42 represents a fundamental design question without a universal answer. The recommended solution is to make the 0-link reference policy **configurable** rather than enforced, allowing each implementation to choose the approach that best fits its domain-specific requirements.

This decision should be documented in the platform's configuration interface, with clear explanations of the implications of each choice for users implementing Links-based systems.

---

**Status**: Resolved through configurability approach
**Date**: 2025-10-16
**Discussion Period**: 2015-11-05 to 2015-12-11
