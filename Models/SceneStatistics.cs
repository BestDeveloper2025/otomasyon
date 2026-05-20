namespace otomasyon.Models;

/// <summary>
/// Arayüzde gösterilen özet sayımlar.
/// </summary>
public readonly struct SceneStatistics
{
    public static SceneStatistics Zero => default;

    public int EdgeLineAndPolySegments { get; init; }
    public int ArcCount { get; init; }
    public int CircleCount { get; init; }
    public int TrackedEntityCount { get; init; }
}
