using System;
using System.Collections.Generic;
using System.Linq;

namespace Platform.Data.Triggers.Tests
{
    /// <summary>
    /// Mock link storage for testing purposes.
    /// </summary>
    public class MockLinkStorage : ILinkStorage
    {
        private readonly List<MockLink> _links = new List<MockLink>();
        private int _nextId = 1;

        public class MockLink
        {
            public int Id { get; set; }
            public object Source { get; set; }
            public object Linker { get; set; }
            public object Target { get; set; }
        }

        public object CreateLink(object source, object linker, object target)
        {
            var link = new MockLink
            {
                Id = _nextId++,
                Source = source,
                Linker = linker,
                Target = target
            };
            _links.Add(link);
            return link;
        }

        public void UpdateLink(object link, object newSource, object newLinker, object newTarget)
        {
            var mockLink = link as MockLink;
            if (mockLink != null)
            {
                mockLink.Source = newSource;
                mockLink.Linker = newLinker;
                mockLink.Target = newTarget;
            }
        }

        public void DeleteLink(object link)
        {
            var mockLink = link as MockLink;
            if (mockLink != null)
            {
                _links.Remove(mockLink);
            }
        }

        public IEnumerable<object> SearchLinks(object source, object linker, object target)
        {
            return _links.Where(l =>
                (source == null || Equals(l.Source, source)) &&
                (linker == null || Equals(l.Linker, linker)) &&
                (target == null || Equals(l.Target, target)));
        }

        public object GetSource(object link)
        {
            return (link as MockLink)?.Source;
        }

        public object GetLinker(object link)
        {
            return (link as MockLink)?.Linker;
        }

        public object GetTarget(object link)
        {
            return (link as MockLink)?.Target;
        }

        public IEnumerable<MockLink> GetAllLinks()
        {
            return _links.AsReadOnly();
        }
    }
}
