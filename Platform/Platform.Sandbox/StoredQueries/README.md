# Stored Queries (Хранимые Запросы)

## Overview / Обзор

Stored Queries is a feature that allows queries to be stored directly in the database, providing several advantages over traditional query execution:

Хранимые запросы — это функция, которая позволяет сохранять запросы непосредственно в базе данных, предоставляя несколько преимуществ по сравнению с традиционным выполнением запросов:

- **Repeated Execution** / **Многократное выполнение**: Queries can be triggered and executed multiple times without recompilation.
- **Better Optimization** / **Лучшая оптимизация**: More time can be spent optimizing queries since they are pre-compiled.
- **Execution Statistics** / **Статистика выполнения**: Detailed statistics are collected for each query execution.
- **Read Triggers** / **Триггеры чтения**: Queries can have triggers that execute before, after, or on error.
- **Language Flexibility** / **Гибкость языка**: Queries can be replaced with function execution in any programming language.

## Architecture / Архитектура

The Stored Queries feature consists of several key components:

Функция хранимых запросов состоит из нескольких ключевых компонентов:

### 1. StoredQuery<TLinkAddress, TResult>

The main class representing a stored query. It contains:

Основной класс, представляющий хранимый запрос. Он содержит:

- Query expression (LINQ expression)
- Compiled query (for fast execution)
- Execution statistics
- List of triggers
- Metadata (name, ID, creation date, etc.)

### 2. StoredQueriesManager<TLinkAddress>

Manager class responsible for:

Класс менеджера, отвечающий за:

- Creating and storing queries
- Executing queries by name or ID
- Managing query lifecycle (enable/disable/delete)
- Maintaining query registry

### 3. QueryExecutionStatistics

Tracks detailed statistics for query execution:

Отслеживает подробную статистику выполнения запроса:

- Total executions count
- Success/failure counts
- Average, min, max execution times
- Average result count
- Recent execution records

### 4. IQueryTrigger<TLinkAddress, TResult>

Interface for query triggers that can execute:

Интерфейс для триггеров запросов, которые могут выполняться:

- **Before** query execution
- **After** successful execution
- **OnError** when execution fails

### 5. Built-in Triggers

Two example trigger implementations:

Две примерные реализации триггеров:

- **LoggingTrigger**: Logs query execution events
- **CachingTrigger**: Caches query results with expiration

## Usage Examples / Примеры использования

### Creating a Stored Query / Создание хранимого запроса

```csharp
var manager = new StoredQueriesManager<ulong>(links);

var query = manager.CreateQuery<ILink<ulong>>(
    "FindLinksBySource",
    linksDb => linksDb.All().Where(link => link.Source == someSource)
);
```

### Adding Triggers / Добавление триггеров

```csharp
// Add logging
var loggingTrigger = new LoggingTrigger<ulong, ILink<ulong>>(
    "QueryLogger",
    TriggerType.Before
);
query.AddTrigger(loggingTrigger);

// Add caching
var cachingTrigger = new CachingTrigger<ulong, ILink<ulong>>(
    "ResultCache",
    TimeSpan.FromMinutes(5)
);
query.AddTrigger(cachingTrigger);
```

### Executing a Query / Выполнение запроса

```csharp
// Execute by name
var results = manager.ExecuteQuery<ILink<ulong>>("FindLinksBySource");

// Execute by ID
var results = manager.ExecuteQueryById<ILink<ulong>>(queryId);
```

### Viewing Statistics / Просмотр статистики

```csharp
var stats = query.Statistics;
Console.WriteLine($"Total Executions: {stats.TotalExecutions}");
Console.WriteLine($"Success Rate: {(double)stats.SuccessfulExecutions / stats.TotalExecutions:P}");
Console.WriteLine($"Average Time: {stats.AverageExecutionTime.TotalMilliseconds}ms");

// Get recent execution records
var recent = stats.GetRecentExecutions(10);
foreach (var record in recent)
{
    Console.WriteLine($"{record.Timestamp}: {record.ExecutionTime.TotalMilliseconds}ms");
}
```

### Managing Queries / Управление запросами

```csharp
// Disable a query
manager.DisableQuery("FindLinksBySource");

// Enable a query
manager.EnableQuery("FindLinksBySource");

// Delete a query
manager.DeleteQuery("FindLinksBySource");

// List all queries
var allQueries = manager.GetAllQueries();
```

## Implementation Details / Детали реализации

### Query Compilation / Компиляция запросов

Queries are compiled immediately upon creation using LINQ's `Expression.Compile()` method. This allows for:

Запросы компилируются сразу после создания с помощью метода `Expression.Compile()` LINQ. Это позволяет:

1. Fast execution without re-parsing
2. Type safety at compile time
3. Full LINQ query support

### Statistics Collection / Сбор статистики

Statistics are collected automatically on every query execution:

Статистика собирается автоматически при каждом выполнении запроса:

- Execution time is measured using high-precision timestamps
- Success/failure status is tracked
- Recent executions are stored (up to 1000 records by default)
- All statistics operations are thread-safe

### Trigger Execution / Выполнение триггеров

Triggers execute at specific points in the query lifecycle:

Триггеры выполняются в определённых точках жизненного цикла запроса:

1. **Before triggers** execute before the query
2. **Query execution** happens
3. **After triggers** execute if successful (with results)
4. **OnError triggers** execute if an exception occurs

### Thread Safety / Потокобезопасность

The implementation is thread-safe:

Реализация является потокобезопасной:

- `ConcurrentDictionary` is used for query storage
- Locks are used for statistics updates
- Query compilation is thread-safe

## Future Enhancements / Будущие улучшения

Potential improvements for future versions:

Потенциальные улучшения для будущих версий:

1. **Persistent Storage**: Store queries as links in the database
2. **Query Optimizer**: Analyze query patterns and suggest optimizations
3. **Distributed Execution**: Support for distributed query execution
4. **Query Plans**: Visual representation of query execution plans
5. **More Triggers**: Additional built-in triggers (alerting, metrics, etc.)
6. **Query Versioning**: Track query versions and changes over time
7. **Parameter Support**: Add support for parameterized queries
8. **Custom Functions**: Allow custom functions to replace query expressions

## Testing / Тестирование

See `experiments/StoredQueriesExample.cs` for a comprehensive example demonstrating all features.

Смотрите `experiments/StoredQueriesExample.cs` для полного примера, демонстрирующего все функции.

## Related Work / Связанная работа

This implementation builds upon the existing `QueryExecutorExtensions.cs` which provides basic query caching. Stored Queries extends this concept with:

Эта реализация основывается на существующем `QueryExecutorExtensions.cs`, который обеспечивает базовое кэширование запросов. Хранимые запросы расширяют эту концепцию:

- Better management and organization
- Statistics tracking
- Trigger support
- Enable/disable functionality

## References / Ссылки

- Issue #620: https://github.com/konard/LinksPlatform/issues/620
- Platform.Data.Doublets: https://github.com/linksplatform/Data.Doublets
