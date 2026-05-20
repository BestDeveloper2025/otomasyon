using otomasyon.Analysis;
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
        out string? error)
    {
        job = null;
        error = null;

        if (!ContourPathOrderer.HasSimulatableContour(scene))
        {
            error = "Kapalı kontur bulunamadı. Kapalı polyline veya uç uca birleşen LINE'lar gerekir.";
            return false;
        }

        if (!ContourPathBuilder.TryBuild(scene, out var path))
        {
            error = "Kontur yolu oluşturulamadı.";
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

        var plan = MachiningPlanBuilder.Build(path, thicknessByEdgeMm, tool);
        job = new SimulationJob
        {
            Scene = scene,
            Path = path,
            Plan = plan,
            Tool = tool,
            SourceFilePath = sourceFilePath
        };
        return true;
    }
}
