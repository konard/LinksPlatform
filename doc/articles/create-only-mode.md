# Create-Only Mode (No Deletion Mode)

## Введение / Introduction

**Русский**: Режим "только создание" (или "без удаления") является подходом к хранению данных, предложенным в Ассоциативной Модели Данных (AMD) Саймона Вильямса. Этот подход упрощает систему хранения и позволяет сохранять полную историю всех изменений в состоянии связей.

**English**: Create-only mode (or no deletion mode) is a data storage approach suggested in the Associative Model of Data (AMD) by Simon Williams. This approach simplifies the storage system and allows storing the complete history of all changes in the links state.

## Основная идея / Core Concept

### Русский

В традиционных системах управления данными существуют три основные операции:
- **CREATE** (Создание) - добавление новых данных
- **UPDATE** (Обновление) - изменение существующих данных
- **DELETE** (Удаление) - удаление данных

В режиме "только создание" все изменения состояния записываются используя только операцию CREATE. Это достигается следующим образом:

- **Создание**: Остаётся без изменений - создаётся новая связь
- **Обновление**: Вместо изменения существующей связи создаётся новая связь с новым содержимым, а старая связь помечается как устаревшая/замещённая
- **Удаление**: Вместо физического удаления связи создаётся маркер, указывающий что связь удалена (мягкое удаление)

### English

In traditional data management systems, there are three main operations:
- **CREATE** - adding new data
- **UPDATE** - modifying existing data
- **DELETE** - removing data

In create-only mode, all state changes are recorded using only the CREATE operation. This is achieved as follows:

- **Create**: Remains unchanged - a new link is created
- **Update**: Instead of modifying an existing link, a new link with new content is created, and the old link is marked as superseded/outdated
- **Delete**: Instead of physically deleting a link, a marker is created indicating the link is deleted (soft delete)

## Преимущества / Benefits

### Русский

1. **Упрощение системы хранения**: Отсутствие необходимости управлять физическим удалением и перезаписью упрощает архитектуру хранилища
2. **Сохранение истории**: Автоматическое сохранение полной истории всех изменений
3. **Темпоральные запросы**: Возможность запрашивать состояние данных на любой момент времени
4. **Неизменяемость данных**: Отсутствие деструктивных операций повышает целостность данных
5. **Аудит и отладка**: Полная прослеживаемость всех изменений
6. **Параллелизм**: Упрощение работы с конкурентным доступом (меньше конфликтов блокировок)

### English

1. **Storage System Simplification**: No need to manage physical deletion and overwriting simplifies storage architecture
2. **History Preservation**: Automatic preservation of complete change history
3. **Temporal Queries**: Ability to query data state at any point in time
4. **Data Immutability**: Absence of destructive operations improves data integrity
5. **Audit and Debugging**: Complete traceability of all changes
6. **Concurrency**: Simplified concurrent access handling (fewer lock conflicts)

## Реализация / Implementation

### Концептуальная модель / Conceptual Model

#### Русский

Для реализации режима "только создание" необходимы следующие компоненты:

1. **Маркеры состояния**:
   - `SupersededMarker` - маркер для обозначения замещённых связей
   - `DeletedMarker` - маркер для обозначения удалённых связей

2. **Временные метки**: Порядковые номера или временные метки для отслеживания версий

3. **Цепочки версий**: Связи между старыми и новыми версиями данных

#### English

To implement create-only mode, the following components are needed:

1. **State Markers**:
   - `SupersededMarker` - marker for superseded links
   - `DeletedMarker` - marker for deleted links

2. **Timestamps**: Sequence numbers or timestamps for version tracking

3. **Version Chains**: Links between old and new versions of data

### Пример структуры / Structure Example

```
Создание связи / Creating a link:
Link1: (Source: A, Target: B)

Обновление связи / Updating a link:
Link1: (Source: A, Target: B)  <- помечается как superseded / marked as superseded
Link2: (Source: A, Target: C)  <- новая версия / new version
Link3: (Source: SupersededMarker, Target: Link1)  <- маркер / marker
Link4: (Source: Link1, Target: Link2)  <- цепочка версий / version chain

Удаление связи / Deleting a link:
Link2: (Source: A, Target: C)  <- всё ещё существует / still exists
Link5: (Source: DeletedMarker, Target: Link2)  <- маркер удаления / deletion marker
```

### Паттерн декоратора / Decorator Pattern

#### Русский

