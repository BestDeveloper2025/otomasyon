using netDxf;
using otomasyon.Dxf;
using otomasyon.Geometry;
using otomasyon.Models;
using otomasyon.Rendering;

namespace otomasyon;

/// <summary>
/// DXF yükler, sahneyi gösterir; mantık <see cref="DxfSceneBuilder"/> ve <see cref="DxfSceneRenderer"/> içindedir.
/// </summary>
public partial class Form1 : Form
{
    private const double PaddingPixels = 20d;

    private readonly DxfSceneBuilder _sceneBuilder = new();
    private readonly DxfSceneRenderer _sceneRenderer = new();

    private DxfScene _scene = DxfScene.Empty;

    public Form1()
    {
        InitializeComponent();
        _btnSelectFile.Click += BtnSelectFile_Click;
        _drawPanel.Paint += DrawPanel_Paint;
    }

    private void Form1_Load(object? sender, EventArgs e) => ApplyInitialSplitLayout();

    private void ApplyInitialSplitLayout()
    {
        const int panel1Min = 280;
        const int panel2Min = 220;
        int w = _splitMain.ClientSize.Width;
        if (w <= panel1Min + panel2Min + _splitMain.SplitterWidth)
            return;

        _splitMain.Panel1MinSize = panel1Min;
        _splitMain.Panel2MinSize = panel2Min;

        int splitter = _splitMain.SplitterWidth;
        int maxDist = w - splitter - panel2Min;
        int minDist = panel1Min;
        int desired = (int)Math.Round(w * 0.62);
        desired = Math.Clamp(desired, minDist, Math.Max(minDist, maxDist));
        _splitMain.SplitterDistance = desired;
    }

    private void BtnSelectFile_Click(object? sender, EventArgs e)
    {
        try
        {
            using var dlg = new OpenFileDialog
            {
                Filter = "DXF Dosyaları (*.dxf)|*.dxf",
                Title = "DXF dosyası seçin"
            };

            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            _lblFilePath.Text = dlg.FileName;
            LoadDxfFile(dlg.FileName);
            _drawPanel.Invalidate();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                "Dosya açılırken hata oluştu:\n" + ex.Message,
                "Hata",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void LoadDxfFile(string path)
    {
        _scene = DxfScene.Empty;
        _txtCoordinates.Clear();

        try
        {
            DxfDocument? doc = DxfDocument.Load(path);
            if (doc is null)
            {
                MessageBox.Show(this,
                    "DXF dosyası yüklenemedi (işlem null döndü).",
                    "DXF",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                RefreshResultsUi();
                return;
            }

            _scene = _sceneBuilder.Build(doc);
            _txtCoordinates.Text = SceneResultsTextFormatter.Format(_scene);
            _txtCoordinates.SelectionStart = 0;
            _txtCoordinates.ScrollToCaret();
            RefreshResultsUi();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                "DXF okunamadı veya çözümlenemedi:\n" + ex.Message,
                "DXF Okuma Hatası",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            _txtCoordinates.Clear();
            RefreshResultsUi();
        }
    }

    private void RefreshResultsUi()
    {
        var s = _scene.Statistics;
        _lblResults.Text = string.Format(System.Globalization.CultureInfo.InvariantCulture,
            "Kontur kenar: {0} | Radius: {1} | Yay: {2} | Daire: {3} | Entity: {4}",
            s.ContourEdgeCount,
            s.RadiusFeatureCount,
            s.ArcCount,
            s.CircleCount,
            s.TrackedEntityCount);
    }

    private void DrawPanel_Paint(object? sender, PaintEventArgs e)
    {
        try
        {
            var rect = _drawPanel.ClientRectangle;
            if (!WorldToScreenTransform.TryCreate(rect, _scene.Bounds, PaddingPixels, out WorldToScreenTransform transform))
            {
                e.Graphics.Clear(Color.White);
                return;
            }

            _sceneRenderer.Paint(e.Graphics, _scene, rect, transform);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                "Çizim sırasında hata oluştu:\n" + ex.Message,
                "Çizim Hatası",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
