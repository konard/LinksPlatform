using System;
using System.IO;
using System.Numerics;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Xunit;


namespace Platform.Data.Doublets.Tests
{
    /// <summary>
    /// AFTER: Testing through ILinks interface
    /// Benefits:
    /// - Tests the interface contract, not the implementation
    /// - Allows testing with different implementations (UnitedMemoryLinks, SplitMemoryLinks, Ffi.Links, etc.)
    /// - Follows the same pattern as GenericLinksTests.cs
    /// - More flexible and maintainable
    /// </summary>
    public static class ILinksBasicTests
    {
        [Fact]
        public static void DeleteAllUsages()
        {
            // Testing through ILinks<TLinkAddress> interface
            Using<uint>(links =>
            {
                var root = links.CreatePoint();

                var a = links.CreatePoint();
                var b = links.CreatePoint();

                links.CreateAndUpdate(a, root);
                links.CreateAndUpdate(b, root);

                Assert.Equal(5U, links.Count());

                links.DeleteAllUsages(root);

                Assert.Equal(3U, links.Count());
            });
        }

        /// <summary>
        /// Helper method that creates an ILinks instance and passes it to the test action
        /// This pattern allows easy swapping of implementations for testing
        /// </summary>
        private static void Using<TLinkAddress>(Action<ILinks<TLinkAddress>> action)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>,
                                 IShiftOperators<TLinkAddress, int, TLinkAddress>,
                                 IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>,
                                 IMinMaxValue<TLinkAddress>,
                                 IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var mem = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<TLinkAddress>(mem);
            action(links); // Pass interface to test action

            // Future: Can easily add tests with other implementations:
            // var splitLinks = new SplitMemoryLinks<TLinkAddress>(...);
            // action(splitLinks);
        }
    }
}
