using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// <para>
    /// Provides set operations on links.
    /// </para>
    /// <para>
    /// Представляет операции над множествами на связях.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <para>
    /// A set is represented as a collection of links where each link has the form (set_id, element_id).
    /// For example, if set 'a' contains elements x, y, z, it is represented as three links:
    /// (a, x), (a, y), (a, z)
    /// </para>
    /// <para>
    /// Множество представлено как коллекция связей, где каждая связь имеет форму (идентификатор_множества, идентификатор_элемента).
    /// Например, если множество 'a' содержит элементы x, y, z, оно представлено тремя связями:
    /// (a, x), (a, y), (a, z)
    /// </para>
    /// </remarks>
    public class Sets<TLink>
    {
        private readonly ILinks<TLink> _links;

        /// <summary>
        /// <para>
        /// Initializes a new instance of the <see cref="Sets{TLink}"/> class.
        /// </para>
        /// <para>
        /// Инициализирует новый экземпляр класса <see cref="Sets{TLink}"/>.
        /// </para>
        /// </summary>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para>Хранилище связей.</para>
        /// </param>
        public Sets(ILinks<TLink> links)
        {
            _links = links ?? throw new ArgumentNullException(nameof(links));
        }

        /// <summary>
        /// <para>
        /// Checks if an element is contained in a set.
        /// </para>
        /// <para>
        /// Проверяет, содержится ли элемент в множестве.
        /// </para>
        /// </summary>
        /// <param name="setId">
        /// <para>The identifier of the set.</para>
        /// <para>Идентификатор множества.</para>
        /// </param>
        /// <param name="elementId">
        /// <para>The identifier of the element.</para>
        /// <para>Идентификатор элемента.</para>
        /// </param>
        /// <returns>
        /// <para>True if the element is in the set, false otherwise.</para>
        /// <para>True, если элемент содержится в множестве, иначе false.</para>
        /// </returns>
        public bool Contains(TLink setId, TLink elementId)
        {
            var link = _links.SearchOrDefault(setId, elementId);
            return !EqualityComparer<TLink>.Default.Equals(link, default);
        }

        /// <summary>
        /// <para>
        /// Adds an element to a set.
        /// </para>
        /// <para>
        /// Добавляет элемент в множество.
        /// </para>
        /// </summary>
        /// <param name="setId">
        /// <para>The identifier of the set.</para>
        /// <para>Идентификатор множества.</para>
        /// </param>
        /// <param name="elementId">
        /// <para>The identifier of the element.</para>
        /// <para>Идентификатор элемента.</para>
        /// </param>
        /// <returns>
        /// <para>The identifier of the created link (setId, elementId).</para>
        /// <para>Идентификатор созданной связи (setId, elementId).</para>
        /// </returns>
        /// <remarks>
        /// <para>
        /// If the element already exists in the set, the existing link is returned.
        /// </para>
        /// <para>
        /// Если элемент уже существует в множестве, возвращается существующая связь.
        /// </para>
        /// </remarks>
        public TLink AddToSet(TLink setId, TLink elementId)
        {
            return _links.GetOrCreate(setId, elementId);
        }

        /// <summary>
        /// <para>
        /// Gets all elements of a set.
        /// </para>
        /// <para>
        /// Получает все элементы множества.
        /// </para>
        /// </summary>
        /// <param name="setId">
        /// <para>The identifier of the set.</para>
        /// <para>Идентификатор множества.</para>
        /// </param>
        /// <returns>
        /// <para>A list of element identifiers in the set.</para>
        /// <para>Список идентификаторов элементов в множестве.</para>
        /// </returns>
        public List<TLink> GetElements(TLink setId)
        {
            var elements = new List<TLink>();
            var constants = _links.Constants;
            var equalityComparer = EqualityComparer<TLink>.Default;

            _links.Each(setId, constants.Any, link =>
            {
                var element = link[constants.TargetPart];
                // Exclude self-references (points) - they are not set elements
                if (!equalityComparer.Equals(element, setId))
                {
                    elements.Add(element);
                }
                return constants.Continue;
            });

            return elements;
        }

        /// <summary>
        /// <para>
        /// Checks if two sets are equal according to set theory semantics.
        /// </para>
        /// <para>
        /// Проверяет равенство двух множеств согласно семантике теории множеств.
        /// </para>
        /// </summary>
        /// <param name="setId1">
        /// <para>The identifier of the first set.</para>
        /// <para>Идентификатор первого множества.</para>
        /// </param>
        /// <param name="setId2">
        /// <para>The identifier of the second set.</para>
        /// <para>Идентификатор второго множества.</para>
        /// </param>
        /// <returns>
        /// <para>
        /// True if both sets contain the same elements (at least once each),
        /// regardless of order or repetitions, false otherwise.
        /// </para>
        /// <para>
        /// True, если оба множества содержат одинаковые элементы (каждый хотя бы один раз),
        /// независимо от порядка или повторений, иначе false.
        /// </para>
        /// </returns>
        /// <remarks>
        /// <para>
        /// Two sets are equal when they contain the same unique elements.
        /// Repetitions of elements and their order do not affect equality.
        /// Computational complexity: O(N*log(M)) where N is the size of the smaller set
        /// and M is the total number of links in storage.
        /// </para>
        /// <para>
        /// Два множества равны, когда они содержат одинаковые уникальные элементы.
        /// Повторы элементов и их порядок не влияют на равенство.
        /// Вычислительная сложность: O(N*log(M)) где N это размер минимального множества
        /// и M это общее количество связей в хранилище.
        /// </para>
        /// </remarks>
        public bool AreEqual(TLink setId1, TLink setId2)
        {
            var elements1 = GetElements(setId1).Distinct().ToList();
            var elements2 = GetElements(setId2).Distinct().ToList();

            if (elements1.Count != elements2.Count)
            {
                return false;
            }

            // Check if all elements from set1 are in set2
            foreach (var element in elements1)
            {
                if (!elements2.Contains(element))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// <para>
        /// Removes an element from a set.
        /// </para>
        /// <para>
        /// Удаляет элемент из множества.
        /// </para>
        /// </summary>
        /// <param name="setId">
        /// <para>The identifier of the set.</para>
        /// <para>Идентификатор множества.</para>
        /// </param>
        /// <param name="elementId">
        /// <para>The identifier of the element to remove.</para>
        /// <para>Идентификатор элемента для удаления.</para>
        /// </param>
        /// <returns>
        /// <para>True if at least one link was deleted, false otherwise.</para>
        /// <para>True, если хотя бы одна связь была удалена, иначе false.</para>
        /// </returns>
        public bool RemoveFromSet(TLink setId, TLink elementId)
        {
            var link = _links.SearchOrDefault(setId, elementId);
            if (!EqualityComparer<TLink>.Default.Equals(link, default))
            {
                _links.Delete(link);
                return true;
            }
            return false;
        }

        /// <summary>
        /// <para>
        /// Gets the count of unique elements in a set.
        /// </para>
        /// <para>
        /// Получает количество уникальных элементов в множестве.
        /// </para>
        /// </summary>
        /// <param name="setId">
        /// <para>The identifier of the set.</para>
        /// <para>Идентификатор множества.</para>
        /// </param>
        /// <returns>
        /// <para>The number of unique elements in the set.</para>
        /// <para>Количество уникальных элементов в множестве.</para>
        /// </returns>
        public int GetCardinality(TLink setId)
        {
            return GetElements(setId).Distinct().Count();
        }
    }
}
