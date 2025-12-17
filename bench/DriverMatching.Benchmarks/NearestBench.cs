using BenchmarkDotNet.Attributes;
using DriverMatching;

[MemoryDiagnoser]
public class NearestBench
{
    private const int N = 2000;
    private const int M = 2000;

    // Количество водителей в системе (разные размеры для сравнения)
    [Params(5_000, 20_000)]
    public int DriverCount;

    // Сколько запросов "найти ближайших" выполняем в одном прогоне
    [Params(200)]
    public int QueryCount;

    private (int x, int y)[] _queries = null!;
    private BruteForceFinder _brute = null!;
    private RingGridFinder _ring = null!;
    private KdTreeFinder _kd = null!;

    [GlobalSetup]
    public void Setup()
    {
        var rnd = new Random(42);

        _brute = new BruteForceFinder(N, M);
        _ring  = new RingGridFinder(N, M);
        _kd    = new KdTreeFinder(N, M);

        // гарантируем уникальные клетки
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

        _queries = new (int x, int y)[QueryCount];
        for (int i = 0; i < QueryCount; i++)
            _queries[i] = (rnd.Next(N), rnd.Next(M));
    }

    [Benchmark] public int BruteForce() => Run(_brute);
    [Benchmark] public int RingGrid()   => Run(_ring);
    [Benchmark] public int KdTree()     => Run(_kd);

    private int Run(IDriverFinder finder)
    {
        int checksum = 0;
        foreach (var (x, y) in _queries)
        {
            var res = finder.FindNearest(x, y, 5);
            if (res.Count > 0) checksum ^= res[0].Id;
        }
        return checksum;
    }
}
