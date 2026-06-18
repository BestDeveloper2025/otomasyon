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
    private ImportedCsvBatch? _importedCsv;

    private DxfScene _scene = DxfScene.Empty;
    private string _currentFilePath = string.Empty;

    public Form1()
    {
        InitializeComponent();
        _btnSelectFile.Click += BtnSelectFile_Click;
        _btnAddToRecipe.Click += BtnAddToRecipe_Click;
        _btnSimulation.Click += BtnSimulation_Click;
        _btnImportCsv.Click += BtnImportCsv_Click;
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

    private void BtnImportCsv_Click(object? sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Filter = "CSV Dosyası (*.csv)|*.csv",
            Title = "Mevcut CSV dosyasını içe aktar",
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (dlg.ShowDialog(this) != DialogResult.OK)
            return;

        if (_importedCsv is not null)
        {
            var replace = MessageBox.Show(this,
                $"Zaten içe aktarılmış bir CSV var ({_importedCsv.DisplayName}, {_importedCsv.LineCount} satır).\n" +
                "Yeni dosya ile değiştirilsin mi?",
                "CSV İçe Aktar",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (replace != DialogResult.Yes)
                return;
        }

        if (!CsvFileImporter.TryImport(dlg.FileName, out ImportedCsvBatch batch, out string? error))
        {
            MessageBox.Show(this,
                error ?? "CSV dosyası okunamadı.",
                "CSV İçe Aktar",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        _importedCsv = batch;
        RebuildRecipeList();
        RefreshRecipeUi();

        MessageBox.Show(this,
            $"{batch.LineCount} satır içe aktarıldı.\n" +
            "Yeni şekiller ekleyip çıktı aldığınızda bu satırların ardına yazılır.",
            "CSV İçe Aktar",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void BtnExportBatchCsv_Click(object? sender, EventArgs e)
        => ExportRecipeBatch(csv: true);

    private void BtnExportBatchDat_Click(object? sender, EventArgs e)
        => ExportRecipeBatch(csv: false);

    private void ExportRecipeBatch(bool csv)
    {
        bool hasImported = _importedCsv is not null;
        bool hasNew = _recipeItems.Count > 0;

        if (!hasImported && !hasNew)
        {
            MessageBox.Show(this,
                "Kaydedilecek veri yok. CSV içe aktarın veya reçeteye şekil ekleyin.",
                csv ? "Toplu CSV" : "Toplu DAT",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        string defaultName = csv ? "recete.csv" : "recete.dat";
        string? initialDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        if (_importedCsv is not null)
        {
            defaultName = Path.ChangeExtension(_importedCsv.DisplayName, csv ? ".csv" : ".dat");
            string? dir = Path.GetDirectoryName(_importedCsv.SourceFilePath);
            if (!string.IsNullOrEmpty(dir))
                initialDir = dir;
        }

        using var saveDlg = new SaveFileDialog
        {
            Filter = csv ? "CSV Dosyası (*.csv)|*.csv" : "DAT Dosyası (*.dat)|*.dat",
            Title = csv ? "Toplu CSV dosyasını kaydet" : "Toplu DAT dosyasını kaydet",
            FileName = defaultName,
            InitialDirectory = initialDir
        };

        if (saveDlg.ShowDialog(this) != DialogResult.OK)
            return;

        var entries = _recipeItems
            .Select(i => (i.Job, i.ExportOptions))
            .ToList();

        IReadOnlyList<string>? prefix = _importedCsv?.Lines;
        string? error;
        IReadOnlyList<string> writtenLines;
        bool ok = csv
            ? CsvFileExporter.TryWriteBatch(prefix, entries, saveDlg.FileName, out writtenLines, out error)
            : DatFileExporter.TryWriteBatch(prefix, entries, saveDlg.FileName, out writtenLines, out error);

        if (!ok)
        {
            MessageBox.Show(this,
                error ?? $"{(csv ? "CSV" : "DAT")} dosyası yazılamadı.",
                csv ? "Toplu CSV" : "Toplu DAT",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        int importedCount = _importedCsv?.LineCount ?? 0;
        int newCount = _recipeItems.Count;
        _importedCsv = CsvFileImporter.CreateBatchFromLines(saveDlg.FileName, writtenLines);
        if (_importedCsv.Rows.Count == 0)
            _importedCsv = null;
        _recipeItems.Clear();
        RebuildRecipeList();
        RefreshRecipeUi();

        string detail = newCount > 0
            ? $"{importedCount} mevcut satır + {newCount} yeni şekil kaydedildi."
            : $"{writtenLines.Count} satır kaydedildi.";

        MessageBox.Show(this,
            $"{detail}\n{saveDlg.FileName}",
            csv ? "Toplu CSV" : "Toplu DAT",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void BtnRemoveRecipe_Click(object? sender, EventArgs e)
    {
        if (_lvRecipe.SelectedItems.Count == 0)
            return;

        var selected = _lvRecipe.SelectedItems[0];
        if (selected.Tag is ImportedCsvRow importedRow)
        {
            var confirmImport = MessageBox.Show(this,
                $"Bu CSV satırı kaldırılsın mı?\n{importedRow.DisplayName}",
                "Reçete",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmImport != DialogResult.Yes)
                return;

            _importedCsv?.Rows.Remove(importedRow);
            if (_importedCsv is { Rows.Count: 0 })
                _importedCsv = null;

            RebuildRecipeList();
            RefreshRecipeUi();
            return;
        }

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
        bool hasImported = _importedCsv is not null;
        bool hasNew = _recipeItems.Count > 0;
        if (!hasImported && !hasNew)
            return;

        var confirm = MessageBox.Show(this,
            hasImported
                ? $"İçe aktarılan CSV ({_importedCsv!.LineCount} satır) ve reçetedeki {_recipeItems.Count} yeni şekil temizlensin mi?"
                : $"Reçetedeki {_recipeItems.Count} şeklin tamamı silinsin mi?",
            "Reçete",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes)
            return;

        _importedCsv = null;
        _recipeItems.Clear();
        _lvRecipe.Items.Clear();
        RefreshRecipeUi();
    }

    private void AddImportedRowListItem(ImportedCsvRow row)
    {
        var lvi = new ListViewItem(row.RowIndex.ToString())
        {
            Tag = row
        };
        lvi.SubItems.Add(row.DisplayName);
        lvi.SubItems.Add(row.EdgeCount.ToString());
        lvi.SubItems.Add(row.CamKalinlikMm.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
        lvi.SubItems.Add(row.Adet.ToString());
        lvi.SubItems.Add("CSV");
        _lvRecipe.Items.Add(lvi);
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
        lvi.SubItems.Add("Yeni");
        _lvRecipe.Items.Add(lvi);
        lvi.Selected = true;
        lvi.EnsureVisible();
    }

    private void RebuildRecipeList()
    {
        _lvRecipe.BeginUpdate();
        _lvRecipe.Items.Clear();

        int rowBase = 0;
        if (_importedCsv is not null)
        {
            foreach (var row in _importedCsv.Rows)
                AddImportedRowListItem(row);
            rowBase = _importedCsv.LineCount;
        }

        for (int i = 0; i < _recipeItems.Count; i++)
            AddRecipeListItem(_recipeItems[i], rowBase + i + 1);

        _lvRecipe.EndUpdate();
    }

    private void RefreshRecipeUi()
    {
        int newCount = _recipeItems.Count;
        int importedCount = _importedCsv?.LineCount ?? 0;

        _lblRecipeCount.Text = importedCount switch
        {
            > 0 when newCount > 0 => $"CSV: {importedCount} satır + {newCount} yeni şekil",
            > 0 => $"CSV: {importedCount} satır (içe aktarım)",
            _ => newCount == 1 ? "Reçete: 1 şekil" : $"Reçete: {newCount} şekil"
        };

        bool hasData = importedCount > 0 || newCount > 0;
        _btnExportBatchCsv.Enabled = hasData;
        _btnExportBatchDat.Enabled = hasData;
        _btnClearRecipe.Enabled = hasData;
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
