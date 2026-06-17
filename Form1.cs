using netDxf;
using otomasyon.Analysis;
using otomasyon.Dxf;
using otomasyon.Geometry;
using otomasyon.Models;
using otomasyon.Models.Recipe;
using otomasyon.Models.Simulation;
using otomasyon.Rendering;
using otomasyon.Simulation;
using otomasyon.UI;

namespace otomasyon;

/// <summary>
/// DXF yükler, sahneyi gösterir; reçete oluşturma ve toplu CSV çıktısı.
/// </summary>
public partial class Form1 : Form
{
    private const double PaddingPixels = 20d;

    private readonly DxfSceneBuilder _sceneBuilder = new();
    private readonly DxfSceneRenderer _sceneRenderer = new();
    private readonly List<RecipeItem> _recipeItems = new();

    private DxfScene _scene = DxfScene.Empty;
    private string _currentFilePath = string.Empty;

    public Form1()
    {
        InitializeComponent();
        _btnSelectFile.Click += BtnSelectFile_Click;
        _btnAddToRecipe.Click += BtnAddToRecipe_Click;
        _btnSimulation.Click += BtnSimulation_Click;
        _btnExportBatchCsv.Click += BtnExportBatchCsv_Click;
        _btnExportBatchDat.Click += BtnExportBatchDat_Click;
        _btnRemoveRecipe.Click += BtnRemoveRecipe_Click;
        _btnClearRecipe.Click += BtnClearRecipe_Click;
        _lvRecipe.SelectedIndexChanged += (_, _) => RefreshRecipeActionButtons();
        _drawPanel.Paint += DrawPanel_Paint;
    }

    private void Form1_Load(object? sender, EventArgs e)
    {
        ApplyInitialSplitLayout();
        RefreshRecipeUi();
    }

    private void ApplyInitialSplitLayout()
    {
        const int panel1Min = 300;
        const int panel2Min = 280;
        int w = _splitMain.ClientSize.Width;
        if (w > panel1Min + panel2Min + _splitMain.SplitterWidth)
        {
            _splitMain.Panel1MinSize = panel1Min;
            _splitMain.Panel2MinSize = panel2Min;

            int splitter = _splitMain.SplitterWidth;
            int maxDist = w - splitter - panel2Min;
            int minDist = panel1Min;
            int desired = (int)Math.Round(w * 0.58);
            desired = Math.Clamp(desired, minDist, Math.Max(minDist, maxDist));
            _splitMain.SplitterDistance = desired;
        }

        int h = _splitRight.ClientSize.Height;
        if (h > 260)
            _splitRight.SplitterDistance = Math.Clamp((int)Math.Round(h * 0.38), 140, h - 160);
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

            _currentFilePath = dlg.FileName;
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

        bool canProcess = ContourPathOrderer.HasSimulatableContour(_scene);
        _btnSimulation.Enabled = canProcess;
        _btnAddToRecipe.Enabled = canProcess && !string.IsNullOrWhiteSpace(_currentFilePath);
    }

