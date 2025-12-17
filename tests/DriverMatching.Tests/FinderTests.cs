using NUnit.Framework;
using DriverMatching;

namespace DriverMatching.Tests;

public class FinderTests
{
    [Test]
    public void Simple_case()
    {
        var finders = new IDriverFinder[]
        {
            new BruteForceFinder(20, 20),
            new RingGridFinder(20, 20),
            new KdTreeFinder(20, 20)
        };

        foreach (var f in finders)
        {
            f.Upsert(1, 10, 10);
            f.Upsert(2, 11, 10);
            f.Upsert(3, 0, 0);

            var res = f.FindNearest(10, 10, 2);
            Assert.That(res.Count, Is.EqualTo(2));
            Assert.That(res[0].Id, Is.EqualTo(1));
            Assert.That(res[1].Id, Is.EqualTo(2));
        }
    }

    [Test]
    public void Random_same_as_bruteforce()
    {
        const int N = 60;
        const int M = 50;

        var brute = new BruteForceFinder(N, M);
        var ring = new RingGridFinder(N, M);
        var kd = new KdTreeFinder(N, M);

        var rnd = new Random(123);
        var used = new HashSet<int>();

        for (int id = 0; id < 500; id++)
        {
            int x, y, idx;
            do
            {
                x = rnd.Next(N);
                y = rnd.Next(M);
                idx = x * M + y;
            } while (!used.Add(idx));

            brute.Upsert(id, x, y);
            ring.Upsert(id, x, y);
            kd.Upsert(id, x, y);
        }

        for (int t = 0; t < 200; t++)
        {
            int qx = rnd.Next(N);
            int qy = rnd.Next(M);

            var a = brute.FindNearest(qx, qy, 5);
            var b = ring.FindNearest(qx, qy, 5);
            var c = kd.FindNearest(qx, qy, 5);

            Assert.That(b.Select(z => (z.Id, z.Dist2)), Is.EqualTo(a.Select(z => (z.Id, z.Dist2))));
            Assert.That(c.Select(z => (z.Id, z.Dist2)), Is.EqualTo(a.Select(z => (z.Id, z.Dist2))));
        }
    }
}
