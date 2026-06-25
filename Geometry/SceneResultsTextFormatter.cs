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
        string vents = VentsTextFormatter.Format(scene.VentFeatures);
        string radii = RadiusFeaturesTextFormatter.Format(scene.RadiusFeatures);

        var parts = new List<string>(3);
        if (!string.IsNullOrEmpty(edges))
            parts.Add(edges);
        if (!string.IsNullOrEmpty(vents))
            parts.Add(vents);
        if (!string.IsNullOrEmpty(radii))
            parts.Add(radii);

        return parts.Count == 0 ? string.Empty : string.Join("\r\n\r\n", parts);
    }
}
