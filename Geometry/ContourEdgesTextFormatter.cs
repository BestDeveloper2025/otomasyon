using System.Globalization;
using System.Text;
using otomasyon.Localization;
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
                ? L.F("Analysis.EdgeKindRadius", e.Index, ri, e.CornerIndex)
                : L.F("Analysis.EdgeKind", e.Index, e.CornerIndex);
            sb.AppendLine(L.F("Analysis.EdgeSection", kind));
            sb.AppendLine(L.F("Analysis.Start", e.StartX.ToString("G9", CultureInfo.InvariantCulture), e.StartY.ToString("G9", CultureInfo.InvariantCulture)));
            sb.AppendLine(L.F("Analysis.End", e.EndX.ToString("G9", CultureInfo.InvariantCulture), e.EndY.ToString("G9", CultureInfo.InvariantCulture)));
            double len = e.LengthMm > 0
                ? e.LengthMm
                : SegmentLength.FromBulgeMm(e.StartX, e.StartY, e.EndX, e.EndY, e.Bulge);
            sb.AppendLine(L.F("Analysis.Length", len.ToString("G6", CultureInfo.InvariantCulture)));
        }

        return sb.ToString().TrimEnd();
    }
}
