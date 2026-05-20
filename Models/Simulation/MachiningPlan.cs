namespace otomasyon.Models.Simulation;

public sealed class MachiningPlan
{
    public IReadOnlyList<EdgeMachiningPlan> Edges { get; init; } = Array.Empty<EdgeMachiningPlan>();

    public EdgeMachiningPlan? FindEdge(int edgeIndex)
    {
        foreach (var e in Edges)
        {
            if (e.EdgeIndex == edgeIndex)
                return e;
        }

        return null;
    }
}
