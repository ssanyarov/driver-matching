using System.Collections.Generic;

namespace DriverMatching;

public class RingGridFinder : IDriverFinder
{
    private readonly int[] _grid; // id or -1
    private readonly Dictionary<int, Point> _byId = new();

    public int N { get; }
    public int M { get; }

    public RingGridFinder(int n, int m)
    {
        if (n <= 0) throw new ArgumentOutOfRangeException(nameof(n));
        if (m <= 0) throw new ArgumentOutOfRangeException(nameof(m));
        long cells = (long)n * m;
        if (cells > int.MaxValue) throw new ArgumentOutOfRangeException(nameof(n), "Grid too big.");

        N = n;
        M = m;

        _grid = new int[n * m];
        Array.Fill(_grid, -1);
    }

    private int Idx(int x, int y) => x * M + y;

    public void Upsert(int id, int x, int y)
    {
        if (id < 0) throw new ArgumentOutOfRangeException(nameof(id));
        Utils.CheckPoint(N, M, x, y);

        int idx = Idx(x, y);
        int occ = _grid[idx];
        if (occ != -1 && occ != id)
            throw new InvalidOperationException("Cell occupied.");

        if (_byId.TryGetValue(id, out var old))
        {
            int oldIdx = Idx(old.X, old.Y);
            if (oldIdx == idx) return;

            _grid[oldIdx] = -1;
            _grid[idx] = id;
            _byId[id] = new Point(x, y);
            return;
        }

        _grid[idx] = id;
        _byId[id] = new Point(x, y);
    }

    public bool Remove(int id)
    {
        if (!_byId.TryGetValue(id, out var p)) return false;
        _byId.Remove(id);
        _grid[Idx(p.X, p.Y)] = -1;
        return true;
    }

    public List<DriverResult> FindNearest(int x, int y, int k = 5)
    {
        Utils.CheckPoint(N, M, x, y);
        if (k <= 0) throw new ArgumentOutOfRangeException(nameof(k));

        var res = new List<DriverResult>(k);

        int maxR = Math.Max(Math.Max(x, N - 1 - x), Math.Max(y, M - 1 - y));

        for (int r = 0; r <= maxR; r++)
        {
            ScanRing(x, y, r, res, k);

            long worst = Utils.WorstDist2(res, k);
            long nextMin = (long)(r + 1) * (r + 1);
            if (res.Count >= k && worst < nextMin)
                break;
        }

        return res;
    }

    private void ScanRing(int cx, int cy, int r, List<DriverResult> res, int k)
    {
        if (r == 0)
        {
            int id0 = _grid[Idx(cx, cy)];
            if (id0 != -1)
                Utils.AddTopK(res, new DriverResult(id0, cx, cy, 0), k);
            return;
        }

        int x0 = cx - r, x1 = cx + r;
        int y0 = cy - r, y1 = cy + r;

        // top and bottom edges
        for (int xx = x0; xx <= x1; xx++)
        {
            if (xx < 0 || xx >= N) continue;

            if (y0 >= 0 && y0 < M) TryCell(xx, y0, cx, cy, res, k);
            if (y1 >= 0 && y1 < M) TryCell(xx, y1, cx, cy, res, k);
        }

        // left and right edges (without corners)
        for (int yy = y0 + 1; yy <= y1 - 1; yy++)
        {
            if (yy < 0 || yy >= M) continue;

            if (x0 >= 0 && x0 < N) TryCell(x0, yy, cx, cy, res, k);
            if (x1 >= 0 && x1 < N) TryCell(x1, yy, cx, cy, res, k);
        }
    }

    private void TryCell(int x, int y, int cx, int cy, List<DriverResult> res, int k)
    {
        int id = _grid[Idx(x, y)];
        if (id == -1) return;

        long d2 = Utils.Dist2(cx, cy, x, y);
        Utils.AddTopK(res, new DriverResult(id, x, y, d2), k);
    }
}
