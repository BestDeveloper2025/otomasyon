namespace otomasyon;

/// <summary>
/// Çift tamponlu çizim paneli (büyük DXF önizlemelerinde titremeyi azaltır).
/// </summary>
internal sealed class DrawingPanel : Panel
{
    public DrawingPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
    }
}
