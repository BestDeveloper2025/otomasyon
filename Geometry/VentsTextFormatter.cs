using System.Globalization;
using System.Text;
using otomasyon.Localization;
using otomasyon.Models;

namespace otomasyon.Geometry;

/// <summary>
/// Menfez listesi (M1, M2…) — başlangıç noktasına uzaklık sırasıyla.
/// </summary>
public static class VentsTextFormatter
{
    public static string Format(IReadOnlyList<VentFeature> vents)
    {
        if (vents.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var v in vents)
        {
            if (sb.Length > 0)
                sb.AppendLine();

            sb.AppendLine(L.F("Analysis.VentSection", v.Index));
            sb.AppendLine(L.F("Analysis.VentCenter",
                v.CenterX.ToString("G9", CultureInfo.InvariantCulture),
                v.CenterY.ToString("G9", CultureInfo.InvariantCulture)));
            sb.AppendLine(L.F("Analysis.VentDistance",
                v.DistanceFromOriginMm.ToString("G6", CultureInfo.InvariantCulture)));
            sb.AppendLine(L.F("Analysis.VentRadius",
                v.RadiusMm.ToString("G6", CultureInfo.InvariantCulture)));
        }

        return sb.ToString().TrimEnd();
    }
}
