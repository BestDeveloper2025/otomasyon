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
                    var feature = BuildFromSegment(segments, si, radiusNum);
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

            return RadiusTraversalOrderer.Apply(new RadiusAnalysisResult(radii, edges));
        }

        ExtractFallbackEntityOrder(scene, radii, edges);
        return RadiusTraversalOrderer.Apply(new RadiusAnalysisResult(radii, edges));
    }

    private static RadiusFeature? BuildFromSegment(
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

        var angles = RadiusCornerAngles.Compute(line1Dir, line2Dir, tanAtStart, tanAtEnd);

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
                    ExtractPolylineFallback(poly, ref edgeNum, ref radiusNum, radii, edges);
                    break;
                case Arc arc:
                    if (TryAddStandaloneArc(arc, ref radiusNum, radii))
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

    private static bool TryAddStandaloneArc(Arc arc, ref int radiusNum, List<RadiusFeature> radii)
    {
        if (!BulgeArcConverter.TryFromArcEntity(arc,
                out double cx, out double cy, out double r,
                out double startAng, out double endAng, out double bulge))
            return false;

        ArcSampler.GetArcEndpointXY(arc, out double sx, out double sy, out double ex, out double ey);
        bool ccw = bulge > 0;
        double tanStart = AngleMath.ArcTangentDeg(startAng, ccw);
        double tanEnd = AngleMath.ArcTangentDeg(endAng, ccw);

        double line1 = OppositeDirection(tanStart);
        double line2 = tanEnd;
        var angles = RadiusCornerAngles.Compute(line1, line2, tanStart, tanEnd);

        radiusNum++;
        radii.Add(new RadiusFeature
        {
            Index = radiusNum,
            SourceLabel = "Yay",
            CenterX = cx,
            CenterY = cy,
            Radius = r,
            Convexity = RadiusConvexityClassifier.ClassifyForCcwTraversal(bulge),
            StartX = sx,
            StartY = sy,
            EndX = ex,
            EndY = ey,
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
