using System.Globalization;
using System.Text;
using otomasyon.Analysis;
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

            sb.AppendLine(CultureInfo.InvariantCulture, $"--- Radius {f.Index} ({f.SourceLabel}) ---");
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"Bükeylik: {RadiusConvexityClassifier.ToDisplayName(f.Convexity)}");
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"Merkez: {f.CenterX:G9} {f.CenterY:G9}");
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"R: {f.Radius:G9}");
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"Başlangıç: {f.StartX:G9} {f.StartY:G9}");
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"Bitiş: {f.EndX:G9} {f.EndY:G9}");
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"Başlangıç köşe açısı°: {f.StartCornerAngleDeg:G6}");
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"Bitiş köşe açısı°: {f.EndCornerAngleDeg:G6}");
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"Sanal köşe açısı°: {f.CornerAngleDeg:G6}");
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"Başlangıç kenar yön°: {f.StartEdgeAngleDeg:G6}");
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"Bitiş kenar yön°: {f.EndEdgeAngleDeg:G6}");
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"Başlangıç teğet°: {f.StartTangentAngleDeg:G6}");
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"Bitiş teğet°: {f.EndTangentAngleDeg:G6}");
        }

        return sb.ToString().TrimEnd();
    }
}
