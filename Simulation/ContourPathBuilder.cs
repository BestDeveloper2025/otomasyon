using otomasyon.Analysis;
using otomasyon.Geometry;
using otomasyon.Models;
using otomasyon.Models.Simulation;

namespace otomasyon.Simulation;

/// <summary>DXF sahnesinden CCW kontur yolu üretir.</summary>
public static class ContourPathBuilder
{
    public static bool TryBuild(DxfScene scene, out ContourPath path)
    {
        path = ContourPath.Empty;
        if (!ContourPathOrderer.TryBuildOrderedSegments(scene, out var ordered) || ordered.Count == 0)
            return false;

        var segments = new List<ContourPathSegment>(ordered.Count);
        double total = 0;

        foreach (var seg in ordered)
        {
            double? cx = null, cy = null, r = null, sa = null, ea = null;
            if (seg.IsArc && BulgeArcConverter.TryFromBulge(
                    seg.StartX, seg.StartY, seg.EndX, seg.EndY, seg.Bulge,
                    out double centerX, out double centerY, out double radius,
                    out double startAng, out double endAng))
            {
                cx = centerX;
                cy = centerY;
                r = radius;
                sa = startAng;
                ea = endAng;
            }

            var draft = new ContourPathSegment
            {
                EdgeIndex = seg.EdgeIndex,
                CornerIndex = seg.CornerIndex,
                IsArc = seg.IsArc,
                Bulge = seg.Bulge,
                StartX = seg.StartX,
                StartY = seg.StartY,
                EndX = seg.EndX,
                EndY = seg.EndY,
                CenterX = cx,
                CenterY = cy,
                Radius = r,
                StartAngleDeg = sa,
                EndAngleDeg = ea
            };

            double len = ContourSegmentLength.ComputeMm(draft);
            segments.Add(new ContourPathSegment
            {
                EdgeIndex = draft.EdgeIndex,
                CornerIndex = draft.CornerIndex,
                IsArc = draft.IsArc,
                Bulge = draft.Bulge,
                StartX = draft.StartX,
                StartY = draft.StartY,
                EndX = draft.EndX,
                EndY = draft.EndY,
                LengthMm = len,
                CenterX = cx,
                CenterY = cy,
                Radius = r,
                StartAngleDeg = sa,
                EndAngleDeg = ea
            });

            total += len;
        }

        path = new ContourPath(segments, total);
        return true;
    }
}
