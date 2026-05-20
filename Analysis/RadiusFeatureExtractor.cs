using netDxf.Entities;
using otomasyon.Geometry;
using otomasyon.Models;

namespace otomasyon.Analysis;

/// <summary>
/// Radiuslu kenarları (0,0) → sağa → CCW sırayla çıkarır; kenar listesi üretir.
/// </summary>
public sealed class RadiusFeatureExtractor
{
    public RadiusAnalysisResult Extract(DxfScene scene)
    {
        var radii = new List<RadiusFeature>();
        var edges = new List<ContourEdge>();

        if (ContourPathOrderer.TryBuildOrderedSegments(scene, out var segments))
        {
            int radiusNum = 0;
            for (int si = 0; si < segments.Count; si++)
            {
                var seg = segments[si];
                int? radiusIndex = null;
                if (seg.IsArc)
                {
                    radiusNum++;
                    var feature = BuildFromSegment(scene, segments, si, radiusNum);
                    if (feature is not null)
                    {
                        radii.Add(feature);
                        radiusIndex = radiusNum;
                    }
                }

                edges.Add(new ContourEdge
                {
                    Index = seg.EdgeIndex,
                    CornerIndex = seg.CornerIndex,
                    StartX = seg.StartX,
                    StartY = seg.StartY,
                    EndX = seg.EndX,
                    EndY = seg.EndY,
                    IsRadiusSegment = seg.IsArc,
                    RadiusIndex = radiusIndex
                });
            }

            return RadiusTraversalOrderer.Apply(FinalizeResult(radii, edges));
        }

        ExtractFallbackEntityOrder(scene, radii, edges);
        return RadiusTraversalOrderer.Apply(FinalizeResult(radii, edges));
    }

    private static RadiusAnalysisResult FinalizeResult(List<RadiusFeature> radii, List<ContourEdge> edges)
    {
        for (int i = 0; i < radii.Count; i++)
            radii[i] = RadiusEndpointOrienter.Orient(radii[i]);

        return new RadiusAnalysisResult(radii, edges);
    }

    private static RadiusFeature? BuildFromSegment(
        DxfScene scene,
        List<ContourPathOrderer.OrderedSegment> segments,
        int index,
        int radiusIndex)
    {
        var seg = segments[index];
        if (!BulgeArcConverter.TryFromBulge(
                seg.StartX, seg.StartY, seg.EndX, seg.EndY, seg.Bulge,
                out double cx, out double cy, out double r,
                out double startAng, out double endAng))
            return null;

        bool ccwAlongPath = seg.Bulge > 0;
        double tanAtStart = AngleMath.ArcTangentDeg(startAng, ccwAlongPath);
        double tanAtEnd = AngleMath.ArcTangentDeg(endAng, ccwAlongPath);

        double line1Dir = TryAdjacentLineDirection(segments, index - 1, forward: true);
        if (double.IsNaN(line1Dir))
            line1Dir = OppositeDirection(tanAtStart);

        double line2Dir = TryAdjacentLineDirection(segments, index + 1, forward: true);
        if (double.IsNaN(line2Dir))
            line2Dir = tanAtEnd;

        double mcx = (scene.Bounds.MinX + scene.Bounds.MaxX) * 0.5;
        double mcy = (scene.Bounds.MinY + scene.Bounds.MaxY) * 0.5;
        var angles = RadiusCornerAngles.Compute(
            line1Dir, line2Dir, tanAtStart, tanAtEnd,
            seg.StartX, seg.StartY, seg.EndX, seg.EndY, mcx, mcy);

        return new RadiusFeature
        {
            Index = radiusIndex,
            SourceLabel = $"Kenar {seg.EdgeIndex}",
            CenterX = cx,
            CenterY = cy,
            Radius = r,
            Convexity = RadiusConvexityClassifier.ClassifyForCcwTraversal(seg.Bulge),
            StartX = seg.StartX,
            StartY = seg.StartY,
            EndX = seg.EndX,
            EndY = seg.EndY,
            Line1DirectionDeg = line1Dir,
            Line2DirectionDeg = line2Dir,
            CornerAngleDeg = angles.VirtualCornerAngleDeg,
            StartEdgeAngleDeg = angles.StartEdgeAngleDeg,
            EndEdgeAngleDeg = angles.EndEdgeAngleDeg,
            StartCornerAngleDeg = angles.StartCornerAngleDeg,
            EndCornerAngleDeg = angles.EndCornerAngleDeg,
            StartTangentAngleDeg = angles.StartTangentAngleDeg,
            EndTangentAngleDeg = angles.EndTangentAngleDeg,
            Bulge = seg.Bulge,
            EdgeIndex = seg.EdgeIndex,
            CornerIndex = seg.CornerIndex
        };
    }

