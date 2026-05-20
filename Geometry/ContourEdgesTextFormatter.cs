using System.Globalization;
using System.Text;
using otomasyon.Models;

namespace otomasyon.Geometry;

/// <summary>
/// CCW sıralı kenar listesi (Köşe + kenar uçları).
/// </summary>
public static class ContourEdgesTextFormatter
{
    public static string Format(IReadOnlyList<ContourEdge> edges)
    {
        if (edges.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var e in edges)
        {
            if (sb.Length > 0)
                sb.AppendLine();

            sb.AppendLine(CultureInfo.InvariantCulture, $"--- Kenar {e.Index} (Köşe {e.CornerIndex}) ---");
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"Başlangıç: {e.StartX:G9} {e.StartY:G9}");
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"Bitiş: {e.EndX:G9} {e.EndY:G9}");
            if (e.IsRadiusSegment && e.RadiusIndex is int ri)
                sb.AppendLine(CultureInfo.InvariantCulture, $"Radius: {ri}");
        }

        return sb.ToString().TrimEnd();
    }
}
