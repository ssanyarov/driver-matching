using System.Collections.Generic;

namespace DriverMatching;

public class KdTreeFinder : IDriverFinder
{
    private readonly Dictionary<int, Point> _byId = new();
    private readonly Dictionary<(int x, int y), int> _byCell = new();

    private Node? _root;
    private bool _dirty = true;

    public int N { get; }
    public int M { get; }

    public KdTreeFinder(int n, int m)
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
            _dirty = true;
            return;
        }

        if (_byCell.ContainsKey(cell))
            throw new InvalidOperationException("Cell occupied.");

        _byId[id] = new Point(x, y);
        _byCell[cell] = id;
        _dirty = true;
    }

    public bool Remove(int id)
    {
        if (!_byId.TryGetValue(id, out var p)) return false;
        _byId.Remove(id);
        _byCell.Remove((p.X, p.Y));
        _dirty = true;
        return true;
    }

    public List<DriverResult> FindNearest(int x, int y, int k = 5)
    {
        Utils.CheckPoint(N, M, x, y);
        if (k <= 0) throw new ArgumentOutOfRangeException(nameof(k));

        if (_dirty)
        {
            var pts = new List<(int id, Point p)>(_byId.Count);
            foreach (var kv in _byId) pts.Add((kv.Key, kv.Value));
            _root = Build(pts, 0);
            _dirty = false;
        }

        var res = new List<DriverResult>(k);
        Search(_root, x, y, k, res);
        return res;
    }

    private class Node
    {
        public int Id;
        public Point P;
        public int Axis; // 0 = X, 1 = Y
        public Node? Left;
        public Node? Right;

        public Node(int id, Point p, int axis)
        {
            Id = id;
            P = p;
            Axis = axis;
        }
    }

    private static Node? Build(List<(int id, Point p)> pts, int depth)
    {
        if (pts.Count == 0) return null;

        int axis = depth % 2;

        pts.Sort((a, b) =>
        {
            if (axis == 0)
            {
                int c = a.p.X.CompareTo(b.p.X);
                return c != 0 ? c : a.p.Y.CompareTo(b.p.Y);
            }
            else
            {
                int c = a.p.Y.CompareTo(b.p.Y);
                return c != 0 ? c : a.p.X.CompareTo(b.p.X);
            }
        });

        int mid = pts.Count / 2;
        var node = new Node(pts[mid].id, pts[mid].p, axis);

        if (mid > 0)
            node.Left = Build(pts.GetRange(0, mid), depth + 1);

        if (mid + 1 < pts.Count)
            node.Right = Build(pts.GetRange(mid + 1, pts.Count - mid - 1), depth + 1);

        return node;
    }

    private static void Search(Node? node, int qx, int qy, int k, List<DriverResult> res)
    {
        if (node == null) return;

        long d2 = Utils.Dist2(qx, qy, node.P.X, node.P.Y);
        Utils.AddTopK(res, new DriverResult(node.Id, node.P.X, node.P.Y, d2), k);

        int axis = node.Axis;
        int qv = axis == 0 ? qx : qy;
        int nv = axis == 0 ? node.P.X : node.P.Y;

        Node? near = qv <= nv ? node.Left : node.Right;
        Node? far = qv <= nv ? node.Right : node.Left;

        Search(near, qx, qy, k, res);

        long worst = Utils.WorstDist2(res, k);
        long planeD2 = (long)(qv - nv) * (qv - nv);
        if (planeD2 <= worst)
            Search(far, qx, qy, k, res);
    }
}
