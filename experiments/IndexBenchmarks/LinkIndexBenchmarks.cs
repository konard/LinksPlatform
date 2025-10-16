using System;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace IndexBenchmarks
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class LinkIndexBenchmarks
    {
        private const int OperationsCount = 1000;
        private const int SearchCount = 100;

        private ILinksIndex<uint>[] _indexes;
        private (uint linkAddress, uint source, uint target)[] _testData;
        private (uint source, uint target)[] _searchData;

        [GlobalSetup]
        public void Setup()
        {
            // Initialize all index implementations
            _indexes = new ILinksIndex<uint>[]
            {
                new HashtableIndex<uint>(),
                new LinkedListIndex<uint>(),
                new AvlTreeIndex<uint>(),
                new SkipListIndex<uint>(),
                new BTreeIndex<uint>(),
                new MatrixIndex<uint>(),
                new BitIndexIndex<uint>()
            };

            // Generate test data
            var random = new Random(42); // Fixed seed for reproducibility
            _testData = new (uint, uint, uint)[OperationsCount];
            for (int i = 0; i < OperationsCount; i++)
            {
                _testData[i] = (
                    (uint)i,
                    (uint)random.Next(0, OperationsCount / 10),
                    (uint)random.Next(0, OperationsCount / 10)
                );
            }

            _searchData = new (uint, uint)[SearchCount];
            for (int i = 0; i < SearchCount; i++)
            {
                _searchData[i] = (
                    (uint)random.Next(0, OperationsCount / 10),
                    (uint)random.Next(0, OperationsCount / 10)
                );
            }
        }

        [Benchmark]
        public void Hashtable_Add()
        {
            var index = new HashtableIndex<uint>();
            foreach (var (linkAddress, source, target) in _testData)
                index.Add(linkAddress, source, target);
        }

        [Benchmark]
        public void LinkedList_Add()
        {
            var index = new LinkedListIndex<uint>();
            foreach (var (linkAddress, source, target) in _testData)
                index.Add(linkAddress, source, target);
        }

        [Benchmark]
        public void AvlTree_Add()
        {
            var index = new AvlTreeIndex<uint>();
            foreach (var (linkAddress, source, target) in _testData)
                index.Add(linkAddress, source, target);
        }

        [Benchmark]
        public void SkipList_Add()
        {
            var index = new SkipListIndex<uint>();
            foreach (var (linkAddress, source, target) in _testData)
                index.Add(linkAddress, source, target);
        }

        [Benchmark]
        public void BTree_Add()
        {
            var index = new BTreeIndex<uint>();
            foreach (var (linkAddress, source, target) in _testData)
                index.Add(linkAddress, source, target);
        }

        [Benchmark]
        public void Matrix_Add()
        {
            var index = new MatrixIndex<uint>();
            foreach (var (linkAddress, source, target) in _testData)
                index.Add(linkAddress, source, target);
        }

        [Benchmark]
        public void BitIndex_Add()
        {
            var index = new BitIndexIndex<uint>();
            foreach (var (linkAddress, source, target) in _testData)
                index.Add(linkAddress, source, target);
        }

        [Benchmark]
        public void Hashtable_Search()
        {
            var index = new HashtableIndex<uint>();
            foreach (var (linkAddress, source, target) in _testData)
                index.Add(linkAddress, source, target);

            uint sum = 0;
            foreach (var (source, target) in _searchData)
                sum += index.Search(source, target);
        }

        [Benchmark]
        public void LinkedList_Search()
        {
            var index = new LinkedListIndex<uint>();
            foreach (var (linkAddress, source, target) in _testData)
                index.Add(linkAddress, source, target);

            uint sum = 0;
            foreach (var (source, target) in _searchData)
                sum += index.Search(source, target);
        }

        [Benchmark]
        public void AvlTree_Search()
        {
            var index = new AvlTreeIndex<uint>();
            foreach (var (linkAddress, source, target) in _testData)
                index.Add(linkAddress, source, target);

            uint sum = 0;
            foreach (var (source, target) in _searchData)
                sum += index.Search(source, target);
        }

        [Benchmark]
        public void SkipList_Search()
        {
            var index = new SkipListIndex<uint>();
            foreach (var (linkAddress, source, target) in _testData)
                index.Add(linkAddress, source, target);

            uint sum = 0;
            foreach (var (source, target) in _searchData)
                sum += index.Search(source, target);
        }

        [Benchmark]
        public void BTree_Search()
        {
            var index = new BTreeIndex<uint>();
            foreach (var (linkAddress, source, target) in _testData)
                index.Add(linkAddress, source, target);

            uint sum = 0;
            foreach (var (source, target) in _searchData)
                sum += index.Search(source, target);
        }

        [Benchmark]
        public void Matrix_Search()
        {
            var index = new MatrixIndex<uint>();
            foreach (var (linkAddress, source, target) in _testData)
                index.Add(linkAddress, source, target);

            uint sum = 0;
            foreach (var (source, target) in _searchData)
                sum += index.Search(source, target);
        }

        [Benchmark]
        public void BitIndex_Search()
        {
            var index = new BitIndexIndex<uint>();
            foreach (var (linkAddress, source, target) in _testData)
                index.Add(linkAddress, source, target);

            uint sum = 0;
            foreach (var (source, target) in _searchData)
                sum += index.Search(source, target);
        }

        [Benchmark]
        public void Hashtable_Remove()
        {
            var index = new HashtableIndex<uint>();
            foreach (var (linkAddress, source, target) in _testData)
                index.Add(linkAddress, source, target);

            foreach (var (linkAddress, source, target) in _testData)
                index.Remove(linkAddress, source, target);
        }

        [Benchmark]
        public void LinkedList_Remove()
        {
            var index = new LinkedListIndex<uint>();
            foreach (var (linkAddress, source, target) in _testData)
                index.Add(linkAddress, source, target);

            foreach (var (linkAddress, source, target) in _testData)
                index.Remove(linkAddress, source, target);
        }

        [Benchmark]
        public void AvlTree_Remove()
        {
            var index = new AvlTreeIndex<uint>();
            foreach (var (linkAddress, source, target) in _testData)
                index.Add(linkAddress, source, target);

            foreach (var (linkAddress, source, target) in _testData)
                index.Remove(linkAddress, source, target);
        }

        [Benchmark]
        public void SkipList_Remove()
        {
            var index = new SkipListIndex<uint>();
            foreach (var (linkAddress, source, target) in _testData)
                index.Add(linkAddress, source, target);

            foreach (var (linkAddress, source, target) in _testData)
                index.Remove(linkAddress, source, target);
        }

        [Benchmark]
        public void BTree_Remove()
        {
            var index = new BTreeIndex<uint>();
            foreach (var (linkAddress, source, target) in _testData)
                index.Add(linkAddress, source, target);

            foreach (var (linkAddress, source, target) in _testData)
                index.Remove(linkAddress, source, target);
        }

        [Benchmark]
        public void Matrix_Remove()
        {
            var index = new MatrixIndex<uint>();
            foreach (var (linkAddress, source, target) in _testData)
                index.Add(linkAddress, source, target);

            foreach (var (linkAddress, source, target) in _testData)
                index.Remove(linkAddress, source, target);
        }

        [Benchmark]
        public void BitIndex_Remove()
        {
            var index = new BitIndexIndex<uint>();
            foreach (var (linkAddress, source, target) in _testData)
                index.Add(linkAddress, source, target);

            foreach (var (linkAddress, source, target) in _testData)
                index.Remove(linkAddress, source, target);
        }
    }
}
