using System.Globalization;
using System.Text;
using otomasyon.Geometry;
using otomasyon.Models.Simulation;

namespace otomasyon.Simulation;

public static class SimulationLogFormatter
{
    public static string FormatPlan(MachiningPlan plan, StoneToolSettings tool)
    {
        int tours = MachiningTourPlanner.GetGlobalTourCount(plan);
        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"Taş genişliği: {tool.StoneWidthMm:G4} mm");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Bindirme: {tool.BindirmeMm:G4} mm");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Toplam kontur turu: {tours} (her turda L1→L2→… tam şekil, CCW)");
        sb.AppendLine();

        foreach (var edge in plan.Edges)
        {
            sb.AppendLine(CultureInfo.InvariantCulture,
                $"--- L{edge.EdgeIndex} hedef kalınlık {edge.TargetThicknessMm:G4} mm ---");
            if (edge.Passes.Count == 0)
            {
                sb.AppendLine("  Bu kenar turlarda işlenmez (0 mm); taş kalkık geçilir.");
                continue;
            }

            for (int t = 0; t < tours; t++)
            {
                if (t < edge.Passes.Count)
                {
                    sb.AppendLine(CultureInfo.InvariantCulture,
                        $"  Kontur tur {t + 1}: derinlik {edge.Passes[t].DepthFromContourMm:G4} mm");
                }
                else
                {
                    sb.AppendLine(CultureInfo.InvariantCulture,
                        $"  Kontur tur {t + 1}: işleme yok (taş kalkık)");
                }
            }

            sb.AppendLine();
        }

        return sb.ToString().TrimEnd();
    }

    public static string FormatSnapshot(SimulationSnapshot s)
    {
        if (s.IsFinished)
            return s.StatusText;

        return string.Format(CultureInfo.InvariantCulture,
            "{0}\nTaş: ({1:G4}, {2:G4}) {3}",
            s.StatusText, s.ToolX, s.ToolY,
            s.ToolIsEngaged ? "[açık]" : "[kalkık]");
    }

    /// <summary>
    /// Bir kenara girerken tek satırlık doğrulama logu: tur, kenar/köşe, baş→bit koordinat,
    /// yön ve kesim/kalkık durumu. "Doğru yerden mi geçiyoruz" kontrolü için.
    /// </summary>
    public static string FormatEdgeEntry(SimulationSnapshot s, ContourPathSegment seg, bool cutting, double depth)
    {
        string shape = seg.IsArc ? "yay" : "düz";
        double dir = AngleMath.DirectionDeg(seg.EndX - seg.StartX, seg.EndY - seg.StartY);
        string dirText = double.IsNaN(dir) ? "-" : $"{dir:F1}°";
        string mode = cutting
            ? string.Format(CultureInfo.InvariantCulture, "KESİM (derinlik {0:G4} mm)", depth)
            : "KALKIK (rapid)";

        return string.Format(CultureInfo.InvariantCulture,
            "Tur {0}/{1} | L{2} K{3} ({4}): ({5:0.##}, {6:0.##}) → ({7:0.##}, {8:0.##}) | uz {9:0.##} mm | yön {10} | {11}",
            s.TourIndex + 1, s.TourCount, seg.EdgeIndex, seg.CornerIndex, shape,
            seg.StartX, seg.StartY, seg.EndX, seg.EndY, seg.LengthMm, dirText, mode);
    }
}
