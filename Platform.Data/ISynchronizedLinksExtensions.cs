using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Setters;
using Platform.Data.Exceptions;
using Platform.Delegates;
using Platform.Threading.Synchronization;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data
{
    /// <summary>
    /// <para>
    /// Represents the synchronized links extensions.
    /// </para>
    /// <para></para>
    /// </summary>
    public static class ISynchronizedLinksExtensions
    {
        #region Write Operations (use DoWrite)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress Create<TLinkAddress, TLinks, TConstants>(this ISynchronizedLinks<TLinkAddress, TLinks, TConstants> links)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
            where TLinks : ILinks<TLinkAddress, TConstants>
            where TConstants : LinksConstants<TLinkAddress>
            => links.SyncRoot.DoWrite(() => links.Unsync.Create(null));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress Create<TLinkAddress, TLinks, TConstants>(this ISynchronizedLinks<TLinkAddress, TLinks, TConstants> links, IList<TLinkAddress>? substitution)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
            where TLinks : ILinks<TLinkAddress, TConstants>
            where TConstants : LinksConstants<TLinkAddress>
        {
            return links.SyncRoot.DoWrite(() =>
            {
                var constants = links.Constants;
                Setter<TLinkAddress, TLinkAddress> setter = new Setter<TLinkAddress, TLinkAddress>(constants.Continue, constants.Break, constants.Null);
                links.Unsync.Create(substitution, setter.SetFirstFromNonNullSecondListAndReturnTrue);
                return setter.Result;
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress Update<TLinkAddress, TLinks, TConstants>(this ISynchronizedLinks<TLinkAddress, TLinks, TConstants> links, IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
            where TLinks : ILinks<TLinkAddress, TConstants>
            where TConstants : LinksConstants<TLinkAddress>
        {
            return links.SyncRoot.DoWrite(() =>
            {
                var constants = links.Constants;
                Setter<TLinkAddress, TLinkAddress> setter = new(constants.Continue, constants.Break, constants.Null);
                links.Unsync.Update(restriction, substitution, setter.SetFirstFromNonNullSecondListAndReturnTrue);
                return setter.Result;
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress Delete<TLinkAddress, TLinks, TConstants>(this ISynchronizedLinks<TLinkAddress, TLinks, TConstants> links, TLinkAddress linkToDelete)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
            where TLinks : ILinks<TLinkAddress, TConstants>
            where TConstants : LinksConstants<TLinkAddress>
            => Delete(links, (IList<TLinkAddress>?)new LinkAddress<TLinkAddress>(linkToDelete));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress Delete<TLinkAddress, TLinks, TConstants>(this ISynchronizedLinks<TLinkAddress, TLinks, TConstants> links, IList<TLinkAddress>? restriction)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
            where TLinks : ILinks<TLinkAddress, TConstants>
            where TConstants : LinksConstants<TLinkAddress>
        {
            return links.SyncRoot.DoWrite(() =>
            {
                var constants = links.Constants;
                Setter<TLinkAddress, TLinkAddress> setter = new Setter<TLinkAddress, TLinkAddress>(constants.Continue, constants.Break, constants.Null);
                links.Unsync.Delete(restriction, setter.SetFirstFromNonNullFirstListAndReturnTrue);
                return setter.Result;
            });
        }

        #endregion

        #region Read Operations (use DoRead)

        /// <summary>
        /// <para>
        /// Counts the links.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The link address.</para>
        /// <para></para>
        /// </typeparam>
        /// <typeparam name="TLinks">
        /// <para>The links type.</para>
        /// <para></para>
        /// </typeparam>
        /// <typeparam name="TConstants">
        /// <para>The constants.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links.</para>
        /// <para></para>
        /// </param>
        /// <param name="restrictions">
        /// <para>The restrictions.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The link address</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress Count<TLinkAddress, TLinks, TConstants>(this ISynchronizedLinks<TLinkAddress, TLinks, TConstants> links, params TLinkAddress[] restrictions)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
            where TLinks : ILinks<TLinkAddress, TConstants>
            where TConstants : LinksConstants<TLinkAddress>
            => links.SyncRoot.DoRead(() => links.Unsync.Count(restrictions));

        /// <summary>
        /// Возвращает значение, определяющее существует ли связь с указанным индексом в хранилище связей.
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Индекс проверяемой на существование связи.</param>
        /// <returns>Значение, определяющее существует ли связь.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool Exists<TLinkAddress, TLinks, TConstants>(this ISynchronizedLinks<TLinkAddress, TLinks, TConstants> links, TLinkAddress link)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
            where TLinks : ILinks<TLinkAddress, TConstants>
            where TConstants : LinksConstants<TLinkAddress>
        {
            return links.SyncRoot.DoRead(() =>
            {
                var constants = links.Constants;
                return constants.IsExternalReference(link) || (constants.IsInternalReference(link) && Comparer<TLinkAddress>.Default.Compare(links.Unsync.Count(new LinkAddress<TLinkAddress>(link)), default) > 0);
            });
        }

        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Индекс проверяемой на существование связи.</param>
        /// <remarks>
        /// TODO: May be move to EnsureExtensions or make it both there and here
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureLinkExists<TLinkAddress, TLinks, TConstants>(this ISynchronizedLinks<TLinkAddress, TLinks, TConstants> links, TLinkAddress link)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
            where TLinks : ILinks<TLinkAddress, TConstants>
            where TConstants : LinksConstants<TLinkAddress>
        {
            if (!links.Exists(link))
            {
                throw new ArgumentLinkDoesNotExistsException<TLinkAddress>(link);
            }
        }

        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Индекс проверяемой на существование связи.</param>
        /// <param name="argumentName">Имя аргумента, в который передаётся индекс связи.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void EnsureLinkExists<TLinkAddress, TLinks, TConstants>(this ISynchronizedLinks<TLinkAddress, TLinks, TConstants> links, TLinkAddress link, string argumentName)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
            where TLinks : ILinks<TLinkAddress, TConstants>
            where TConstants : LinksConstants<TLinkAddress>
        {
            if (!links.Exists(link))
            {
                throw new ArgumentLinkDoesNotExistsException<TLinkAddress>(link, argumentName);
            }
        }

        /// <summary>
        /// Выполняет проход по всем связям, соответствующим шаблону, вызывая обработчик (handler) для каждой подходящей связи.
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="handler">Обработчик каждой подходящей связи.</param>
        /// <param name="restrictions">Ограничения на содержимое связей. Каждое ограничение может иметь значения: Constants.Null - 0-я связь, обозначающая ссылку на пустоту, Any - отсутствие ограничения, 1..∞ конкретный индекс связи.</param>
        /// <returns>True, в случае если проход по связям не был прерван и False в обратном случае.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TLinkAddress Each<TLinkAddress, TLinks, TConstants>(this ISynchronizedLinks<TLinkAddress, TLinks, TConstants> links, ReadHandler<TLinkAddress>? handler, params TLinkAddress[] restrictions)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
            where TLinks : ILinks<TLinkAddress, TConstants>
            where TConstants : LinksConstants<TLinkAddress>
            => links.SyncRoot.DoRead(() => links.Unsync.Each(restrictions, handler));

        /// <summary>
        /// Возвращает части-значения для связи с указанным индексом.
        /// </summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Индекс связи.</param>
        /// <returns>Уникальную связь.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<TLinkAddress>? GetLink<TLinkAddress, TLinks, TConstants>(this ISynchronizedLinks<TLinkAddress, TLinks, TConstants> links, TLinkAddress link)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
            where TLinks : ILinks<TLinkAddress, TConstants>
            where TConstants : LinksConstants<TLinkAddress>
        {
            return links.SyncRoot.DoRead(() =>
            {
                var constants = links.Constants;
                if (constants.IsExternalReference(link))
                {
                    return new Point<TLinkAddress>(link, constants.TargetPart + 1);
                }
                var linkPartsSetter = new Setter<IList<TLinkAddress>?, TLinkAddress>(constants.Continue, constants.Break);
                links.Unsync.Each(linkPartsSetter.SetAndReturnTrue, link);
                return linkPartsSetter.Result;
            });
        }

        #endregion

        #region Points

        /// <summary>Возвращает значение, определяющее является ли связь с указанным индексом точкой полностью (связью замкнутой на себе дважды).</summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Индекс проверяемой связи.</param>
        /// <returns>Значение, определяющее является ли связь точкой полностью.</returns>
        /// <remarks>
        /// Связь точка - это связь, у которой начало (Source) и конец (Target) есть сама эта связь.
        /// Но что, если точка уже есть, а нужно создать пару с таким же значением? Должны ли точка и пара существовать одновременно?
        /// Или в качестве решения для точек нужно использовать 0 в качестве начала и конца, а сортировать по индексу в массиве связей?
        /// Какое тогда будет значение Source и Target у точки? 0 или её индекс?
        /// Или точка должна быть одновременно точкой и парой, а также последовательностями из самой себя любого размера?
        /// Как только есть ссылка на себя, появляется этот парадокс, причём достаточно даже одной ссылки на себя (частичной точки).
        /// А что если не выбирать что является точкой, пара нулей (цикл через пустоту) или
        /// самостоятельный цикл через себя? Что если предоставить все варианты использования связей?
        /// Что если разрешить и нули, а так же частичные варианты?
        ///
        /// Что если точка, это только в том случае когда link.Source == link &amp;&amp; link.Target == link , т.е. дважды ссылка на себя.
        /// А пара это тогда, когда link.Source == link.Target &amp;&amp; link.Source != link , т.е. ссылка не на себя а во вне.
        ///
        /// Тогда если у нас уже создана пара, но нам нужна точка, мы можем используя промежуточную связь,
        /// например "DoubletOf" обозначить что является точно парой, а что точно точкой.
        /// И наоборот этот же метод поможет, если уже существует точка, но нам нужна пара.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFullPoint<TLinkAddress, TLinks, TConstants>(this ISynchronizedLinks<TLinkAddress, TLinks, TConstants> links, TLinkAddress link)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
            where TLinks : ILinks<TLinkAddress, TConstants>
            where TConstants : LinksConstants<TLinkAddress>
        {
            return links.SyncRoot.DoRead(() =>
            {
                if (links.Constants.IsExternalReference(link))
                {
                    return true;
                }
                links.EnsureLinkExists(link);
                return Point<TLinkAddress>.IsFullPoint(links.GetLink(link));
            });
        }

        /// <summary>Возвращает значение, определяющее является ли связь с указанным индексом точкой частично (связью замкнутой на себе как минимум один раз).</summary>
        /// <param name="links">Хранилище связей.</param>
        /// <param name="link">Индекс проверяемой связи.</param>
        /// <returns>Значение, определяющее является ли связь точкой частично.</returns>
        /// <remarks>
        /// Достаточно любой одной ссылки на себя.
        /// Также в будущем можно будет проверять и всех родителей, чтобы проверить есть ли ссылки на себя (на эту связь).
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPartialPoint<TLinkAddress, TLinks, TConstants>(this ISynchronizedLinks<TLinkAddress, TLinks, TConstants> links, TLinkAddress link)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
            where TLinks : ILinks<TLinkAddress, TConstants>
            where TConstants : LinksConstants<TLinkAddress>
        {
            return links.SyncRoot.DoRead(() =>
            {
                if (links.Constants.IsExternalReference(link))
                {
                    return true;
                }
                links.EnsureLinkExists(link);
                return Point<TLinkAddress>.IsPartialPoint(links.GetLink(link));
            });
        }

        #endregion
    }
}
