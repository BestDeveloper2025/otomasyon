using otomasyon.Models;

namespace otomasyon.Geometry;

/// <summary>
/// Kenarlar (CCW) + radius analizi birleşik metin.
/// </summary>
public static class SceneResultsTextFormatter
{
    public static string Format(DxfScene scene)
    {
        string edges = ContourEdgesTextFormatter.Format(scene.ContourEdges);
        string radii = RadiusFeaturesTextFormatter.Format(scene.RadiusFeatures);

        if (string.IsNullOrEmpty(edges))
            return radii;
        if (string.IsNullOrEmpty(radii))
            return edges;

        return edges + "\r\n\r\n" + radii;
    }
}
