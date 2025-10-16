using System;
using System.Collections.Generic;
using Platform.Data;
using Platform.Data.Doublets;

namespace Platform.Examples
{
    /// <summary>
    /// A decorator for ILinks that logs all CRUD operations in real-time.
    /// </summary>
    public class EventLoggingLinks<TLink> : ILinks<TLink>
    {
        private readonly ILinks<TLink> _decorated;
        private readonly Action<string> _logHandler;

        public EventLoggingLinks(ILinks<TLink> decorated, Action<string> logHandler)
        {
            _decorated = decorated;
            _logHandler = logHandler;
        }

        public LinksConstants<TLink> Constants => _decorated.Constants;

        public TLink Count() => _decorated.Count();

        public TLink Count(IList<TLink> restrictions) => _decorated.Count(restrictions);

        public TLink Create()
        {
            var link = _decorated.Create();
            _logHandler($"[EVENT] Created link: {link}");
            return link;
        }

        public TLink Create(IList<TLink> restrictions)
        {
            var link = _decorated.Create(restrictions);
            _logHandler($"[EVENT] Created link: {link} with restrictions");
            return link;
        }

        public TLink Update(TLink link, TLink newSource, TLink newTarget)
        {
            var oldSource = _decorated.GetSource(link);
            var oldTarget = _decorated.GetTarget(link);
            var result = _decorated.Update(link, newSource, newTarget);
            _logHandler($"[EVENT] Updated link: {link} ({oldSource} -> {oldTarget}) changed to ({newSource} -> {newTarget})");
            return result;
        }

        public TLink Update(IList<TLink> restrictions, IList<TLink> substitution)
        {
            var result = _decorated.Update(restrictions, substitution);
            _logHandler($"[EVENT] Updated link(s) with restrictions/substitution");
            return result;
        }

        public void Delete(TLink link)
        {
            var source = _decorated.GetSource(link);
            var target = _decorated.GetTarget(link);
            _decorated.Delete(link);
            _logHandler($"[EVENT] Deleted link: {link} ({source} -> {target})");
        }

        public void Delete(IList<TLink> restrictions)
        {
            _decorated.Delete(restrictions);
            _logHandler($"[EVENT] Deleted link(s) with restrictions");
        }

        public TLink Each(Func<IList<TLink>, TLink> handler, IList<TLink> restrictions)
        {
            return _decorated.Each(handler, restrictions);
        }

        public TLink GetSource(TLink link) => _decorated.GetSource(link);

        public TLink GetTarget(TLink link) => _decorated.GetTarget(link);
    }
}
