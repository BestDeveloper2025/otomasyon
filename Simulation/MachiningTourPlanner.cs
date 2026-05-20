using otomasyon.Models.Simulation;

namespace otomasyon.Simulation;

/// <summary>Şekil turu (tam CCW kontur) ve kenar bazlı derinlik.</summary>
public static class MachiningTourPlanner
{
    public static int GetGlobalTourCount(MachiningPlan plan)
    {
        int max = 0;
        foreach (var e in plan.Edges)
        {
            if (e.Passes.Count > max)
                max = e.Passes.Count;
        }

        return max;
    }

    public static bool IsCuttingOnEdge(MachiningPlan plan, int edgeIndex, int tourIndex)
    {
        var edge = plan.FindEdge(edgeIndex);
        return edge is not null && tourIndex < edge.Passes.Count;
    }

    public static double GetDepthOnEdge(MachiningPlan plan, int edgeIndex, int tourIndex)
    {
        var edge = plan.FindEdge(edgeIndex);
        if (edge is null || tourIndex >= edge.Passes.Count)
            return 0;
        return edge.Passes[tourIndex].DepthFromContourMm;
    }
}
