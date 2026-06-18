using System.Globalization;
using System.Text;
using otomasyon.Geometry;
using otomasyon.Localization;
using otomasyon.Models.Simulation;

namespace otomasyon.Simulation;

public static class SimulationLogFormatter
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static string FormatPlan(MachiningPlan plan, StoneToolSettings tool)
    {
        int tours = MachiningTourPlanner.GetGlobalTourCount(plan);
        var sb = new StringBuilder();
        sb.AppendLine(L.F("Log.StoneWidth", tool.StoneWidthMm.ToString("G4", Inv)));
        sb.AppendLine(L.F("Log.Overlap", tool.BindirmeMm.ToString("G4", Inv)));
        sb.AppendLine(L.F("Log.TotalTours", tours));
        sb.AppendLine();

        foreach (var edge in plan.Edges)
        {
            sb.AppendLine(L.F("Log.EdgeTarget", edge.EdgeIndex, edge.TargetThicknessMm.ToString("G4", Inv)));
            if (edge.Passes.Count == 0)
            {
                sb.AppendLine(L.Get("Log.EdgeSkipped"));
                continue;
            }

            for (int t = 0; t < tours; t++)
            {
                if (t < edge.Passes.Count)
                {
                    sb.AppendLine(L.F("Log.TourDepth", t + 1, edge.Passes[t].DepthFromContourMm.ToString("G4", Inv)));
                }
                else
                {
                    sb.AppendLine(L.F("Log.TourNoCut", t + 1));
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

        return L.F("Sim.SnapshotFormat",
            s.StatusText,
            s.ToolX,
            s.ToolY,
            s.ToolIsEngaged ? L.Get("Sim.ToolEngaged") : L.Get("Sim.ToolLifted"));
    }

    /// <summary>
    /// Bir kenara girerken tek satırlık doğrulama logu: tur, kenar/köşe, baş→bit koordinat,
    /// yön ve kesim/kalkık durumu. "Doğru yerden mi geçiyoruz" kontrolü için.
    /// </summary>
    public static string FormatEdgeEntry(SimulationSnapshot s, ContourPathSegment seg, bool cutting, double depth)
    {
        string shape = seg.IsArc ? L.Get("Sim.ShapeArc") : L.Get("Sim.ShapeLine");
        double dir = AngleMath.DirectionDeg(seg.EndX - seg.StartX, seg.EndY - seg.StartY);
        string dirText = double.IsNaN(dir) ? "-" : $"{dir.ToString("F1", Inv)}°";
        string mode = cutting
            ? L.F("Sim.ModeCutting", depth.ToString("G4", Inv))
            : L.Get("Sim.ModeRapid");

        return L.F("Sim.EdgeLog",
            s.TourIndex + 1,
            s.TourCount,
            seg.EdgeIndex,
            seg.CornerIndex,
            shape,
            seg.StartX,
            seg.StartY,
            seg.EndX,
            seg.EndY,
            seg.LengthMm,
            dirText,
            mode);
    }
}
