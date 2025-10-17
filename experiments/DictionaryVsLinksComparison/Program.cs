using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;

namespace DictionaryVsLinksComparison;

class Program
{
    static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<DictionaryVsLinksBenchmark>();
    }
}

[MemoryDiagnoser]
[SimpleJob(warmupCount: 3, iterationCount: 5)]
public class DictionaryVsLinksBenchmark
{
    private Dictionary<long, long>? _dictionary;
    private SimpleLinkStorage? _linkStorage;
    private HybridStorage? _hybrid;

    private const int OperationsCount = 10000;

    [Params(10000)]
    public int N;

    // Dictionary<long, long> Benchmarks

    [Benchmark(Baseline = true)]
    public void Dictionary_Insert()
    {
        _dictionary = new Dictionary<long, long>();
        for (long i = 1; i <= N; i++)
        {
            _dictionary[i] = i * 2;
        }
    }

    [Benchmark]
    public void Dictionary_Read()
    {
        _dictionary = new Dictionary<long, long>();
        for (long i = 1; i <= N; i++)
        {
            _dictionary[i] = i * 2;
        }

        long sum = 0;
        for (long i = 1; i <= N; i++)
        {
            sum += _dictionary[i];
        }
    }

    [Benchmark]
    public void Dictionary_Update()
    {
        _dictionary = new Dictionary<long, long>();
        for (long i = 1; i <= N; i++)
        {
            _dictionary[i] = i * 2;
        }

        for (long i = 1; i <= N; i++)
        {
            _dictionary[i] = i * 3;
        }
    }

    [Benchmark]
    public void Dictionary_Delete()
    {
        _dictionary = new Dictionary<long, long>();
        for (long i = 1; i <= N; i++)
        {
            _dictionary[i] = i * 2;
        }

        for (long i = 1; i <= N; i++)
        {
            _dictionary.Remove(i);
        }
    }

    // Simple in-memory link storage (simulating doublets structure)

    [Benchmark]
    public void LinkStorage_Insert()
    {
        _linkStorage = new SimpleLinkStorage();
        for (ulong i = 1; i <= (ulong)N; i++)
        {
            _linkStorage.Create(i, i * 2);
        }
    }

    [Benchmark]
    public void LinkStorage_Read()
    {
        _linkStorage = new SimpleLinkStorage();
        var linkIds = new ulong[N];
        for (ulong i = 1; i <= (ulong)N; i++)
        {
            linkIds[i - 1] = _linkStorage.Create(i, i * 2);
        }

        ulong sum = 0;
        for (int i = 0; i < N; i++)
        {
            var link = _linkStorage.GetLink(linkIds[i]);
            sum += link.Source + link.Target;
        }
    }

    [Benchmark]
    public void LinkStorage_Update()
    {
        _linkStorage = new SimpleLinkStorage();
        var linkIds = new ulong[N];
        for (ulong i = 1; i <= (ulong)N; i++)
        {
            linkIds[i - 1] = _linkStorage.Create(i, i * 2);
        }

        for (int i = 0; i < N; i++)
        {
            var link = _linkStorage.GetLink(linkIds[i]);
            _linkStorage.Update(linkIds[i], link.Source, link.Target + 1);
        }
    }

    [Benchmark]
    public void LinkStorage_Delete()
    {
        _linkStorage = new SimpleLinkStorage();
        var linkIds = new ulong[N];
        for (ulong i = 1; i <= (ulong)N; i++)
        {
            linkIds[i - 1] = _linkStorage.Create(i, i * 2);
        }

        for (int i = 0; i < N; i++)
        {
            _linkStorage.Delete(linkIds[i]);
        }
    }

    // Hybrid Approach - Dictionary for lookups, Links for storage

    [Benchmark]
    public void Hybrid_Insert()
    {
        _hybrid = new HybridStorage();
        for (long i = 1; i <= N; i++)
        {
            _hybrid.Set(i, i * 2);
        }
    }

    [Benchmark]
    public void Hybrid_Read()
    {
        _hybrid = new HybridStorage();
        for (long i = 1; i <= N; i++)
        {
            _hybrid.Set(i, i * 2);
        }

        long sum = 0;
        for (long i = 1; i <= N; i++)
        {
            sum += _hybrid.Get(i);
        }
    }

    [Benchmark]
    public void Hybrid_Update()
    {
        _hybrid = new HybridStorage();
        for (long i = 1; i <= N; i++)
        {
            _hybrid.Set(i, i * 2);
        }

        for (long i = 1; i <= N; i++)
        {
            _hybrid.Set(i, i * 3);
        }
    }

    [Benchmark]
    public void Hybrid_Delete()
    {
        _hybrid = new HybridStorage();
        for (long i = 1; i <= N; i++)
        {
            _hybrid.Set(i, i * 2);
        }

        for (long i = 1; i <= N; i++)
        {
            _hybrid.Remove(i);
        }
    }
}

/// <summary>
/// Simple in-memory link storage simulating the Doublets structure
/// Each link has: Index (ID), Source, Target
/// </summary>
public class SimpleLinkStorage
{
    private readonly Dictionary<ulong, Link> _links;
    private ulong _nextId;

    public SimpleLinkStorage()
    {
        _links = new Dictionary<ulong, Link>();
        _nextId = 1;
    }

    public ulong Create(ulong source, ulong target)
    {
        var id = _nextId++;
        _links[id] = new Link(id, source, target);
        return id;
    }

    public Link GetLink(ulong id)
    {
        return _links[id];
    }

    public void Update(ulong id, ulong source, ulong target)
    {
        _links[id] = new Link(id, source, target);
    }

    public void Delete(ulong id)
    {
        _links.Remove(id);
    }

    public struct Link
    {
        public ulong Index { get; }
        public ulong Source { get; }
        public ulong Target { get; }

        public Link(ulong index, ulong source, ulong target)
        {
            Index = index;
            Source = source;
            Target = target;
        }
    }
}

/// <summary>
/// Hybrid storage that uses Dictionary for fast lookups and SimpleLinkStorage for actual storage
/// This demonstrates combining the benefits of both approaches
/// </summary>
public class HybridStorage
{
    private readonly Dictionary<long, ulong> _keyToLinkIndex;
    private readonly SimpleLinkStorage _linkStorage;

    public HybridStorage()
    {
        _keyToLinkIndex = new Dictionary<long, ulong>();
        _linkStorage = new SimpleLinkStorage();
    }

    public void Set(long key, long value)
    {
        if (_keyToLinkIndex.TryGetValue(key, out var linkIndex))
        {
            // Update existing link
            _linkStorage.Update(linkIndex, (ulong)key, (ulong)value);
        }
        else
        {
            // Create new link
            var newLink = _linkStorage.Create((ulong)key, (ulong)value);
            _keyToLinkIndex[key] = newLink;
        }
    }

    public long Get(long key)
    {
        if (_keyToLinkIndex.TryGetValue(key, out var linkIndex))
        {
            var link = _linkStorage.GetLink(linkIndex);
            return (long)link.Target;
        }
        throw new KeyNotFoundException($"Key {key} not found");
    }

    public void Remove(long key)
    {
        if (_keyToLinkIndex.TryGetValue(key, out var linkIndex))
        {
            _linkStorage.Delete(linkIndex);
            _keyToLinkIndex.Remove(key);
        }
    }
}
