namespace otomasyon.Geometry;

/// <summary>
/// Polilinya kenar sayısı (toplam kenar analizi için).
/// </summary>
public static class PolylineMetrics
{
    public static int CountSegments(int vertexCount, bool closed)
    {
        if (vertexCount < 2)
            return 0;
        return closed ? vertexCount : vertexCount - 1;
    }
}
