using System.Globalization;
using System.Text;
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
}
