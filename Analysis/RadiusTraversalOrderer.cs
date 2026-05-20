using otomasyon.Models;

namespace otomasyon.Analysis;

/// <summary>
/// Radiusları (0,0) referansından +X yönünde başlayarak CCW dolaşım sırasına göre numaralandırır.
/// </summary>
public static class RadiusTraversalOrderer
{
    /// <summary>
    /// Yürüyüşte yaya ilk girilen uç noktanın CCW açı parametresi (0 = +X ekseni).
    /// </summary>
    public static double GetEntryCcwParameter(RadiusFeature feature)
    {
        double pStart = CcwAngleFromPositiveX(feature.StartX, feature.StartY);
        double pEnd = CcwAngleFromPositiveX(feature.EndX, feature.EndY);

        if (feature.Bulge > 0)
            return Math.Min(pStart, pEnd);

        if (feature.Bulge < 0)
            return Math.Max(pStart, pEnd);

        return Math.Min(pStart, pEnd);
    }

    public static RadiusAnalysisResult Apply(RadiusAnalysisResult result)
    {
        if (result.RadiusFeatures.Count <= 1)
            return result;

        var sorted = result.RadiusFeatures
            .OrderBy(GetEntryCcwParameter)
            .ThenBy(f => f.CenterX)
            .ThenBy(f => f.CenterY)
            .ToList();

        var oldToNew = new Dictionary<int, int>(sorted.Count);
        var renumbered = new List<RadiusFeature>(sorted.Count);

        for (int i = 0; i < sorted.Count; i++)
        {
            int newIndex = i + 1;
            var f = sorted[i];
            oldToNew[f.Index] = newIndex;
            renumbered.Add(CopyWithIndex(f, newIndex));
        }

        var edges = new List<ContourEdge>(result.ContourEdges.Count);
        foreach (var e in result.ContourEdges)
        {
            int? radiusIndex = e.RadiusIndex;
            if (radiusIndex is int ri && oldToNew.TryGetValue(ri, out int ni))
                radiusIndex = ni;

            edges.Add(new ContourEdge
            {
                Index = e.Index,
                CornerIndex = e.CornerIndex,
                StartX = e.StartX,
                StartY = e.StartY,
                EndX = e.EndX,
                EndY = e.EndY,
                IsRadiusSegment = e.IsRadiusSegment,
                RadiusIndex = radiusIndex
            });
        }

        return new RadiusAnalysisResult(renumbered, edges);
    }

    private static double CcwAngleFromPositiveX(double x, double y)
    {
        double a = Math.Atan2(y, x);
        if (a < 0)
            a += 2.0 * Math.PI;
        return a;
    }

    private static RadiusFeature CopyWithIndex(RadiusFeature f, int index) => new()
    {
        Index = index,
        SourceLabel = f.SourceLabel,
        CenterX = f.CenterX,
        CenterY = f.CenterY,
        Radius = f.Radius,
        Convexity = f.Convexity,
        StartX = f.StartX,
        StartY = f.StartY,
        EndX = f.EndX,
        EndY = f.EndY,
        Line1DirectionDeg = f.Line1DirectionDeg,
        Line2DirectionDeg = f.Line2DirectionDeg,
        CornerAngleDeg = f.CornerAngleDeg,
        StartEdgeAngleDeg = f.StartEdgeAngleDeg,
        EndEdgeAngleDeg = f.EndEdgeAngleDeg,
        StartCornerAngleDeg = f.StartCornerAngleDeg,
        EndCornerAngleDeg = f.EndCornerAngleDeg,
        StartTangentAngleDeg = f.StartTangentAngleDeg,
        EndTangentAngleDeg = f.EndTangentAngleDeg,
        Bulge = f.Bulge,
        EdgeIndex = f.EdgeIndex,
        CornerIndex = f.CornerIndex
    };
}
