using netDxf;
using otomasyon.Analysis;
using otomasyon.Dxf;
using otomasyon.Geometry;
using otomasyon.Localization;
using otomasyon.Models;
using otomasyon.Models.Recipe;
using otomasyon.Models.Simulation;
using otomasyon.Rendering;
using otomasyon.Settings;
using otomasyon.Simulation;
using otomasyon.UI;

namespace otomasyon;

public partial class Form1 : Form, ILocalizable
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
        _btnSettings.Click += BtnSettings_Click;
        _lvRecipe.SelectedIndexChanged += (_, _) => RefreshRecipeActionButtons();
        _drawPanel.Paint += DrawPanel_Paint;
        LocalizationManager.LanguageChanged += OnLanguageChanged;
        AppSettingsManager.MachineDirectionChanged += OnMachineDirectionChanged;
        AppSettingsManager.SettingsChanged += OnSettingsChanged;
        ApplyLocalization();
    }

    private void OnSettingsChanged(object? sender, EventArgs e)
    {
        if (IsDisposed)
            return;

        RefreshResultsUi();
        RefreshRecipeUi();
        if (AppSettingsManager.IsConfigured && !string.IsNullOrWhiteSpace(_currentFilePath))
            ReloadCurrentScene();
    }

    private void ShowSettingsDialog()
    {
        using var dlg = new SettingsDialog();
        dlg.ShowDialog(this);
        RefreshResultsUi();
        RefreshRecipeUi();
    }

    private bool EnsureConfiguredForAction()
    {
        if (AppSettingsManager.IsConfigured)
            return true;

        MessageBox.Show(this,
            L.Get("Msg.SettingsRequired"),
            L.Get("Title.SettingsRequired"),
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);

        ShowSettingsDialog();
        return AppSettingsManager.IsConfigured;
    }

    private void BtnSettings_Click(object? sender, EventArgs e)
        => ShowSettingsDialog();

    private void OnMachineDirectionChanged(object? sender, EventArgs e)
    {
        if (IsDisposed || string.IsNullOrWhiteSpace(_currentFilePath))
            return;

        ReloadCurrentScene();
    }

    private void ReloadCurrentScene()
    {
        LoadDxfFile(_currentFilePath);
        _drawPanel.Invalidate();
    }

    private void OnLanguageChanged(object? sender, EventArgs e)
    {
        if (!IsDisposed)
            ApplyLocalization();
    }

    public void ApplyLocalization()
    {
        Text = L.Get("App.Title");
        _btnSettings.Text = L.Get("Btn.Settings");
        _btnSelectFile.Text = L.Get("Btn.SelectFile");
        _btnAddToRecipe.Text = L.Get("Btn.AddToRecipe");
        _btnSimulation.Text = L.Get("Btn.Simulation");
        _btnImportCsv.Text = L.Get("Btn.ImportCsv");
        _btnExportBatchCsv.Text = L.Get("Btn.ExportBatchCsv");
        _btnExportBatchDat.Text = L.Get("Btn.ExportBatchDat");
        _btnRemoveRecipe.Text = L.Get("Btn.RemoveSelected");
        _btnClearRecipe.Text = L.Get("Btn.ClearAll");
        _lblRecipeHeader.Text = L.Get("Label.Recipe");

        if (string.IsNullOrWhiteSpace(_currentFilePath))
            _lblFilePath.Text = L.Get("Label.NoFileSelected");

        if (_lvRecipe.Columns.Count >= 6)
        {
            _lvRecipe.Columns[0].Text = L.Get("Col.Index");
            _lvRecipe.Columns[1].Text = L.Get("Col.File");
            _lvRecipe.Columns[2].Text = L.Get("Col.Edge");
            _lvRecipe.Columns[3].Text = L.Get("Col.GlassThickness");
            _lvRecipe.Columns[4].Text = L.Get("Col.Quantity");
            _lvRecipe.Columns[5].Text = L.Get("Col.Source");
        }

        RefreshResultsUi();
        if (_scene.ContourEdges.Count > 0 || _scene.RadiusFeatures.Count > 0)
            _txtCoordinates.Text = SceneResultsTextFormatter.Format(_scene);
        RebuildRecipeList();
        RefreshRecipeUi();
        _drawPanel.Invalidate();
    }

    private void Form1_Load(object? sender, EventArgs e)
        => ApplyInitialSplitLayout();

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
        if (!EnsureConfiguredForAction())
            return;

        try
        {
            using var dlg = new OpenFileDialog
            {
                Filter = L.Get("Filter.Dxf"),
                Title = L.Get("Dialog.SelectDxf")
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
                L.F("Msg.FileOpenError", ex.Message),
                L.Get("Title.Error"),
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
                    L.Get("Msg.DxfLoadNull"),
                    L.Get("Title.Dxf"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                RefreshResultsUi();
                return;
            }

            _scene = _sceneBuilder.Build(doc);
            _txtCoordinates.Text = SceneResultsTextFormatter.Format(_scene);
            _txtCoordinates.SelectionStart = 0;
            _txtCoordinates.ScrollToCaret();

            if (!ShapeLimitsValidator.TryValidate(_scene, out string? limitMsg))
            {
                MessageBox.Show(this,
                    limitMsg ?? L.Get("Msg.ShapeLimitExceededGeneric"),
                    L.Get("Title.ShapeLimit"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }

            RefreshResultsUi();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this,
                L.F("Msg.DxfParseError", ex.Message),
                L.Get("Title.DxfReadError"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            _txtCoordinates.Clear();
            RefreshResultsUi();
        }
    }

    private void RefreshResultsUi()
    {
        var s = _scene.Statistics;
        _lblResults.Text = L.F("Status.Stats",
            s.ContourEdgeCount,
            s.RadiusFeatureCount,
            s.ArcCount,
            s.CircleCount,
            s.TrackedEntityCount);

        bool canProcess = ContourPathOrderer.HasSimulatableContour(_scene)
            && ShapeLimitsValidator.IsWithinLimits(_scene)
            && AppSettingsManager.IsConfigured;
        _btnSimulation.Enabled = canProcess;
        _btnAddToRecipe.Enabled = canProcess && !string.IsNullOrWhiteSpace(_currentFilePath);
    }

    private void BtnAddToRecipe_Click(object? sender, EventArgs e)
    {
        if (!EnsureConfiguredForAction())
            return;

        if (!TryCreateJobFromCurrentScene(
                SetupPurpose.Recipe,
                out SimulationJob? job,
                out string? error,
                out CsvFileExporter.ExportOptions? exportOptions))
        {
            if (!string.IsNullOrEmpty(error))
                MessageBox.Show(this, error, L.Get("Title.Recipe"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
        AddRecipeListItem(item, _recipeItems.Count + (_importedCsv?.LineCount ?? 0));
        RefreshRecipeUi();

        MessageBox.Show(this,
            L.F("Msg.RecipeAdded", item.DisplayName),
            L.Get("Title.Recipe"),
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void BtnSimulation_Click(object? sender, EventArgs e)
    {
        if (!EnsureConfiguredForAction())
            return;

        if (!TryCreateJobFromCurrentScene(
                SetupPurpose.Simulation,
                out SimulationJob? job,
                out string? error,
                out _))
        {
            if (!string.IsNullOrEmpty(error))
                MessageBox.Show(this, error, L.Get("Title.Simulation"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            error = L.Get("Msg.SimNeedClosedContour");
            return false;
        }

        using var setup = new SimulationSetupDialog(_scene, purpose);
        if (setup.ShowDialog(this) != DialogResult.OK ||
            setup.ThicknessByEdge is null ||
            setup.Tool is null)
            return false;

        if (purpose == SetupPurpose.Recipe && setup.CsvExportOptions is null)
        {
            error = L.Get("Msg.CsvParamsMissing");
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
            Filter = L.Get("Filter.Csv"),
            Title = L.Get("Dialog.ImportCsv"),
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
        };

        if (dlg.ShowDialog(this) != DialogResult.OK)
            return;

        if (_importedCsv is not null)
        {
            var replace = MessageBox.Show(this,
                L.F("Msg.ImportReplaceConfirm", _importedCsv.DisplayName, _importedCsv.LineCount),
                L.Get("Title.ImportCsv"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (replace != DialogResult.Yes)
                return;
        }

        if (!CsvFileImporter.TryImport(dlg.FileName, out ImportedCsvBatch batch, out string? error))
        {
            MessageBox.Show(this,
                error ?? L.Get("Title.ImportCsv"),
                L.Get("Title.ImportCsv"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }

        _importedCsv = batch;
        RebuildRecipeList();
        RefreshRecipeUi();

        MessageBox.Show(this,
            L.F("Msg.ImportSuccess", batch.LineCount),
            L.Get("Title.ImportCsv"),
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
                L.Get("Msg.NoExportData"),
                csv ? L.Get("Title.BatchCsv") : L.Get("Title.BatchDat"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        string defaultName = csv ? L.Get("File.DefaultCsv") : L.Get("File.DefaultDat");
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
            Filter = csv ? L.Get("Filter.Csv") : L.Get("Filter.Dat"),
            Title = csv ? L.Get("Dialog.SaveBatchCsv") : L.Get("Dialog.SaveBatchDat"),
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
                error ?? L.F("Msg.ExportFailed", csv ? "CSV" : "DAT"),
                csv ? L.Get("Title.BatchCsv") : L.Get("Title.BatchDat"),
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
            ? L.F("Msg.ExportSavedExistingAndNew", importedCount, newCount)
            : L.F("Msg.ExportSavedRows", writtenLines.Count);

        MessageBox.Show(this,
            L.F("Msg.ExportSuccessDetail", detail, saveDlg.FileName),
            csv ? L.Get("Title.BatchCsv") : L.Get("Title.BatchDat"),
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
                L.F("Msg.RemoveCsvRowConfirm", importedRow.GetDisplayName()),
                L.Get("Title.Recipe"),
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
            L.F("Msg.RemoveShapeConfirm", item.DisplayName),
            L.Get("Title.Recipe"),
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
                ? L.F("Msg.ClearAllWithCsv", _importedCsv!.LineCount, _recipeItems.Count)
                : L.F("Msg.ClearAllShapes", _recipeItems.Count),
            L.Get("Title.Recipe"),
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
        var lvi = new ListViewItem(row.RowIndex.ToString()) { Tag = row };
        lvi.SubItems.Add(row.GetDisplayName());
        lvi.SubItems.Add(row.EdgeCount.ToString());
        lvi.SubItems.Add(row.CamKalinlikMm.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
        lvi.SubItems.Add(row.Adet.ToString());
        lvi.SubItems.Add(L.Get("Source.Csv"));
        _lvRecipe.Items.Add(lvi);
    }

    private void AddRecipeListItem(RecipeItem item, int index)
    {
        var lvi = new ListViewItem(index.ToString()) { Tag = item };
        lvi.SubItems.Add(item.DisplayName);
        lvi.SubItems.Add(item.EdgeCount.ToString());
        lvi.SubItems.Add(item.ExportOptions.KalinlikMm.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture));
        lvi.SubItems.Add(item.ExportOptions.IstenilenAdet.ToString());
        lvi.SubItems.Add(L.Get("Source.New"));
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
            > 0 when newCount > 0 => L.F("Status.CsvAndNew", importedCount, newCount),
            > 0 => L.F("Status.CsvImportedOnly", importedCount),
            _ => newCount == 1 ? L.Get("Status.RecipeCountOne") : L.F("Status.RecipeCount", newCount)
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
                L.F("Msg.DrawError", ex.Message),
                L.Get("Title.DrawError"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

}
