using netDxf.Entities;

namespace otomasyon.Models;

/// <summary>
/// Bir DXF layout’tan çıkarılmış çizilebilir sahne: entity’ler, istatistik ve sınır kutusu.
/// </summary>
public sealed class DxfScene
{
    public static DxfScene Empty { get; } = new(
        Array.Empty<EntityObject>(),
        Array.Empty<IReadOnlyList<(double X, double Y)>>(),
        SceneBoundingBox.Empty,
        SceneStatistics.Zero,
        Array.Empty<RadiusFeature>(),
        Array.Empty<ContourEdge>());

    public IReadOnlyList<EntityObject> Entities { get; }
    public IReadOnlyList<IReadOnlyList<(double X, double Y)>> EntityPointLists { get; }
    public SceneBoundingBox Bounds { get; }
    public SceneStatistics Statistics { get; }
    public IReadOnlyList<RadiusFeature> RadiusFeatures { get; }
    public IReadOnlyList<ContourEdge> ContourEdges { get; }

    public DxfScene(
        IReadOnlyList<EntityObject> entities,
        IReadOnlyList<IReadOnlyList<(double X, double Y)>> entityPointLists,
        SceneBoundingBox bounds,
        SceneStatistics statistics,
        IReadOnlyList<RadiusFeature> radiusFeatures,
        IReadOnlyList<ContourEdge> contourEdges)
    {
        if (entities.Count != entityPointLists.Count)
            throw new ArgumentException("Her entity için nokta listesi olmalıdır.", nameof(entityPointLists));

        Entities = entities;
        EntityPointLists = entityPointLists;
        Bounds = bounds;
        Statistics = statistics;
        RadiusFeatures = radiusFeatures;
        ContourEdges = contourEdges;
    }
}
