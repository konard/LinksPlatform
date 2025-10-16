using System;
using System.Collections.Generic;

namespace Platform.Data.VirtualLinks
{
    /// <summary>
    /// Manages virtual link address ranges and resolution.
    ///
    /// This class implements the concept of "reserved addresses" for virtual links,
    /// where each real link has a reserved range of virtual addresses that automatically
    /// trigger computation when accessed.
    ///
    /// For example, if a link has address N, virtual links for that link might occupy
    /// addresses in a reserved range like [VirtualBase + N*VirtualCount, VirtualBase + N*VirtualCount + VirtualCount-1].
    /// </summary>
    /// <typeparam name="TLink">The link address type.</typeparam>
    public class VirtualLinkAddressSpace<TLink> where TLink : struct, IComparable<TLink>
    {
        private readonly VirtualLinkFactory<TLink> _factory;
        private readonly TLink _virtualBase;
        private readonly int _virtualLinksPerRealLink;
        private readonly Dictionary<TLink, VirtualLinkType> _addressToTypeMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="VirtualLinkAddressSpace{TLink}"/> class.
        /// </summary>
        /// <param name="factory">The virtual link factory.</param>
        /// <param name="virtualBase">The base address for the virtual range.</param>
        /// <param name="virtualLinksPerRealLink">Number of virtual links reserved per real link.</param>
        public VirtualLinkAddressSpace(
            VirtualLinkFactory<TLink> factory,
            TLink virtualBase,
            int virtualLinksPerRealLink = 16)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _virtualBase = virtualBase;
            _virtualLinksPerRealLink = virtualLinksPerRealLink;
            _addressToTypeMap = new Dictionary<TLink, VirtualLinkType>();

            InitializeAddressMapping();
        }

        /// <summary>
        /// Gets the base address for virtual links.
        /// </summary>
        public TLink VirtualBase => _virtualBase;

        /// <summary>
        /// Gets the number of virtual links reserved per real link.
        /// </summary>
        public int VirtualLinksPerRealLink => _virtualLinksPerRealLink;

        /// <summary>
        /// Initializes the mapping from offset to virtual link type.
        /// </summary>
        private void InitializeAddressMapping()
        {
            // Map offsets to virtual link types
            // For each real link at address N, virtual links are at:
            // VirtualBase + N*VirtualLinksPerRealLink + offset
            // where offset determines the type
            var offset = 0;
            foreach (VirtualLinkType type in Enum.GetValues(typeof(VirtualLinkType)))
            {
                if (offset < _virtualLinksPerRealLink)
                {
                    _addressToTypeMap[(TLink)Convert.ChangeType(offset, typeof(TLink))] = type;
                    offset++;
                }
            }
        }

        /// <summary>
        /// Checks if an address is in the virtual address range.
        /// </summary>
        /// <param name="address">The address to check.</param>
        /// <returns>True if the address is virtual, false otherwise.</returns>
        public bool IsVirtualAddress(TLink address)
        {
            return address.CompareTo(_virtualBase) >= 0;
        }

        /// <summary>
        /// Resolves a virtual address to its corresponding real link and virtual link type.
        /// </summary>
        /// <param name="virtualAddress">The virtual address.</param>
        /// <returns>A tuple of (real link address, virtual link type), or null if not a valid virtual address.</returns>
        public (TLink realLink, VirtualLinkType type)? ResolveVirtualAddress(TLink virtualAddress)
        {
            if (!IsVirtualAddress(virtualAddress))
            {
                return null;
            }

            // Calculate offset from virtual base
            var offset = Subtract(virtualAddress, _virtualBase);
            var offsetValue = ToInt64(offset);

            // Calculate real link address and type offset
            var realLinkIndex = offsetValue / _virtualLinksPerRealLink;
            var typeOffset = offsetValue % _virtualLinksPerRealLink;

            var realLink = (TLink)Convert.ChangeType(realLinkIndex, typeof(TLink));
            var typeOffsetKey = (TLink)Convert.ChangeType(typeOffset, typeof(TLink));

            if (_addressToTypeMap.TryGetValue(typeOffsetKey, out var type))
            {
                return (realLink, type);
            }

            return null;
        }

        /// <summary>
        /// Gets the virtual address for a specific virtual link.
        /// </summary>
        /// <param name="realLink">The real link address.</param>
        /// <param name="type">The virtual link type.</param>
        /// <returns>The virtual address.</returns>
        public TLink GetVirtualAddress(TLink realLink, VirtualLinkType type)
        {
            var typeOffset = (int)type;
            if (typeOffset >= _virtualLinksPerRealLink)
            {
                throw new ArgumentException($"Virtual link type {type} exceeds reserved space.", nameof(type));
            }

            var realLinkValue = ToInt64(realLink);
            var virtualOffset = realLinkValue * _virtualLinksPerRealLink + typeOffset;

            return Add(_virtualBase, (TLink)Convert.ChangeType(virtualOffset, typeof(TLink)));
        }

        /// <summary>
        /// Creates a virtual link for the given address if it's a virtual address.
        /// </summary>
        /// <param name="address">The address to resolve.</param>
        /// <returns>A virtual link if the address is virtual, null otherwise.</returns>
        public IVirtualLink<TLink> CreateFromAddress(TLink address)
        {
            var resolved = ResolveVirtualAddress(address);
            if (!resolved.HasValue)
            {
                return null;
            }

            var (realLink, type) = resolved.Value;
            return _factory.CreateVirtualLink(realLink, type);
        }

        private static TLink Add(TLink a, TLink b)
        {
            dynamic da = a;
            dynamic db = b;
            return (TLink)(da + db);
        }

        private static TLink Subtract(TLink a, TLink b)
        {
            dynamic da = a;
            dynamic db = b;
            return (TLink)(da - db);
        }

        private static long ToInt64(TLink value)
        {
            return Convert.ToInt64(value);
        }
    }
}
