using System.Globalization;
using System.Text;
using otomasyon.Localization;
using otomasyon.Models.Simulation;

namespace otomasyon.Simulation;

public static class SimulationReportBuilder
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    public static string BuildReport(SimulationJob job, SimulationSnapshot finalSnapshot)
    {
        var sb = new StringBuilder();
        sb.AppendLine(L.Get("Report.Title"));
        sb.AppendLine(L.F("Report.File", Path.GetFileName(job.SourceFilePath)));
        sb.AppendLine(L.F("Report.TotalTours", finalSnapshot.TourCount));

        double speedMmPerMin = 2000.0;
        double speedRapidMmPerMin = 5000.0;

        double cuttingDistance = finalSnapshot.TotalCuttingMm;
        double rapidDistance = finalSnapshot.TotalTraversedMm - finalSnapshot.TotalCuttingMm;

        double timeMinutes = (cuttingDistance / speedMmPerMin) + (rapidDistance / speedRapidMmPerMin);

        sb.AppendLine(L.F("Report.TotalMovement", finalSnapshot.TotalTraversedMm.ToString("N1", Inv)));
        sb.AppendLine(L.F("Report.CuttingDistance", cuttingDistance.ToString("N1", Inv)));
        sb.AppendLine(L.F("Report.RapidDistance", rapidDistance.ToString("N1", Inv)));
        sb.AppendLine(L.F("Report.EstimatedTime", timeMinutes.ToString("N1", Inv)));
        sb.AppendLine();
        sb.AppendLine(L.Get("Report.EdgeDetails"));

        foreach (var edgePlan in job.Plan.Edges)
        {
            var passes = edgePlan.Passes;
            if (passes.Count == 0 || edgePlan.TargetThicknessMm < 1e-6)
            {
                sb.AppendLine(L.F("Report.EdgeNotMachined", edgePlan.EdgeIndex));
                continue;
            }

            sb.AppendLine(L.F("Report.EdgeTarget", edgePlan.EdgeIndex, edgePlan.TargetThicknessMm.ToString("N1", Inv)));

            var seg = FindSegment(job, edgePlan.EdgeIndex);
            if (seg is not null)
            {
                string shape = seg.IsArc ? L.Get("Sim.ShapeArc") : L.Get("Sim.ShapeLine");
                sb.AppendLine(L.F("Report.EdgePosition",
                    shape, seg.StartX, seg.StartY, seg.EndX, seg.EndY, seg.LengthMm));
            }

            sb.AppendLine(L.F("Report.TotalPasses", passes.Count));

            for (int i = 0; i < passes.Count; i++)
            {
                sb.AppendLine(L.F("Report.PassDepth", i + 1, passes[i].DepthFromContourMm.ToString("N2", Inv)));
            }
        }

        sb.AppendLine();
        sb.AppendLine(L.Get("Report.Note"));

        return sb.ToString();
    }

    private static ContourPathSegment? FindSegment(SimulationJob job, int edgeIndex)
    {
        foreach (var seg in job.Path.Segments)
        {
            if (seg.EdgeIndex == edgeIndex)
                return seg;
        }

        return null;
    }
}
