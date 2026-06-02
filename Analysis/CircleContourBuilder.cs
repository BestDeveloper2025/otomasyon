using netDxf.Entities;
using otomasyon.Models;

namespace otomasyon.Analysis;

/// <summary>
/// Tek bir kapalı daireyi iki yarım yay (bulge = +1) olarak kontur segmentlerine çevirir.
/// </summary>
public static class CircleContourBuilder
{
    public static bool TryBuild(DxfScene scene, out List<ContourPathOrderer.OrderedSegment> segments)
    {
        segments = new List<ContourPathOrderer.OrderedSegment>();

        Circle? best = null;
        foreach (var entity in scene.Entities)
        {
            if (entity is not Circle c || c.Radius <= 1e-9)
                continue;

            if (best is null || c.Radius > best.Radius)
                best = c;
        }

        if (best is null)
            return false;

        double cx = best.Center.X;
        double cy = best.Center.Y;
        double r = best.Radius;

        double rightX = cx + r, rightY = cy;
        double leftX = cx - r, leftY = cy;

        // CCW: sağ uçtan üst yarım, sol uçtan alt yarım (bulge +1 = CCW yarım daire).
        segments.Add(new ContourPathOrderer.OrderedSegment
        {
            EdgeIndex = 1,
            CornerIndex = 1,
            StartX = rightX,
            StartY = rightY,
            EndX = leftX,
            EndY = leftY,
            Bulge = 1.0
        });

        segments.Add(new ContourPathOrderer.OrderedSegment
        {
            EdgeIndex = 2,
            CornerIndex = 2,
            StartX = leftX,
            StartY = leftY,
            EndX = rightX,
            EndY = rightY,
            Bulge = 1.0
        });

        return true;
    }
}
