using System;

namespace Platform.Examples
{
    /// <summary xml:lang="en">
    /// Example class demonstrating multilingual XML documentation support.
    /// This class shows how to use xml:lang attribute for internationalization.
    /// </summary>
    /// <summary xml:lang="ru">
    /// Пример класса, демонстрирующий поддержку многоязычной XML-документации.
    /// Этот класс показывает, как использовать атрибут xml:lang для интернационализации.
    /// </summary>
    /// <remarks xml:lang="en">
    /// The xml:lang attribute can be applied to any XML documentation tag
    /// to provide language-specific content. This approach allows developers
    /// to maintain multiple language versions directly in the source code.
    /// </remarks>
    /// <remarks xml:lang="ru">
    /// Атрибут xml:lang может быть применен к любому тегу XML-документации
    /// для предоставления контента на конкретном языке. Этот подход позволяет разработчикам
    /// поддерживать версии на нескольких языках непосредственно в исходном коде.
    /// </remarks>
    public class MultilingualDocumentationExample
    {
        /// <summary xml:lang="en">
        /// Gets or sets the unique identifier.
        /// </summary>
        /// <summary xml:lang="ru">
        /// Получает или устанавливает уникальный идентификатор.
        /// </summary>
        public ulong Id { get; set; }

        /// <summary xml:lang="en">
        /// Gets or sets the name of the entity.
        /// </summary>
        /// <summary xml:lang="ru">
        /// Получает или устанавливает имя сущности.
        /// </summary>
        public string Name { get; set; }

        /// <summary xml:lang="en">
        /// Initializes a new instance of the MultilingualDocumentationExample class.
        /// </summary>
        /// <summary xml:lang="ru">
        /// Инициализирует новый экземпляр класса MultilingualDocumentationExample.
        /// </summary>
        public MultilingualDocumentationExample()
        {
            Id = 0;
            Name = string.Empty;
        }

        /// <summary xml:lang="en">
        /// Initializes a new instance with specified id and name.
        /// </summary>
        /// <summary xml:lang="ru">
        /// Инициализирует новый экземпляр с указанным идентификатором и именем.
        /// </summary>
        /// <param name="id" xml:lang="en">The unique identifier.</param>
        /// <param name="id" xml:lang="ru">Уникальный идентификатор.</param>
        /// <param name="name" xml:lang="en">The entity name.</param>
        /// <param name="name" xml:lang="ru">Имя сущности.</param>
        public MultilingualDocumentationExample(ulong id, string name)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary xml:lang="en">
        /// Validates the current state of the object.
        /// </summary>
        /// <summary xml:lang="ru">
        /// Проверяет текущее состояние объекта.
        /// </summary>
        /// <returns xml:lang="en">
        /// True if the object is in a valid state; otherwise, false.
        /// </returns>
        /// <returns xml:lang="ru">
        /// True, если объект находится в допустимом состоянии; в противном случае - false.
        /// </returns>
        /// <example xml:lang="en">
        /// <code>
        /// var example = new MultilingualDocumentationExample(1, "Test");
        /// bool isValid = example.Validate();
        /// Console.WriteLine($"Is valid: {isValid}");
        /// </code>
        /// </example>
        /// <example xml:lang="ru">
        /// <code>
        /// var example = new MultilingualDocumentationExample(1, "Тест");
        /// bool isValid = example.Validate();
        /// Console.WriteLine($"Допустимый: {isValid}");
        /// </code>
        /// </example>
        public bool Validate()
        {
            return Id > 0 && !string.IsNullOrWhiteSpace(Name);
        }

        /// <summary xml:lang="en">
        /// Processes the entity with the specified operation.
        /// </summary>
        /// <summary xml:lang="ru">
        /// Обрабатывает сущность с указанной операцией.
        /// </summary>
        /// <param name="operation" xml:lang="en">The operation to perform.</param>
        /// <param name="operation" xml:lang="ru">Операция для выполнения.</param>
        /// <exception cref="ArgumentNullException" xml:lang="en">
        /// Thrown when operation is null.
        /// </exception>
        /// <exception cref="ArgumentNullException" xml:lang="ru">
        /// Выбрасывается, когда operation имеет значение null.
        /// </exception>
        /// <exception cref="InvalidOperationException" xml:lang="en">
        /// Thrown when the entity is in an invalid state.
        /// </exception>
        /// <exception cref="InvalidOperationException" xml:lang="ru">
        /// Выбрасывается, когда сущность находится в недопустимом состоянии.
        /// </exception>
        public void Process(Action<MultilingualDocumentationExample> operation)
        {
            if (operation == null)
            {
                throw new ArgumentNullException(nameof(operation));
            }

            if (!Validate())
            {
                throw new InvalidOperationException("Entity is in an invalid state.");
            }

            operation(this);
        }

        /// <summary xml:lang="en">
        /// Converts the entity to a formatted string representation.
        /// </summary>
        /// <summary xml:lang="ru">
        /// Преобразует сущность в форматированное строковое представление.
        /// </summary>
        /// <returns xml:lang="en">
        /// A string that represents the current entity.
        /// </returns>
        /// <returns xml:lang="ru">
        /// Строка, представляющая текущую сущность.
        /// </returns>
        public override string ToString()
        {
            return $"[{Id}] {Name}";
        }
    }

    /// <summary xml:lang="en">
    /// Static utility class for working with multilingual documentation examples.
    /// </summary>
    /// <summary xml:lang="ru">
    /// Статический вспомогательный класс для работы с примерами многоязычной документации.
    /// </summary>
    public static class MultilingualDocumentationHelper
    {
        /// <summary xml:lang="en">
        /// Creates a new example instance with default values.
        /// </summary>
        /// <summary xml:lang="ru">
        /// Создает новый экземпляр примера со значениями по умолчанию.
        /// </summary>
        /// <returns xml:lang="en">
        /// A new instance of MultilingualDocumentationExample.
        /// </returns>
        /// <returns xml:lang="ru">
        /// Новый экземпляр MultilingualDocumentationExample.
        /// </returns>
        public static MultilingualDocumentationExample CreateDefault()
        {
            return new MultilingualDocumentationExample(1, "Default Example");
        }

        /// <summary xml:lang="en">
        /// Compares two instances for equality based on their identifiers.
        /// </summary>
        /// <summary xml:lang="ru">
        /// Сравнивает два экземпляра на равенство на основе их идентификаторов.
        /// </summary>
        /// <param name="first" xml:lang="en">The first instance to compare.</param>
        /// <param name="first" xml:lang="ru">Первый экземпляр для сравнения.</param>
        /// <param name="second" xml:lang="en">The second instance to compare.</param>
        /// <param name="second" xml:lang="ru">Второй экземпляр для сравнения.</param>
        /// <returns xml:lang="en">
        /// True if both instances have the same identifier; otherwise, false.
        /// </returns>
        /// <returns xml:lang="ru">
        /// True, если оба экземпляра имеют одинаковый идентификатор; в противном случае - false.
        /// </returns>
        public static bool AreEqual(MultilingualDocumentationExample first, MultilingualDocumentationExample second)
        {
            if (first == null && second == null)
            {
                return true;
            }

            if (first == null || second == null)
            {
                return false;
            }

            return first.Id == second.Id;
        }
    }
}
