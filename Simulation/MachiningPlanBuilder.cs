using otomasyon.Models.Simulation;

namespace otomasyon.Simulation;

public static class MachiningPlanBuilder
{
    public static MachiningPlan Build(
        ContourPath path,
        IReadOnlyDictionary<int, double> thicknessByEdge,
        StoneToolSettings tool,
        IReadOnlyDictionary<int, double>? offsetByEdge = null)
    {
        tool.Validate();
        var edgePlans = new List<EdgeMachiningPlan>(path.Segments.Count);

        foreach (var seg in path.Segments)
        {
            if (!thicknessByEdge.TryGetValue(seg.EdgeIndex, out double thickness))
                thickness = 0;

            double offset = 0;
            offsetByEdge?.TryGetValue(seg.EdgeIndex, out offset);

            var passes = OffsetPassPlanner.PlanPasses(thickness, tool.StoneWidthMm, tool.BindirmeMm);
            edgePlans.Add(new EdgeMachiningPlan
            {
                EdgeIndex = seg.EdgeIndex,
                TargetThicknessMm = thickness,
                OffsetMm = offset,
                Passes = passes
            });
        }

        return new MachiningPlan { Edges = edgePlans };
    }
}
