using netDxf.Entities;

namespace otomasyon.Geometry;

/// <summary>
/// Etiket ve metin listesi için dünya XY köşe / uç noktalarını toplar.
/// </summary>
public static class WorldCornerCollector
{
    public static void Collect(EntityObject entity, ICollection<(double X, double Y)> dest)
    {
        switch (entity)
        {
            case Line line:
                dest.Add((line.StartPoint.X, line.StartPoint.Y));
                dest.Add((line.EndPoint.X, line.EndPoint.Y));
                break;
            case Polyline2D p2:
                foreach (var vx in p2.Vertexes)
                    dest.Add((vx.Position.X, vx.Position.Y));
                break;
            case Polyline3D p3:
                foreach (var v in p3.Vertexes)
                    dest.Add((v.X, v.Y));
                break;
            case Arc arc:
                ArcSampler.GetArcEndpointXY(arc, out double sx, out double sy, out double ex, out double ey);
                dest.Add((sx, sy));
                dest.Add((ex, ey));
                break;
            case Circle c:
                double cx = c.Center.X, cy = c.Center.Y, r = c.Radius;
                dest.Add((cx - r, cy));
                dest.Add((cx + r, cy));
                dest.Add((cx, cy + r));
                dest.Add((cx, cy - r));
                break;
        }
    }

    /// <summary>
    /// Köşe etiketleri için: tepe noktaları + polyline explode (yay uçları dahil).
    /// </summary>
    public static void CollectForDisplay(EntityObject entity, ICollection<(double X, double Y)> dest)
    {
        Collect(entity, dest);

        if (entity is Polyline2D poly)
            CollectPolyline2DExploded(poly, dest);
    }

    private static void CollectPolyline2DExploded(Polyline2D poly, ICollection<(double X, double Y)> dest)
    {
        try
        {
            foreach (var fragment in poly.Explode())
            {
                switch (fragment)
                {
                    case Line ln:
                        dest.Add((ln.StartPoint.X, ln.StartPoint.Y));
                        dest.Add((ln.EndPoint.X, ln.EndPoint.Y));
                        break;
                    case Arc arc:
                        ArcSampler.GetArcEndpointXY(arc, out double sx, out double sy, out double ex, out double ey);
                        dest.Add((sx, sy));
                        dest.Add((ex, ey));
                        break;
                }
            }
        }
        catch
        {
            // Explode başarısızsa tepe noktaları yeterli.
        }
    }

    /// <summary>
    /// Paylaşılan köşelerde tekrarlayan etiketleri azaltmak için birleştirir.
    /// </summary>
    public static List<(double X, double Y)> MergeClosePoints(IReadOnlyList<(double X, double Y)> raw, double spanForTolerance)
        => MergeClosePoints(raw, spanForTolerance, forDisplayLabels: false);

    /// <summary>
    /// Çizim etiketleri için birleştirme; büyük parçalarda köşelerin yanlışlıkla tek noktada toplanmasını önler.
    /// </summary>
    public static List<(double X, double Y)> MergeClosePointsForDisplay(
        IReadOnlyList<(double X, double Y)> raw,
        double spanForTolerance)
        => MergeClosePoints(raw, spanForTolerance, forDisplayLabels: true);

    private static List<(double X, double Y)> MergeClosePoints(
        IReadOnlyList<(double X, double Y)> raw,
        double spanForTolerance,
        bool forDisplayLabels)
    {
        double mergeEps = forDisplayLabels
            ? Math.Max(1e-9, Math.Min(1e-3, spanForTolerance * 1e-11))
            : Math.Max(1e-9, spanForTolerance * 1e-10);
        var unique = new List<(double X, double Y)>(raw.Count);
        foreach (var p in raw)
        {
            bool dup = false;
            foreach (var u in unique)
            {
                if (Math.Abs(u.X - p.X) <= mergeEps && Math.Abs(u.Y - p.Y) <= mergeEps)
                {
                    dup = true;
                    break;
                }
            }

            if (!dup)
                unique.Add(p);
        }

        return unique;
    }
}
