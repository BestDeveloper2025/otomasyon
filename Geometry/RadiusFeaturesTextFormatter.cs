using System.Globalization;
using System.Text;
using otomasyon.Analysis;
using otomasyon.Localization;
using otomasyon.Models;

namespace otomasyon.Geometry;

/// <summary>
/// Sağ panel için radius analiz metni.
/// </summary>
public static class RadiusFeaturesTextFormatter
{
    public static string Format(IReadOnlyList<RadiusFeature> features)
    {
        if (features.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        foreach (var f in features)
        {
            if (sb.Length > 0)
                sb.AppendLine();

            var inv = CultureInfo.InvariantCulture;
            string source = f.EdgeIndex > 0
                ? L.F("Analysis.EdgeSource", f.EdgeIndex)
                : L.Get("Analysis.ArcSource");
            sb.AppendLine(L.F("Analysis.RadiusHeader", f.Index, source));
            sb.AppendLine(L.F("Analysis.Convexity", RadiusConvexityClassifier.ToDisplayName(f.Convexity)));
            sb.AppendLine(L.F("Analysis.Center", f.CenterX.ToString("G9", inv), f.CenterY.ToString("G9", inv)));
            sb.AppendLine(L.F("Analysis.RadiusValue", f.Radius.ToString("G9", inv)));
            sb.AppendLine(L.F("Analysis.Start", f.StartX.ToString("G9", inv), f.StartY.ToString("G9", inv)));
            sb.AppendLine(L.F("Analysis.End", f.EndX.ToString("G9", inv), f.EndY.ToString("G9", inv)));
            sb.AppendLine(L.F("Analysis.StartCornerAngle", f.StartCornerAngleDeg.ToString("0.###", inv)));
            sb.AppendLine(L.F("Analysis.EndCornerAngle", f.EndCornerAngleDeg.ToString("0.###", inv)));
            sb.AppendLine(L.F("Analysis.VirtualCornerAngle", f.CornerAngleDeg.ToString("0.###", inv)));
            sb.AppendLine(L.F("Analysis.StartEdgeDir", f.StartEdgeAngleDeg.ToString("G6", inv)));
            sb.AppendLine(L.F("Analysis.EndEdgeDir", f.EndEdgeAngleDeg.ToString("G6", inv)));
            sb.AppendLine(L.F("Analysis.StartTangent", f.StartTangentAngleDeg.ToString("G6", inv)));
            sb.AppendLine(L.F("Analysis.EndTangent", f.EndTangentAngleDeg.ToString("G6", inv)));
        }

        return sb.ToString().TrimEnd();
    }
}
