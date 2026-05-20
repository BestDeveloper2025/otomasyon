using netDxf;
using netDxf.Entities;
using otomasyon.Analysis;
using otomasyon.Dxf;
using otomasyon.Geometry;

namespace otomasyon.Tools;

internal static class DxfContourDump
{
    public static void Run(string path)
    {
        var doc = DxfDocument.Load(path);
        if (doc is null)
        {
            Console.WriteLine("load fail");
            return;
        }

        var scene = new DxfSceneBuilder().Build(doc);
        Console.WriteLine($"Entities: {scene.Entities.Count}");
        foreach (var e in scene.Entities)
            Console.WriteLine($"  {e.Type}");

        if (ContourPathOrderer.TryBuildOrderedSegments(scene, out var segs))
        {
            Console.WriteLine($"Segments: {segs.Count}");
            foreach (var s in segs)
            {
                Console.WriteLine(
                    $"  L{s.EdgeIndex} K{s.CornerIndex} ({s.StartX:G6},{s.StartY:G6})->({s.EndX:G6},{s.EndY:G6}) bulge={s.Bulge:G6} arc={s.IsArc}");
            }
        }
        else
        {
            Console.WriteLine("No segments");
        }

        var analysis = new RadiusFeatureExtractor().Extract(scene);
        Console.WriteLine($"Edges: {analysis.ContourEdges.Count} Radii: {analysis.RadiusFeatures.Count}");
        foreach (var r in analysis.RadiusFeatures)
        {
            Console.WriteLine(
                $"  R{r.Index} baş={r.StartCornerAngleDeg:F1} bit={r.EndCornerAngleDeg:F1} L1={r.Line1DirectionDeg:F1} L2={r.Line2DirectionDeg:F1}");
        }
    }
}
