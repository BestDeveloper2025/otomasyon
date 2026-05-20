namespace otomasyon.Models;

/// <summary>
/// DXF dünya düzleminde (XY) eksenlere paralel sınır kutusu.
/// </summary>
public readonly struct SceneBoundingBox
{
    public static SceneBoundingBox Empty => default;

    public double MinX { get; init; }
    public double MaxX { get; init; }
    public double MinY { get; init; }
    public double MaxY { get; init; }
    public bool HasBounds { get; init; }

    public double Width => MaxX - MinX;
    public double Height => MaxY - MinY;
}
