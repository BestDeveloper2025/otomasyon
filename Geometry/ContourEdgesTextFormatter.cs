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

            string kind = e.IsRadiusSegment && e.RadiusIndex is int ri
                ? $"Kenar {e.Index} / Yay R{ri} (Köşe {e.CornerIndex})"
                : $"Kenar {e.Index} (Köşe {e.CornerIndex})";
            sb.AppendLine(CultureInfo.InvariantCulture, $"--- {kind} ---");
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"Başlangıç: {e.StartX:G9} {e.StartY:G9}");
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"Bitiş: {e.EndX:G9} {e.EndY:G9}");
            double len = e.LengthMm > 0
                ? e.LengthMm
                : SegmentLength.FromBulgeMm(e.StartX, e.StartY, e.EndX, e.EndY, e.Bulge);
            sb.AppendLine(CultureInfo.InvariantCulture, $"Uzunluk: {len:G6} mm");
        }

        return sb.ToString().TrimEnd();
    }
}