    private void BtnAddToRecipe_Click(object? sender, EventArgs e)
    {
        if (!TryCreateJobFromCurrentScene(
                SetupPurpose.Recipe,
                out SimulationJob? job,
                out string? error,
                out CsvFileExporter.ExportOptions? exportOptions))
        {
            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show(this, error, "Reçete", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return;
        }

        var item = new RecipeItem
        {
            DisplayName = Path.GetFileName(_currentFilePath),
            SourceFilePath = _currentFilePath,
            Job = job!,
            ExportOptions = exportOptions!
        };

        _recipeItems.Add(item);
        AddRecipeListItem(item, _recipeItems.Count);
        RefreshRecipeUi();

        MessageBox.Show(this,
            $"\"{item.DisplayName}\" reçeteye eklendi.\nBaşka bir DXF yükleyip işleme devam edebilirsiniz.",
            "Reçete",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void BtnSimulation_Click(object? sender, EventArgs e)
    {
        if (!TryCreateJobFromCurrentScene(
                SetupPurpose.Simulation,
                out SimulationJob? job,
                out string? error,
                out _))
        {
            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show(this, error, "Simülasyon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return;
        }

        using var sim = new SimulationForm(job!);
        sim.ShowDialog(this);
    }

    private bool TryCreateJobFromCurrentScene(
        SetupPurpose purpose,
        out SimulationJob? job,
        out string? error,
        out CsvFileExporter.ExportOptions? exportOptions)
    {
        job = null;
        error = null;
        exportOptions = null;

        if (!ContourPathOrderer.HasSimulatableContour(_scene))
        {
            error = "Simülasyon için kapalı bir kontur gerekir (kapalı polyline veya birleşen çizgiler).";
            return false;
        }

        using var setup = new SimulationSetupDialog(_scene, purpose);
        if (setup.ShowDialog(this) != DialogResult.OK ||
            setup.ThicknessByEdge is null ||
            setup.Tool is null)
            return false;

        if (purpose == SetupPurpose.Recipe && setup.CsvExportOptions is null)
        {
            error = "CSV çıktı parametreleri eksik.";
            return false;
        }

        if (!SimulationJobFactory.TryCreate(
                _scene,
                _currentFilePath,
                setup.ThicknessByEdge,
                setup.Tool,
                out job,
                out error,
                setup.OffsetByEdge))
            return false;

        if (purpose == SetupPurpose.Recipe)
            exportOptions = setup.CsvExportOptions;

        return true;
    }

    private void BtnExportBatchCsv_Click(object? sender, EventArgs e)
        => ExportRecipeBatch(csv: true);

    private void BtnExportBatchDat_Click(object? sender, EventArgs e)
        => ExportRecipeBatch(csv: false);

    private void ExportRecipeBatch(bool csv)
    {
        if (_recipeItems.Count == 0)
        {
            MessageBox.Show(this,
                "Reçetede kayıtlı şekil yok. Önce DXF yükleyip \"Reçeteye Ekle\" kullanın.",
                csv ? "Toplu CSV" : "Toplu DAT",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        using var saveDlg = new SaveFileDialog
        {
            Filter = csv ? "CSV Dosyası (*.csv)|*.csv" : "DAT Dosyası (*.dat)|*.dat",
            Title = csv ? "Toplu CSV dosyasını kaydet" : "Toplu DAT dosyasını kaydet",
            FileName = csv ? "recete.csv" : "recete.dat",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (saveDlg.ShowDialog(this) != DialogResult.OK)
            return;

        var entries = _recipeItems
            .Select(i => (i.Job, i.ExportOptions))
            .ToList();

        string? error;
        bool ok = csv
            ? CsvFileExporter.TryWriteBatch(entries, saveDlg.FileName, out error)
            : DatFileExporter.TryWriteBatch(entries, saveDlg.FileName, out error);

        if (!ok)
        {
            MessageBox.Show(this,
                error ?? $"{(csv ? "CSV" : "DAT")} dosyası yazılamadı.",
                csv ? "Toplu CSV" : "Toplu DAT",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        MessageBox.Show(this,
            $"{_recipeItems.Count} şekil tek dosyada kaydedildi:\n{saveDlg.FileName}",
            csv ? "Toplu CSV" : "Toplu DAT",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void BtnRemoveRecipe_Click(object? sender, EventArgs e)
    {
        if (_lvRecipe.SelectedItems.Count == 0)
            return;

        var selected = _lvRecipe.SelectedItems[0];
        if (selected.Tag is not RecipeItem item)
            return;

        var confirm = MessageBox.Show(this,
            $"\"{item.DisplayName}\" reçeteden kaldırılsın mı?",
            "Reçete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes)
            return;

        _recipeItems.Remove(item);
        RebuildRecipeList();
        RefreshRecipeUi();
    }

    private void BtnClearRecipe_Click(object? sender, EventArgs e)
    {
        if (_recipeItems.Count == 0)
            return;

        var confirm = MessageBox.Show(this,
            $"Reçetedeki {_recipeItems.Count} şeklin tamamı silinsin mi?",
            "Reçete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes)
            return;

        _recipeItems.Clear();
        _lvRecipe.Items.Clear();
        RefreshRecipeUi();
    }

    private void AddRecipeListItem(RecipeItem item, int index)
    {
        var lvi = new ListViewItem(index.ToString())
        {
            Tag = item
        };
        lvi.SubItems.Add(item.DisplayName);
        lvi.SubItems.Add(item.EdgeCount.ToString());
        lvi.SubItems.Add(item.ExportOptions.KalinlikMm.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
        lvi.SubItems.Add(item.ExportOptions.IstenilenAdet.ToString());
        lvi.SubItems.Add(item.AddedAt.ToString("HH:mm:ss"));
        _lvRecipe.Items.Add(lvi);
        lvi.Selected = true;
        lvi.EnsureVisible();
    }

    private void RebuildRecipeList()
    {
        _lvRecipe.BeginUpdate();
        _lvRecipe.Items.Clear();
        for (int i = 0; i < _recipeItems.Count; i++)
            AddRecipeListItem(_recipeItems[i], i + 1);
        _lvRecipe.EndUpdate();
    }

    private void RefreshRecipeUi()
    {
        int count = _recipeItems.Count;
        _lblRecipeCount.Text = count == 1 ? "Reçete: 1 şekil" : $"Reçete: {count} şekil";
        _btnExportBatchCsv.Enabled = count > 0;
        _btnExportBatchDat.Enabled = count > 0;
        _btnClearRecipe.Enabled = count > 0;
        RefreshRecipeActionButtons();
    }

    private void RefreshRecipeActionButtons()
    {
        _btnRemoveRecipe.Enabled = _lvRecipe.SelectedItems.Count > 0;
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
