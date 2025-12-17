using System.Collections.Generic;

namespace DriverMatching;

public class BruteForceFinder : IDriverFinder
{
    private readonly Dictionary<int, Point> _byId = new();
    private readonly Dictionary<(int x, int y), int> _byCell = new();

    public int N { get; }
    public int M { get; }

    public BruteForceFinder(int n, int m)
    {
        if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n));
        if (m <= 0) throw new ArgumentOutOfRangeException(nameof(m));
        N = n;
        M = m;
    }

    public void Upsert(int id, int x, int y)
    {
        if (id < 0) throw new ArgumentOutOfRangeException(nameof(id));
        Utils.CheckPoint(N, M, x, y);

        var cell = (x, y);

        if (_byId.TryGetValue(id, out var old))
        {
            var oldCell = (old.X, old.Y);
            if (oldCell == cell) return;

            if (_byCell.TryGetValue(cell, out var other) && other != id)
                throw new InvalidOperationException("Cell occupied.");

            _byCell.Remove(oldCell);
            _byCell[cell] = id;
            _byId[id] = new Point(x, y);
            return;
        }

        if (_byCell.ContainsKey(cell))
            throw new InvalidOperationException("Cell occupied.");

        _byId[id] = new Point(x, y);
        _byCell[cell] = id;
    }

    public bool Remove(int id)
    {
        if (!_byId.TryGetValue(id, out var p)) return false;
        _byId.Remove(id);
        _byCell.Remove((p.X, p.Y));
        return true;
    }

    public List<DriverResult> FindNearest(int x, int y, int k = 5)
    {
        Utils.CheckPoint(N, M, x, y);
        if (k <= 0) throw new ArgumentOutOfRangeException(nameof(k));

        var res = new List<DriverResult>(k);

        foreach (var kv in _byId)
        {
            var id = kv.Key;
            var p = kv.Value;
            long d2 = Utils.Dist2(x, y, p.X, p.Y);
            Utils.AddTopK(res, new DriverResult(id, p.X, p.Y, d2), k);
        }

        return res;
    }
}