Рекомендуемый способ реализации - использование паттерна декоратора, который оборачивает существующую реализацию `ILinks<TLinkAddress>`:

```csharp
public class CreateOnlyLinksDecorator<TLinkAddress> : LinksDecoratorBase<TLinkAddress>
{
    private readonly TLinkAddress _supersededMarker;
    private readonly TLinkAddress _deletedMarker;

    // Конструктор инициализирует маркеры
    public CreateOnlyLinksDecorator(ILinks<TLinkAddress> links) : base(links)
    {
        // Инициализация маркеров
    }

    // Create остаётся без изменений
    public override TLinkAddress Create(IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
    {
        return base.Create(substitution, handler);
    }

    // Update превращается в Create + пометку старой связи
    public override TLinkAddress Update(IList<TLinkAddress>? restriction,
                                       IList<TLinkAddress>? substitution,
                                       WriteHandler<TLinkAddress>? handler)
    {
        // 1. Создать новую связь с новым содержимым
        var newLink = base.Create(substitution, null);

        // 2. Создать маркер замещения для старой связи
        var oldLink = restriction[Constants.IndexPart];
        base.Create(new[] { _supersededMarker, oldLink }, null);

        // 3. Создать цепочку версий: старая -> новая
        base.Create(new[] { oldLink, newLink }, null);

        return newLink;
    }

    // Delete превращается в Create маркера удаления
    public override TLinkAddress Delete(IList<TLinkAddress>? restriction,
                                       WriteHandler<TLinkAddress>? handler)
    {
        var linkToDelete = restriction[Constants.IndexPart];

        // Создать маркер удаления
        return base.Create(new[] { _deletedMarker, linkToDelete }, null);
    }

    // Each и Count фильтруют замещённые и удалённые связи
    public override TLinkAddress Each(IList<TLinkAddress>? restriction,
                                     ReadHandler<TLinkAddress>? handler)
    {
        return base.Each(restriction, link =>
        {
            // Пропустить если связь помечена как замещённая или удалённая
            if (IsSuperseded(link) || IsDeleted(link))
                return Constants.Continue;

            return handler(link);
        });
    }
}
```

#### English

The recommended implementation approach is using the decorator pattern that wraps an existing `ILinks<TLinkAddress>` implementation:

```csharp
public class CreateOnlyLinksDecorator<TLinkAddress> : LinksDecoratorBase<TLinkAddress>
{
    private readonly TLinkAddress _supersededMarker;
    private readonly TLinkAddress _deletedMarker;

    // Constructor initializes markers
    public CreateOnlyLinksDecorator(ILinks<TLinkAddress> links) : base(links)
    {
        // Initialize markers
    }

    // Create remains unchanged
    public override TLinkAddress Create(IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
    {
        return base.Create(substitution, handler);
    }

    // Update becomes Create + marking old link
    public override TLinkAddress Update(IList<TLinkAddress>? restriction,
                                       IList<TLinkAddress>? substitution,
                                       WriteHandler<TLinkAddress>? handler)
    {
        // 1. Create new link with new content
        var newLink = base.Create(substitution, null);

        // 2. Create superseded marker for old link
        var oldLink = restriction[Constants.IndexPart];
        base.Create(new[] { _supersededMarker, oldLink }, null);

        // 3. Create version chain: old -> new
        base.Create(new[] { oldLink, newLink }, null);

        return newLink;
    }

    // Delete becomes Create of deletion marker
    public override TLinkAddress Delete(IList<TLinkAddress>? restriction,
                                       WriteHandler<TLinkAddress>? handler)
    {
        var linkToDelete = restriction[Constants.IndexPart];

        // Create deletion marker
        return base.Create(new[] { _deletedMarker, linkToDelete }, null);
    }

    // Each and Count filter out superseded and deleted links
    public override TLinkAddress Each(IList<TLinkAddress>? restriction,
                                     ReadHandler<TLinkAddress>? handler)
    {
        return base.Each(restriction, link =>
        {
            // Skip if link is marked as superseded or deleted
            if (IsSuperseded(link) || IsDeleted(link))
                return Constants.Continue;

            return handler(link);
        });
    }
}
```

## Использование / Usage

### Русский

