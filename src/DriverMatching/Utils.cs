namespace DriverMatching;

internal static class Utils
{
    public static long Dist2(int ax, int ay, int bx, int by)
    {
        long dx = (long)ax - bx;
        long dy = (long)ay - by;
        return dx * dx + dy * dy;
    }

    public static void CheckPoint(int n, int m, int x, int y)
    {
        if (x < 0 || x >= n) throw new ArgumentOutOfRangeException(nameof(x));
        if (y < 0 || y >= m) throw new ArgumentOutOfRangeException(nameof(y));
    }

    // list is always sorted by (Dist2, Id), size <= k
    public static void AddTopK(List<DriverResult> list, DriverResult item, int k)
    {
        int i = 0;
        while (i < list.Count)
        {
            var cur = list[i];
            if (item.Dist2 < cur.Dist2) break;
            if (item.Dist2 == cur.Dist2 && item.Id < cur.Id) break;
            i++;
        }

        list.Insert(i, item);

        if (list.Count > k)
            list.RemoveAt(list.Count - 1);
    }

    public static long WorstDist2(List<DriverResult> list, int k)
    {
        if (list.Count < k) return long.MaxValue;
        return list[list.Count - 1].Dist2;
    }
}