    private static void ExtractFallbackEntityOrder(
        DxfScene scene,
        List<RadiusFeature> radii,
        List<ContourEdge> edges)
    {
        int edgeNum = 0;
        int radiusNum = 0;

        for (int ei = 0; ei < scene.Entities.Count; ei++)
        {
            var entity = scene.Entities[ei];
            switch (entity)
            {
                case Polyline2D poly when poly.IsClosed:
                    ExtractPolylineFallback(scene, poly, ref edgeNum, ref radiusNum, radii, edges);
                    break;
                case Arc arc:
                    if (TryAddStandaloneArc(scene, arc, ref radiusNum, radii))
                    {
                        ArcSampler.GetArcEndpointXY(arc, out double sx, out double sy, out double ex, out double ey);
                        edges.Add(new ContourEdge
                        {
                            Index = ++edgeNum,
                            CornerIndex = edgeNum,
                            StartX = sx,
                            StartY = sy,
                            EndX = ex,
                            EndY = ey,
                            IsRadiusSegment = true,
                            RadiusIndex = radiusNum
                        });
                    }
                    break;
            }
        }
    }

    private static void ExtractPolylineFallback(
        DxfScene scene,
        Polyline2D poly,
        ref int edgeNum,
        ref int radiusNum,
        List<RadiusFeature> radii,
        List<ContourEdge> edges)
    {
        var verts = poly.Vertexes;
        int n = verts.Count;
        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;
            double bulge = verts[i].Bulge;
            var p0 = verts[i].Position;
            var p1 = verts[j].Position;

            int? radiusIndex = null;
            if (Math.Abs(bulge) >= 1e-12)
            {
                radiusNum++;
                var seg = new ContourPathOrderer.OrderedSegment
                {
                    EdgeIndex = edgeNum + 1,
                    CornerIndex = i + 1,
                    StartX = p0.X,
                    StartY = p0.Y,
                    EndX = p1.X,
                    EndY = p1.Y,
                    Bulge = bulge
                };
                var feature = BuildFromSegment(
                    scene,
                    new List<ContourPathOrderer.OrderedSegment> { seg },
                    0,
                    radiusNum);
                if (feature is not null)
                    radii.Add(feature);
                radiusIndex = radiusNum;
            }

            edges.Add(new ContourEdge
            {
                Index = ++edgeNum,
                CornerIndex = i + 1,
                StartX = p0.X,
                StartY = p0.Y,
                EndX = p1.X,
                EndY = p1.Y,
                IsRadiusSegment = Math.Abs(bulge) >= 1e-12,
                RadiusIndex = radiusIndex
            });
        }
    }

    private static bool TryAddStandaloneArc(DxfScene scene, Arc arc, ref int radiusNum, List<RadiusFeature> radii)
    {
        if (!BulgeArcConverter.TryFromArcEntity(arc,
                out double cx, out double cy, out double r,
                out _, out _, out double bulge))
            return false;

        ArcSampler.GetArcEndpointXY(arc, out double sx, out double sy, out double ex, out double ey);

        double ax = sx, ay = sy, bx = ex, by = ey;
        TryFindAdjacentLineDirections(scene, sx, sy, ex, ey,
            out double lineAtSx, out double lineAtEx);

        bool swapped = RadiusEndpointOrienter.ShouldSwapForCcw(ax, ay, bx, by);
        if (swapped)
        {
            (ax, ay, bx, by) = (bx, by, ax, ay);
            (lineAtSx, lineAtEx) = (lineAtEx, lineAtSx);
        }

        bool forwardCcw = swapped ? bulge < 0 : bulge > 0;
        double angA = AngleMath.DirectionDeg(ax - cx, ay - cy);
        double angB = AngleMath.DirectionDeg(bx - cx, by - cy);
        double tanAtStart = AngleMath.ArcTangentDeg(angA, forwardCcw);
        double tanAtEnd = AngleMath.ArcTangentDeg(angB, forwardCcw);

        double line1 = lineAtSx;
        if (double.IsNaN(line1))
            line1 = OppositeDirection(tanAtStart);

        double line2 = lineAtEx;
        if (double.IsNaN(line2))
            line2 = tanAtEnd;

        double mcx = (scene.Bounds.MinX + scene.Bounds.MaxX) * 0.5;
        double mcy = (scene.Bounds.MinY + scene.Bounds.MaxY) * 0.5;
        var angles = RadiusCornerAngles.Compute(line1, line2, tanAtStart, tanAtEnd, ax, ay, bx, by, mcx, mcy);

        radiusNum++;
        radii.Add(new RadiusFeature
        {
            Index = radiusNum,
            SourceLabel = "Yay",
            CenterX = cx,
            CenterY = cy,
            Radius = r,
            Convexity = RadiusConvexityClassifier.ClassifyForCcwTraversal(bulge),
            StartX = ax,
            StartY = ay,
            EndX = bx,
            EndY = by,
            Line1DirectionDeg = line1,
            Line2DirectionDeg = line2,
            CornerAngleDeg = angles.VirtualCornerAngleDeg,
            StartEdgeAngleDeg = angles.StartEdgeAngleDeg,
            EndEdgeAngleDeg = angles.EndEdgeAngleDeg,
            StartCornerAngleDeg = angles.StartCornerAngleDeg,
            EndCornerAngleDeg = angles.EndCornerAngleDeg,
            StartTangentAngleDeg = angles.StartTangentAngleDeg,
            EndTangentAngleDeg = angles.EndTangentAngleDeg,
            Bulge = bulge
        });
        return true;
    }

    private static void TryFindAdjacentLineDirections(
        DxfScene scene,
        double arcStartX, double arcStartY,
        double arcEndX, double arcEndY,
        out double lineIntoArcStart,
        out double lineOutOfArcEnd)
    {
        lineIntoArcStart = double.NaN;
        lineOutOfArcEnd = double.NaN;

        if (!scene.Bounds.HasBounds)
            return;

        double span = Math.Max(scene.Bounds.Width, scene.Bounds.Height);
        double eps = Math.Max(1e-4, span * 1e-6);

        foreach (var entity in scene.Entities)
        {
            if (entity is not Line line)
                continue;

            double x0 = line.StartPoint.X, y0 = line.StartPoint.Y;
            double x1 = line.EndPoint.X, y1 = line.EndPoint.Y;
            double dx = x1 - x0, dy = y1 - y0;

            if (PointsNear(x1, y1, arcStartX, arcStartY, eps) || PointsNear(x0, y0, arcStartX, arcStartY, eps))
                lineIntoArcStart = AngleMath.DirectionDeg(dx, dy);

            if (PointsNear(x0, y0, arcEndX, arcEndY, eps) || PointsNear(x1, y1, arcEndX, arcEndY, eps))
                lineOutOfArcEnd = AngleMath.DirectionDeg(dx, dy);
        }
    }

    private static bool PointsNear(double x1, double y1, double x2, double y2, double eps)
        => Math.Abs(x1 - x2) <= eps && Math.Abs(y1 - y2) <= eps;

    private static double TryAdjacentLineDirection(
        List<ContourPathOrderer.OrderedSegment> segments,
        int index,
        bool forward)
    {
        if (index < 0 || index >= segments.Count)
            return double.NaN;

        var adj = segments[index];
        if (adj.IsArc)
            return double.NaN;

        return AngleMath.DirectionDeg(adj.EndX - adj.StartX, adj.EndY - adj.StartY);
    }

    private static double OppositeDirection(double dirDeg)
        => AngleMath.Normalize360(dirDeg + 180.0);
}

public sealed class RadiusAnalysisResult
{
    public IReadOnlyList<RadiusFeature> RadiusFeatures { get; }
    public IReadOnlyList<ContourEdge> ContourEdges { get; }

    public RadiusAnalysisResult(IReadOnlyList<RadiusFeature> radiusFeatures, IReadOnlyList<ContourEdge> contourEdges)
    {
        RadiusFeatures = radiusFeatures;
        ContourEdges = contourEdges;
    }

    public static RadiusAnalysisResult Empty { get; } = new(Array.Empty<RadiusFeature>(), Array.Empty<ContourEdge>());
}
