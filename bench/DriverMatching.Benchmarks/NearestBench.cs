using BenchmarkDotNet.Attributes;
using DriverMatching;

[MemoryDiagnoser]
public class NearestBench
{
    private const int N = 2000;
    private const int M = 2000;

    [Params(20000, 80000)]
    public int DriverCount;

    private (int x, int y)[] _queries = Array.Empty<(int, int)>();
    private IDriverFinder _brute = null!;
    private IDriverFinder _ring = null!;
    private IDriverFinder _kd = null!;

    [GlobalSetup]
    public void Setup()
    {
        var rnd = new Random(42);

        _brute = new BruteForceFinder(N, M);
        _ring = new RingGridFinder(N, M);
        _kd = new KdTreeFinder(N, M);

        var used = new HashSet<int>(DriverCount);

        for (int id = 0; id < DriverCount; id++)
        {
            int x, y, idx;
            do
            {
                x = rnd.Next(N);
                y = rnd.Next(M);
                idx = x * M + y;
            } while (!used.Add(idx));

            _brute.Upsert(id, x, y);
            _ring.Upsert(id, x, y);
            _kd.Upsert(id, x, y);
        }

        _queries = new (int, int)[1000];
        for (int i = 0; i < _queries.Length; i++)
            _queries[i] = (rnd.Next(N), rnd.Next(M));
    }

    [Benchmark] public int BruteForce() => Run(_brute);
    [Benchmark] public int RingGrid() => Run(_ring);
    [Benchmark] public int KdTree() => Run(_kd);

    private int Run(IDriverFinder f)
    {
        int check = 0;
        foreach (var q in _queries)
        {
            var res = f.FindNearest(q.x, q.y, 5);
            if (res.Count > 0) check ^= res[0].Id;
        }
        return check;
    }
}
