# Использование Doublets как хранилища ключ-значение

## Обзор

Doublets — это реализация базы данных, основанная на [ассоциативной модели данных](https://en.wikipedia.org/wiki/Associative_model_of_data). Хотя она работает на низком уровне, используя связи (пары), её можно эффективно использовать как хранилище ключ-значение, подобное Redis или другим NoSQL базам данных. Это руководство демонстрирует, как использовать Doublets в качестве системы хранения ключ-значение.

## Сравнение с Redis

Если вы знакомы с [Redis (клиент Jedis)](https://github.com/redis/jedis), вы найдете похожие концепции в Doublets:

| Концепция Redis | Эквивалент в Doublets | Описание |
|-----------------|----------------------|----------|
| Соединение | `UnitedMemoryLinks<uint>` | Создать соединение с хранилищем данных |
| SET key value | `PropertiesOperator.SetValue()` | Сохранить значение с ключом |
| GET key | `PropertiesOperator.GetValue()` | Получить значение по ключу |
| Ключ (строка) | Адрес связи или маркер | Идентификатор ваших данных |
| Значение (строка/объект) | Связь или последовательность | Сохраненные данные |

## Быстрый старт

### 1. Установка

Добавьте необходимые NuGet пакеты в ваш проект:

```bash
dotnet add package Platform.Data.Doublets
```

### 2. Базовая настройка

```csharp
using System;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.PropertyOperators;

// Создать или открыть хранилище дуплетов (аналогично подключению к Redis)
using var links = new UnitedMemoryLinks<uint>("my-store.links");

// Создать оператор свойств для операций ключ-значение
var properties = new PropertiesOperator<uint>(links);
```

## Базовые операции

### Создание ключей (маркеров свойств)

Перед сохранением значений создайте маркеры, которые будут выступать в качестве ваших ключей:

```csharp
// Создать маркеры свойств (ключи)
var usernameKey = links.CreatePoint();  // Связь, которая ссылается сама на себя
var emailKey = links.CreatePoint();
var ageKey = links.CreatePoint();
```

### Сохранение значений (операция SET)

```csharp
// Создать объект (как Redis hash или документ)
var user1 = links.CreatePoint();

// Сохранить простые значения-связи
properties.SetValue(user1, usernameKey, someValueLink);

// Для сложных данных, таких как строки, нужно конвертировать их в последовательности
// (См. раздел "Работа со строками" ниже)
```

### Получение значений (операция GET)

```csharp
// Получить значение для конкретного объекта и свойства
var username = properties.GetValue(user1, usernameKey);

if (username != default)
{
    Console.WriteLine($"Связь имени пользователя: {username}");
}
else
{
    Console.WriteLine("Имя пользователя не найдено");
}
```

### Удаление значений

```csharp
// Чтобы удалить значение свойства, установите его в default или null-маркер
// Или удалите всю связь объекта
links.Delete(user1);
```

## Работа со строками

Поскольку Doublets работает со связями, а не напрямую со строками, нужно конвертировать строки в последовательности связей символов. Вот полный пример:

```csharp
using System;
using System.Text;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.PropertyOperators;
using Platform.Data.Doublets.Sequences.Converters;
using Platform.Data.Doublets.Unicode;

// Инициализировать хранилище
using var links = new UnitedMemoryLinks<uint>("kv-store.links");
var properties = new PropertiesOperator<uint>(links);

// Настроить конвертеры строк
var unicodeMap = new UnicodeMap<uint>(links);
var addressToNumberConverter = new AddressToRawNumberSequenceConverter<uint>(links);
var stringToSequenceConverter = new CachingConverterDecorator<string, uint>(
    new StringToUnicodeSequenceConverter<uint>(links, unicodeMap, addressToNumberConverter)
);
var sequenceToStringConverter = new UnicodeSequenceToStringConverter<uint>(links, unicodeMap);

// Вспомогательные методы
uint StringToLink(string str) => stringToSequenceConverter.Convert(str);
string LinkToString(uint link) => sequenceToStringConverter.Convert(link);

// Создать ключи
var nameKey = links.CreatePoint();
var emailKey = links.CreatePoint();

// Создать объект пользователя
var user = links.CreatePoint();

// Сохранить строковые значения (как Redis SET)
properties.SetValue(user, nameKey, StringToLink("Иван Иванов"));
properties.SetValue(user, emailKey, StringToLink("ivan@example.com"));

// Получить строковые значения (как Redis GET)
var nameLink = properties.GetValue(user, nameKey);
var emailLink = properties.GetValue(user, emailKey);

if (nameLink != default)
{
    Console.WriteLine($"Имя: {LinkToString(nameLink)}");
}

if (emailLink != default)
{
    Console.WriteLine($"Email: {LinkToString(emailLink)}");
}
```

## Продвинутые паттерны

### Структура типа Hash (Redis HSET/HGET)

```csharp
// В Redis: HSET user:1 username "john" email "john@example.com"
// В Doublets:

var user1 = links.CreatePoint();  // Как "user:1"

// Установить несколько свойств
properties.SetValue(user1, usernameKey, StringToLink("ivan"));
properties.SetValue(user1, emailKey, StringToLink("ivan@example.com"));
properties.SetValue(user1, ageKey, links.CreatePoint()); // Для числовых значений используйте соответствующий конвертер

// Получить все свойства
var username = LinkToString(properties.GetValue(user1, usernameKey));
var email = LinkToString(properties.GetValue(user1, emailKey));
```

### Множественные объекты (множественные ключи)

```csharp
// Создать несколько пользователей
var user1 = links.CreatePoint();
var user2 = links.CreatePoint();
var user3 = links.CreatePoint();

// Сохранить разные значения для каждого
properties.SetValue(user1, nameKey, StringToLink("Алиса"));
properties.SetValue(user2, nameKey, StringToLink("Боб"));
properties.SetValue(user3, nameKey, StringToLink("Чарли"));

// Получить имя конкретного пользователя
var aliceName = LinkToString(properties.GetValue(user1, nameKey));
Console.WriteLine($"Пользователь 1: {aliceName}");
```

### Обновление значений

```csharp
// Обновление — это то же самое, что и Set — автоматически заменяет старое значение
properties.SetValue(user1, emailKey, StringToLink("newemail@example.com"));
```

## Полный пример

Вот полный рабочий пример, демонстрирующий простую систему управления пользователями:

```csharp
using System;
using Platform.Data;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.PropertyOperators;
using Platform.Data.Doublets.Sequences.Converters;
using Platform.Data.Doublets.Unicode;

public class DoubletsKeyValueExample
{
    private readonly UnitedMemoryLinks<uint> _links;
    private readonly PropertiesOperator<uint> _properties;
    private readonly Func<string, uint> _stringToLink;
    private readonly Func<uint, string> _linkToString;

    // Ключи свойств
    private readonly uint _nameKey;
    private readonly uint _emailKey;
    private readonly uint _ageKey;

    public DoubletsKeyValueExample(string dbPath)
    {
        // Инициализировать хранилище
        _links = new UnitedMemoryLinks<uint>(dbPath);
        _properties = new PropertiesOperator<uint>(_links);

        // Настроить конвертеры
        var unicodeMap = new UnicodeMap<uint>(_links);
        var addressToNumberConverter = new AddressToRawNumberSequenceConverter<uint>(_links);
        var stringToSeqConverter = new CachingConverterDecorator<string, uint>(
            new StringToUnicodeSequenceConverter<uint>(_links, unicodeMap, addressToNumberConverter)
        );
        var seqToStringConverter = new UnicodeSequenceToStringConverter<uint>(_links, unicodeMap);

        _stringToLink = str => stringToSeqConverter.Convert(str);
        _linkToString = link => seqToStringConverter.Convert(link);

        // Создать ключи свойств (использовать повторно, если они уже существуют)
        _nameKey = _links.CreatePoint();
        _emailKey = _links.CreatePoint();
        _ageKey = _links.CreatePoint();
    }

    public uint CreateUser(string name, string email)
    {
        var user = _links.CreatePoint();
        _properties.SetValue(user, _nameKey, _stringToLink(name));
        _properties.SetValue(user, _emailKey, _stringToLink(email));
        return user;
    }

    public string GetUserName(uint userId)
    {
        var nameLink = _properties.GetValue(userId, _nameKey);
        return nameLink != default ? _linkToString(nameLink) : null;
    }

    public string GetUserEmail(uint userId)
    {
        var emailLink = _properties.GetValue(userId, _emailKey);
        return emailLink != default ? _linkToString(emailLink) : null;
    }

    public void UpdateUserEmail(uint userId, string newEmail)
    {
        _properties.SetValue(userId, _emailKey, _stringToLink(newEmail));
    }

    public void DeleteUser(uint userId)
    {
        _links.Delete(userId);
    }

    public void Dispose()
    {
        _links?.Dispose();
    }

    public static void Main()
    {
        using var kvStore = new DoubletsKeyValueExample("users.links");

        // Создать пользователей (как операции Redis SET)
        var user1 = kvStore.CreateUser("Алиса", "alice@example.com");
        var user2 = kvStore.CreateUser("Боб", "bob@example.com");

        // Прочитать пользователей (как операции Redis GET)
        Console.WriteLine($"Имя пользователя 1: {kvStore.GetUserName(user1)}");
        Console.WriteLine($"Email пользователя 1: {kvStore.GetUserEmail(user1)}");

        // Обновить пользователя (как Redis SET с существующим ключом)
        kvStore.UpdateUserEmail(user1, "alice.new@example.com");
        Console.WriteLine($"Обновленный email пользователя 1: {kvStore.GetUserEmail(user1)}");

        // Удалить пользователя (как Redis DEL)
        kvStore.DeleteUser(user2);
        Console.WriteLine($"Пользователь 2 после удаления: {kvStore.GetUserName(user2) ?? "Не найден"}");
    }
}
```

## Детали реализации

### Как это работает внутри

`PropertiesOperator` реализует хранилище ключ-значение, используя паттерн из трех связей:

1. **Связь объекта**: Представляет сущность (как `user:1` в Redis)
2. **Связь свойства**: Соединяет объект с его ключом свойства (как `username`)
3. **Связь значения**: Хранит фактическое значение

Отношение: `Объект -> (Объект-Свойство) -> Значение`

Когда вы вызываете `SetValue(object, property, value)`:
- Создает или находит связь `объект-свойство`
- Удаляет любые существующие значения для этой комбинации объект-свойство
- Создает новую связь `(объект-свойство) -> значение`

Когда вы вызываете `GetValue(object, property)`:
- Ищет связь `объект-свойство`
- Находит связи, где источник — это `объект-свойство`
- Возвращает цель этой связи (значение)

## Соображения производительности

- **Файлы, отображаемые в памяти**: Doublets использует файлы, отображаемые в памяти, для быстрого ввода-вывода
- **Переиспользование связей**: Общие подпоследовательности автоматически дедуплицируются
- **Индексирование**: Doublets поддерживает индексы для быстрого поиска
- **Пакетные операции**: Для лучшей производительности рассмотрите пакетирование нескольких операций

## Ссылки

- [Репозиторий Doublets](https://github.com/linksplatform/Data.Doublets)
- [Сравнение SQLite vs Doublets](https://github.com/linksplatform/Comparisons.SQLiteVSDoublets) - Показывает, как `PropertiesOperator` используется для операций, подобных базе данных
- [Исходный код PropertiesOperator](https://github.com/linksplatform/Data.Doublets/blob/main/csharp/Platform.Data.Doublets/PropertyOperators/PropertiesOperator.cs)
- [Примеры CRUD](https://github.com/linksplatform/Examples.Doublets.CRUD.DotNet)

## Резюме

Doublets предоставляет мощную, гибкую систему хранения ключ-значение, основанную на ассоциативных связях. Хотя она требует понимания концепций, основанных на связях, она предлагает уникальные преимущества:

- **Структурное разделение**: Автоматическая дедупликация общих шаблонов данных
- **Гибкость**: Может представлять любую структуру данных, не только ключ-значение
- **Производительность**: Хранилище, отображаемое в памяти, с эффективным индексированием
- **Типобезопасность**: Обобщенная реализация с проверкой типов во время компиляции

Для простых операций ключ-значение используйте класс `PropertiesOperator`, как показано в этом руководстве. Для более сложных сценариев вы можете работать непосредственно со связями для создания пользовательских структур данных.
