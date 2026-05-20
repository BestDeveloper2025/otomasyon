using System.Globalization;
using System.Text;
using otomasyon.Models;

namespace otomasyon.Geometry;

/// <summary>
/// Sağ panel metin kutusu için köşe listesi (yalnızca sayılar).
/// </summary>
public static class CoordinatesTextFormatter
{
    public static string FormatCorners(DxfScene scene)
    {
        if (scene.Entities.Count == 0)
            return string.Empty;

        var sb = new StringBuilder();
        for (int i = 0; i < scene.Entities.Count; i++)
        {
            if (i > 0)
                sb.AppendLine();

            var buf = new List<(double X, double Y)>(16);
            WorldCornerCollector.Collect(scene.Entities[i], buf);
            foreach (var p in buf)
                AppendXYLine(sb, p.X, p.Y);
        }

        return sb.ToString().TrimEnd();
    }

    private static void AppendXYLine(StringBuilder sb, double x, double y)
    {
        sb.AppendFormat(CultureInfo.InvariantCulture, "{0:G9} {1:G9}\r\n", x, y);
    }
}
