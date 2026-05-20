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
    /// Paylaşılan köşelerde tekrarlayan etiketleri azaltmak için birleştirir.
    /// </summary>
    public static List<(double X, double Y)> MergeClosePoints(IReadOnlyList<(double X, double Y)> raw, double spanForTolerance)
    {
        double mergeEps = Math.Max(1e-9, spanForTolerance * 1e-10);
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
