namespace DriverMatching;

public struct Point
{
    public int X;
    public int Y;

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public class DriverResult
{
    public int Id { get; }
    public int X { get; }
    public int Y { get; }
    public long Dist2 { get; }

    public DriverResult(int id, int x, int y, long dist2)
    {
        Id = id;
        X = x;
        Y = y;
        Dist2 = dist2;
    }
}

public interface IDriverFinder
{
    int N { get; }
    int M { get; }
    void Upsert(int id, int x, int y);
    bool Remove(int id);
    List<DriverResult> FindNearest(int x, int y, int k = 5);
}