```csharp
// Создание базового хранилища
var baseLinks = new UnitedMemoryLinks<ulong>();

// Оборачивание в декоратор create-only
var createOnlyLinks = new CreateOnlyLinksDecorator<ulong>(baseLinks);

// Теперь все операции записываются только через CREATE
var link1 = createOnlyLinks.Create(new ulong[] { 1, 2 });

// Update физически не изменит связь, а создаст новую
createOnlyLinks.Update(new ulong[] { link1 }, new ulong[] { 1, 3 });

// Delete физически не удалит связь, а создаст маркер
createOnlyLinks.Delete(new ulong[] { link1 });

// При чтении автоматически фильтруются замещённые и удалённые связи
createOnlyLinks.Each(null, link => {
    // Видны только актуальные связи
    return createOnlyLinks.Constants.Continue;
});
```

### English

```csharp
// Create base storage
var baseLinks = new UnitedMemoryLinks<ulong>();

// Wrap in create-only decorator
var createOnlyLinks = new CreateOnlyLinksDecorator<ulong>(baseLinks);

// Now all operations are recorded only through CREATE
var link1 = createOnlyLinks.Create(new ulong[] { 1, 2 });

// Update won't physically modify the link, but will create a new one
createOnlyLinks.Update(new ulong[] { link1 }, new ulong[] { 1, 3 });

// Delete won't physically remove the link, but will create a marker
createOnlyLinks.Delete(new ulong[] { link1 });

// When reading, superseded and deleted links are automatically filtered
createOnlyLinks.Each(null, link => {
    // Only current links are visible
    return createOnlyLinks.Constants.Continue;
});
```

## Темпоральные запросы / Temporal Queries

### Русский

Режим "только создание" естественным образом поддерживает запросы к историческому состоянию данных:

```csharp
// Расширенный декоратор с временными метками
public class TemporalCreateOnlyLinksDecorator<TLinkAddress> : CreateOnlyLinksDecorator<TLinkAddress>
{
    // Получить состояние на определённый момент времени
    public IEnumerable<TLinkAddress> GetLinksAtTime(DateTime timestamp)
    {
        // Вернуть только связи, которые:
        // 1. Были созданы до timestamp
        // 2. Не были помечены как замещённые до timestamp
        // 3. Не были помечены как удалённые до timestamp
    }

    // Получить историю изменений связи
    public IEnumerable<TLinkAddress> GetLinkHistory(TLinkAddress link)
    {
        // Проследить цепочку версий
    }
}
```

### English

Create-only mode naturally supports querying historical data state:

```csharp
// Extended decorator with timestamps
public class TemporalCreateOnlyLinksDecorator<TLinkAddress> : CreateOnlyLinksDecorator<TLinkAddress>
{
    // Get state at a specific point in time
    public IEnumerable<TLinkAddress> GetLinksAtTime(DateTime timestamp)
    {
        // Return only links that:
        // 1. Were created before timestamp
        // 2. Were not marked as superseded before timestamp
        // 3. Were not marked as deleted before timestamp
    }

    // Get link change history
    public IEnumerable<TLinkAddress> GetLinkHistory(TLinkAddress link)
    {
        // Follow version chain
    }
}
```

## Производительность / Performance

### Русский

**Компромиссы**:
- ✅ **Быстрая запись**: Только операции добавления, без блокировок на удаление
- ✅ **Простая репликация**: Все изменения - только добавления
- ⚠️ **Использование памяти**: Увеличивается со временем (все версии хранятся)
- ⚠️ **Скорость чтения**: Требуется фильтрация замещённых/удалённых связей

**Оптимизации**:
1. Индекс активных связей для быстрого поиска
2. Периодическая компактификация (опционально)
3. Битовая карта для пометки замещённых/удалённых связей

### English

**Trade-offs**:
- ✅ **Fast writes**: Only append operations, no delete locks
- ✅ **Simple replication**: All changes are appends only
- ⚠️ **Memory usage**: Increases over time (all versions stored)
- ⚠️ **Read speed**: Requires filtering of superseded/deleted links

**Optimizations**:
1. Index of active links for fast lookups
2. Periodic compaction (optional)
3. Bitmap for marking superseded/deleted links

## Ссылки / References

1. Simon Williams - "The Associative Model of Data" (http://www.sentences.com/docs/amd.pdf)
2. Event Sourcing pattern
3. Append-only data structures
4. Temporal databases

## См. также / See Also

- [Links Theory](links-theory.md) - Теория связей / Links theory
- Platform.Data.Doublets library - Реализация дуплетов / Doublets implementation
- Decorator pattern in Links Platform
