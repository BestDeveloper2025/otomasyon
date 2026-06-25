using otomasyon.Analysis;
using otomasyon.Localization;
using otomasyon.Models;
using otomasyon.Models.Simulation;

namespace otomasyon.Simulation;

public static class SimulationJobFactory
{
    public static bool TryCreate(
        DxfScene scene,
        string sourceFilePath,
        IReadOnlyDictionary<int, double> thicknessByEdgeMm,
        StoneToolSettings tool,
        out SimulationJob? job,
        out string? error,
        IReadOnlyDictionary<int, double>? offsetByEdgeMm = null,
        IReadOnlyDictionary<int, double>? ventStrippingByIndex = null)
    {
        job = null;
        error = null;

        if (!ContourPathOrderer.HasSimulatableContour(scene))
        {
            error = L.Get("Error.NoClosedContour");
            return false;
        }

        if (!ContourPathBuilder.TryBuild(scene, out var path))
        {
            error = L.Get("Error.PathBuildFailed");
            return false;
        }

        try
        {
            tool.Validate();
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }

        var plan = MachiningPlanBuilder.Build(path, thicknessByEdgeMm, tool, offsetByEdgeMm);
        job = new SimulationJob
        {
            Scene = scene,
            Path = path,
            Plan = plan,
            Tool = tool,
            SourceFilePath = sourceFilePath,
            VentStrippingByIndex = ventStrippingByIndex ?? new Dictionary<int, double>()
        };
        return true;
    }
}
