using netDxf.Entities;
using otomasyon.Models;

namespace otomasyon.Analysis;

/// <summary>
/// Sahnedeki en büyük kapalı polyline’dan profil dönüş yönünü tahmin eder.
/// </summary>
public static class ProfileWindingDetector
{
    public static ProfileWinding Detect(DxfScene scene)
    {
        double bestArea = 0;
        ProfileWinding best = ProfileWinding.Unknown;

        foreach (var entity in scene.Entities)
        {
            if (entity is not Polyline2D poly || !poly.IsClosed || poly.Vertexes.Count < 3)
                continue;

            double area = SignedArea(poly);
            double abs = Math.Abs(area);
            if (abs <= bestArea)
                continue;

            bestArea = abs;
            best = area > 0 ? ProfileWinding.CounterClockwise : ProfileWinding.Clockwise;
        }

        return best;
    }

    private static double SignedArea(Polyline2D poly)
    {
        var v = poly.Vertexes;
        int n = v.Count;
        double sum = 0;
        for (int i = 0; i < n; i++)
        {
            var a = v[i].Position;
            var b = v[(i + 1) % n].Position;
            sum += a.X * b.Y - b.X * a.Y;
        }

        return sum * 0.5;
    }
}
