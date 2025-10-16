using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Platform.Data.Triplets.Memory
{
    /// <summary>
    /// Represents a triplet link with Source, Linker, and Target components.
    /// Each link can reference three other links, forming a graph structure.
    /// </summary>
    public partial class Link
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Link? _source;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Link? _linker;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Link? _target;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Link? _firstRefererBySource;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Link? _firstRefererByLinker;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Link? _firstRefererByTarget;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Link? _nextSiblingRefererBySource;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Link? _nextSiblingRefererByLinker;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Link? _nextSiblingRefererByTarget;

        /// <summary>
        /// Gets or sets the source link of this triplet.
        /// When changed, updates the referrer chain in both old and new source links.
        /// </summary>
        public Link? Source
        {
            get => _source;
            set
            {
                if (_source != value)
                {
                    RemoveFromSourceChain();
                    _source = value;
                    AddToSourceChain(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the linker link of this triplet.
        /// When changed, updates the referrer chain in both old and new linker links.
        /// </summary>
        public Link? Linker
        {
            get => _linker;
            set
            {
                if (_linker != value)
                {
                    RemoveFromLinkerChain();
                    _linker = value;
                    AddToLinkerChain(value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the target link of this triplet.
        /// When changed, updates the referrer chain in both old and new target links.
        /// </summary>
        public Link? Target
        {
            get => _target;
            set
            {
                if (_target != value)
                {
                    RemoveFromTargetChain();
                    _target = value;
                    AddToTargetChain(value);
                }
            }
        }

        /// <summary>
        /// Gets all links that reference this link as their source.
        /// </summary>
        public IEnumerable<Link> ReferersBySource
        {
            get
            {
                var referer = _firstRefererBySource;
                while (referer != null)
                {
                    yield return referer;
                    referer = referer._nextSiblingRefererBySource;
                }
            }
        }

        /// <summary>
        /// Gets all links that reference this link as their linker.
        /// </summary>
        public IEnumerable<Link> ReferersByLinker
        {
            get
            {
                var referer = _firstRefererByLinker;
                while (referer != null)
                {
                    yield return referer;
                    referer = referer._nextSiblingRefererByLinker;
                }
            }
        }

        /// <summary>
        /// Gets all links that reference this link as their target.
        /// </summary>
        public IEnumerable<Link> ReferersByTarget
        {
            get
            {
                var referer = _firstRefererByTarget;
                while (referer != null)
                {
                    yield return referer;
                    referer = referer._nextSiblingRefererByTarget;
                }
            }
        }

        /// <summary>
        /// Removes this link from a referrer chain by source.
        /// </summary>
        private void RemoveFromSourceChain()
        {
            if (_source == null)
                return;

            if (_source._firstRefererBySource == this)
            {
                _source._firstRefererBySource = _nextSiblingRefererBySource;
            }
            else
            {
                var current = _source._firstRefererBySource;
                while (current != null && current._nextSiblingRefererBySource != this)
                {
                    current = current._nextSiblingRefererBySource;
                }
                if (current != null)
                {
                    current._nextSiblingRefererBySource = _nextSiblingRefererBySource;
                }
            }
            _nextSiblingRefererBySource = null;
        }

        /// <summary>
        /// Removes this link from a referrer chain by linker.
        /// </summary>
        private void RemoveFromLinkerChain()
        {
            if (_linker == null)
                return;

            if (_linker._firstRefererByLinker == this)
            {
                _linker._firstRefererByLinker = _nextSiblingRefererByLinker;
            }
            else
            {
                var current = _linker._firstRefererByLinker;
                while (current != null && current._nextSiblingRefererByLinker != this)
                {
                    current = current._nextSiblingRefererByLinker;
                }
                if (current != null)
                {
                    current._nextSiblingRefererByLinker = _nextSiblingRefererByLinker;
                }
            }
            _nextSiblingRefererByLinker = null;
        }

        /// <summary>
        /// Removes this link from a referrer chain by target.
        /// </summary>
        private void RemoveFromTargetChain()
        {
            if (_target == null)
                return;

            if (_target._firstRefererByTarget == this)
            {
                _target._firstRefererByTarget = _nextSiblingRefererByTarget;
            }
            else
            {
                var current = _target._firstRefererByTarget;
                while (current != null && current._nextSiblingRefererByTarget != this)
                {
                    current = current._nextSiblingRefererByTarget;
                }
                if (current != null)
                {
                    current._nextSiblingRefererByTarget = _nextSiblingRefererByTarget;
                }
            }
            _nextSiblingRefererByTarget = null;
        }

        /// <summary>
        /// Adds this link to a referrer chain by source.
        /// </summary>
        private void AddToSourceChain(Link? newSource)
        {
            if (newSource == null)
                return;

            _nextSiblingRefererBySource = newSource._firstRefererBySource;
            newSource._firstRefererBySource = this;
        }

        /// <summary>
        /// Adds this link to a referrer chain by linker.
        /// </summary>
        private void AddToLinkerChain(Link? newLinker)
        {
            if (newLinker == null)
                return;

            _nextSiblingRefererByLinker = newLinker._firstRefererByLinker;
            newLinker._firstRefererByLinker = this;
        }

        /// <summary>
        /// Adds this link to a referrer chain by target.
        /// </summary>
        private void AddToTargetChain(Link? newTarget)
        {
            if (newTarget == null)
                return;

            _nextSiblingRefererByTarget = newTarget._firstRefererByTarget;
            newTarget._firstRefererByTarget = this;
        }

        /// <summary>
        /// Checks if this link has been deleted (has no referers).
        /// </summary>
        /// <returns>True if the link is deleted, false otherwise.</returns>
        public bool IsDeleted()
        {
            return _firstRefererBySource == null
                && _firstRefererByLinker == null
                && _firstRefererByTarget == null;
        }

        /// <summary>
        /// Deletes this link and recursively deletes all links that refer to it.
        /// </summary>
        public void Delete()
        {
            Source = null;
            Linker = null;
            Target = null;

            while (_firstRefererBySource != null)
                _firstRefererBySource.Delete();
            while (_firstRefererByLinker != null)
                _firstRefererByLinker.Delete();
            while (_firstRefererByTarget != null)
                _firstRefererByTarget.Delete();
        }

        /// <summary>
        /// Returns a string representation of this link.
        /// </summary>
        public override string ToString()
        {
            if (Source != null && Linker != null && Target != null)
                return $"({Source.GetHashCode()}, {Linker.GetHashCode()}, {Target.GetHashCode()})";
            else
                return $"Link#{GetHashCode()}";
        }
    }
}
