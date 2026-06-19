using netDxf.Entities;
using otomasyon.Models;
using otomasyon.Settings;

namespace otomasyon.Analysis;

/// <summary>
/// Tek bir kapalı daireyi iki yarım yay (bulge = ±1) olarak kontur segmentlerine çevirir.
/// Başlangıç: en alt nokta; LTR → CCW (sağ yarım), RTL → CW (sol yarım).
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

        double bottomX = cx;
        double bottomY = cy - r;
        double topX = cx;
        double topY = cy + r;

        double bulge = AppSettingsManager.MachineDirection == MachineDirection.LeftToRight ? 1.0 : -1.0;

        segments.Add(new ContourPathOrderer.OrderedSegment
        {
            EdgeIndex = 1,
            CornerIndex = 1,
            StartX = bottomX,
            StartY = bottomY,
            EndX = topX,
            EndY = topY,
            Bulge = bulge
        });

        segments.Add(new ContourPathOrderer.OrderedSegment
        {
            EdgeIndex = 2,
            CornerIndex = 2,
            StartX = topX,
            StartY = topY,
            EndX = bottomX,
            EndY = bottomY,
            Bulge = bulge
        });

        return true;
    }
}
