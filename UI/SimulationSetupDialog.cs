using otomasyon.Localization;
using otomasyon.Models;
using otomasyon.Models.Recipe;
using otomasyon.Models.Simulation;
using otomasyon.Simulation;

namespace otomasyon.UI;

public enum SetupPurpose
{
    Simulation,
    Recipe
}

public sealed class SimulationSetupDialog : Form, ILocalizable
{
    private readonly DxfScene _scene;
    private readonly SetupPurpose _purpose;
    private readonly bool _isEditMode;
    private readonly RecipeSetupInitialValues? _initialValues;
    private readonly Panel _panelConfirm = new();
    private readonly Panel _panelParams = new();
    private readonly FlowLayoutPanel _edgeFlow = new();
    private readonly FlowLayoutPanel _ventFlow = new();
    private readonly NumericUpDown _numStone = new();
    private readonly NumericUpDown _numBindirme = new();
    private readonly NumericUpDown _numKalinlik = new();
    private readonly NumericUpDown _numAdet = new();
    private readonly Dictionary<int, NumericUpDown> _edgeThicknessInputs = new();
    private readonly Dictionary<int, NumericUpDown> _edgeOffsetInputs = new();
    private readonly Dictionary<int, NumericUpDown> _ventStrippingInputs = new();
    private readonly List<(Label Label, ContourEdge Edge)> _edgeRowLabels = new();

    private Label _lblConfirm = null!;
    private Button _btnNo = null!;
    private Button _btnYes = null!;
    private Label _lblEdgesHint = null!;
    private Label _lblVentsHint = null!;
    private Label _lblColVent = null!;
    private Label _lblColVentThickness = null!;
    private Label _lblColEdge = null!;
    private Label _lblColThickness = null!;
    private Label _lblColOffset = null!;
    private GroupBox _toolPanel = null!;
    private Label _lblStoneWidth = null!;
    private Label _lblOverlap = null!;
    private Label _lblToolHint = null!;
    private GroupBox? _exportPanel;
    private Label? _lblGlassThickness;
    private Label? _lblDesiredQty;
    private Button _btnCancel = null!;
    private Button _btnBack = null!;
    private Button _btnOk = null!;

    public IReadOnlyDictionary<int, double>? ThicknessByEdge { get; private set; }
    public IReadOnlyDictionary<int, double>? OffsetByEdge { get; private set; }
    public IReadOnlyDictionary<int, double>? VentStrippingByIndex { get; private set; }
    public StoneToolSettings? Tool { get; private set; }
    public CsvFileExporter.ExportOptions? CsvExportOptions { get; private set; }

    public SimulationSetupDialog(
        DxfScene scene,
        SetupPurpose purpose = SetupPurpose.Simulation,
        RecipeSetupInitialValues? initialValues = null,
        bool isEditMode = false)
    {
        _scene = scene;
        _purpose = purpose;
        _isEditMode = isEditMode;
        _initialValues = initialValues;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = purpose == SetupPurpose.Recipe ? new Size(480, 640) : new Size(480, 560);
        MinimumSize = new Size(400, 400);

        BuildConfirmStep();
        BuildParamsStep();

        LocalizationManager.LanguageChanged += (_, _) => { if (!IsDisposed) ApplyLocalization(); };
        ApplyLocalization();

        if (purpose == SetupPurpose.Recipe)
            ShowParamsStep();
        else
            ShowConfirmStep();

        if (_initialValues is not null)
            ApplyInitialValues(_initialValues);
    }

    private void ApplyInitialValues(RecipeSetupInitialValues initial)
    {
        foreach (var kv in initial.ThicknessByEdge)
        {
            if (_edgeThicknessInputs.TryGetValue(kv.Key, out var num))
                num.Value = ClampDecimal(num, kv.Value);
        }

        foreach (var kv in initial.OffsetByEdge)
        {
            if (_edgeOffsetInputs.TryGetValue(kv.Key, out var num))
                num.Value = ClampDecimal(num, kv.Value);
        }

        foreach (var kv in initial.VentStrippingByIndex)
        {
            if (_ventStrippingInputs.TryGetValue(kv.Key, out var num))
                num.Value = ClampDecimal(num, kv.Value);
        }

        _numStone.Value = ClampDecimal(_numStone, initial.Tool.StoneWidthMm);
        _numBindirme.Value = ClampDecimal(_numBindirme, initial.Tool.BindirmeMm);

        if (_purpose == SetupPurpose.Recipe)
        {
            _numKalinlik.Value = ClampDecimal(_numKalinlik, initial.ExportOptions.KalinlikMm);
            _numAdet.Value = Math.Clamp(initial.ExportOptions.IstenilenAdet, (int)_numAdet.Minimum, (int)_numAdet.Maximum);
        }
    }

    private static decimal ClampDecimal(NumericUpDown control, double value)
    {
        decimal d = (decimal)value;
        if (d < control.Minimum)
            return control.Minimum;
        if (d > control.Maximum)
            return control.Maximum;
        return d;
    }

    public void ApplyLocalization()
    {
        Text = _isEditMode
            ? L.Get("Dialog.EditRecipe")
            : _purpose == SetupPurpose.Recipe
                ? L.Get("Dialog.SetupRecipe")
                : L.Get("Dialog.SetupSimulation");

        _lblConfirm.Text = L.Get("Setup.ConfirmText");
        _btnNo.Text = L.Get("Btn.No");
        _btnYes.Text = L.Get("Btn.Yes");
        _lblEdgesHint.Text = L.Get("Setup.EdgesHint");
        if (_lblVentsHint is not null)
            _lblVentsHint.Text = L.Get("Setup.VentsHint");
        if (_lblColVent is not null)
            _lblColVent.Text = L.Get("Setup.ColVent");
        if (_lblColVentThickness is not null)
            _lblColVentThickness.Text = L.Get("Setup.ColThickness");
        _lblColEdge.Text = L.Get("Setup.ColEdge");
        _lblColThickness.Text = L.Get("Setup.ColThickness");
        _lblColOffset.Text = L.Get("Setup.ColOffset");
        _toolPanel.Text = L.Get("Setup.ToolGroup");
        _lblStoneWidth.Text = L.Get("Setup.StoneWidth");
        _lblOverlap.Text = L.Get("Setup.Overlap");
        _lblToolHint.Text = L.Get("Setup.ToolHint");
        _btnCancel.Text = L.Get("Btn.Cancel");
        _btnBack.Text = L.Get("Btn.Back");
        _btnOk.Text = _isEditMode
            ? L.Get("Btn.SaveRecipeChanges")
            : _purpose == SetupPurpose.Recipe
                ? L.Get("Btn.AddToRecipe")
                : L.Get("Btn.StartSimulation");

        if (_exportPanel is not null)
            _exportPanel.Text = L.Get("Setup.CsvGroup");
        if (_lblGlassThickness is not null)
            _lblGlassThickness.Text = L.Get("Setup.GlassThickness");
        if (_lblDesiredQty is not null)
            _lblDesiredQty.Text = L.Get("Setup.DesiredQty");

        RefreshEdgeRowLabels();
    }

    private void RefreshEdgeRowLabels()
    {
        foreach (var (label, edge) in _edgeRowLabels)
        {
            label.Text = edge.IsRadiusSegment && edge.RadiusIndex is int ri
                ? L.F("Setup.EdgeLabelRadius", edge.Index, edge.CornerIndex, ri)
                : L.F("Setup.EdgeLabel", edge.Index, edge.CornerIndex);
        }
    }

    private void BuildConfirmStep()
    {
        _panelConfirm.Dock = DockStyle.Fill;
        _lblConfirm = new Label
        {
            Dock = DockStyle.Top,
            Height = 120,
            Padding = new Padding(12),
            Font = new Font("Segoe UI", 10f)
        };

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(12)
        };

        _btnNo = new Button { DialogResult = DialogResult.Cancel, Width = 100 };
        _btnYes = new Button { Width = 120 };
        _btnYes.Click += (_, _) => ShowParamsStep();

        flow.Controls.Add(_btnNo);
        flow.Controls.Add(_btnYes);
        _panelConfirm.Controls.Add(flow);
        _panelConfirm.Controls.Add(_lblConfirm);
    }

    private void BuildParamsStep()
    {
        _panelParams.Dock = DockStyle.Fill;
        _panelParams.Visible = false;

        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        var inner = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 1,
            Padding = new Padding(12)
        };

        _lblEdgesHint = new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        };
        inner.Controls.Add(_lblEdgesHint);

        var header = new Panel { Width = 400, Height = 22 };
        _lblColEdge = new Label { Location = new Point(0, 4), AutoSize = true, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) };
        _lblColThickness = new Label { Location = new Point(150, 4), AutoSize = true, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) };
        _lblColOffset = new Label { Location = new Point(280, 4), AutoSize = true, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) };
        header.Controls.Add(_lblColEdge);
        header.Controls.Add(_lblColThickness);
        header.Controls.Add(_lblColOffset);
        inner.Controls.Add(header);

        _edgeFlow.FlowDirection = FlowDirection.TopDown;
        _edgeFlow.AutoSize = true;
        _edgeFlow.WrapContents = false;
        _edgeFlow.Width = 420;

        foreach (var edge in _scene.ContourEdges)
        {
            var row = new Panel { Width = 400, Height = 36 };
            var lblEdge = new Label { Location = new Point(0, 8), AutoSize = true };
            _edgeRowLabels.Add((lblEdge, edge));
            row.Controls.Add(lblEdge);

            var num = new NumericUpDown
            {
                Location = new Point(150, 4),
                Width = 110,
                DecimalPlaces = 2,
                Maximum = 99999,
                Minimum = 0,
                Value = 10
            };
            _edgeThicknessInputs[edge.Index] = num;
            row.Controls.Add(num);

            var numOffset = new NumericUpDown
            {
                Location = new Point(280, 4),
                Width = 110,
                DecimalPlaces = 2,
                Maximum = 99999,
                Minimum = 0,
                Value = 0
            };
            _edgeOffsetInputs[edge.Index] = numOffset;
            row.Controls.Add(numOffset);

            _edgeFlow.Controls.Add(row);
        }

        inner.Controls.Add(_edgeFlow);

        if (_scene.VentFeatures.Count > 0)
        {
            _lblVentsHint = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Margin = new Padding(0, 12, 0, 8)
            };
            inner.Controls.Add(_lblVentsHint);

            var ventHeader = new Panel { Width = 400, Height = 22 };
            _lblColVent = new Label { Location = new Point(0, 4), AutoSize = true, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) };
            _lblColVentThickness = new Label { Location = new Point(200, 4), AutoSize = true, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold) };
            ventHeader.Controls.Add(_lblColVent);
            ventHeader.Controls.Add(_lblColVentThickness);
            inner.Controls.Add(ventHeader);

            _ventFlow.FlowDirection = FlowDirection.TopDown;
            _ventFlow.AutoSize = true;
            _ventFlow.WrapContents = false;
            _ventFlow.Width = 420;

            foreach (var vent in _scene.VentFeatures.OrderBy(v => v.Index))
            {
                var row = new Panel { Width = 400, Height = 36 };
                var lblVent = new Label
                {
                    Location = new Point(0, 8),
                    AutoSize = true,
                    Text = L.F("Setup.VentLabel", vent.Index)
                };
                row.Controls.Add(lblVent);

                var numVent = new NumericUpDown
                {
                    Location = new Point(200, 4),
                    Width = 110,
                    DecimalPlaces = 2,
                    Maximum = 99999,
                    Minimum = 0,
                    Value = 10
                };
                _ventStrippingInputs[vent.Index] = numVent;
                row.Controls.Add(numVent);
                _ventFlow.Controls.Add(row);
            }

            inner.Controls.Add(_ventFlow);
        }

        _toolPanel = new GroupBox
        {
            Dock = DockStyle.Top,
            Height = 110,
            Padding = new Padding(12),
            Margin = new Padding(0, 16, 0, 0)
        };

        _lblStoneWidth = new Label { Location = new Point(16, 28), AutoSize = true };
        _numStone.Location = new Point(200, 24);
        _numStone.Width = 120;
        _numStone.DecimalPlaces = 2;
        _numStone.Minimum = 0.01m;
        _numStone.Maximum = 99999;
        _numStone.Value = 10;
        _toolPanel.Controls.Add(_lblStoneWidth);
        _toolPanel.Controls.Add(_numStone);

        _lblOverlap = new Label { Location = new Point(16, 58), AutoSize = true };
        _numBindirme.Location = new Point(200, 54);
        _numBindirme.Width = 120;
        _numBindirme.DecimalPlaces = 2;
        _numBindirme.Minimum = 0;
        _numBindirme.Maximum = 99998;
        _numBindirme.Value = 2;
        _toolPanel.Controls.Add(_lblOverlap);
        _toolPanel.Controls.Add(_numBindirme);

        _lblToolHint = new Label
        {
            Location = new Point(16, 82),
            Size = new Size(420, 32),
            ForeColor = Color.DimGray,
            Font = new Font("Segoe UI", 8f)
        };
        _toolPanel.Controls.Add(_lblToolHint);

        inner.Controls.Add(_toolPanel);

        if (_purpose == SetupPurpose.Recipe)
        {
            var defaults = CsvFileExporter.CreateDefaultOptions();
            _exportPanel = new GroupBox
            {
                Dock = DockStyle.Top,
                Height = 88,
                Padding = new Padding(12),
                Margin = new Padding(0, 16, 0, 0)
            };

            _lblGlassThickness = new Label { Location = new Point(16, 28), AutoSize = true };
            _numKalinlik.Location = new Point(200, 24);
            _numKalinlik.Width = 120;
            _numKalinlik.DecimalPlaces = 2;
            _numKalinlik.Minimum = 1;
            _numKalinlik.Maximum = 99999;
            _numKalinlik.Value = (decimal)defaults.KalinlikMm;
            _exportPanel.Controls.Add(_lblGlassThickness);
            _exportPanel.Controls.Add(_numKalinlik);

            _lblDesiredQty = new Label { Location = new Point(16, 56), AutoSize = true };
            _numAdet.Location = new Point(200, 52);
            _numAdet.Width = 120;
            _numAdet.Minimum = 1;
            _numAdet.Maximum = 99999;
            _numAdet.Value = defaults.IstenilenAdet;
            _exportPanel.Controls.Add(_lblDesiredQty);
            _exportPanel.Controls.Add(_numAdet);

            inner.Controls.Add(_exportPanel);
        }

        scroll.Controls.Add(inner);

        var bottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(12)
        };

        _btnCancel = new Button { DialogResult = DialogResult.Cancel, Width = 90 };
        _btnBack = new Button { Width = 90 };
        _btnBack.Click += (_, _) => ShowConfirmStep();
        _btnBack.Visible = _purpose == SetupPurpose.Simulation;
        _btnOk = new Button { Width = 150, DialogResult = DialogResult.None };
        _btnOk.Click += OnStartClick;

        bottom.Controls.Add(_btnCancel);
        bottom.Controls.Add(_btnOk);
        bottom.Controls.Add(_btnBack);

        _panelParams.Controls.Add(scroll);
        _panelParams.Controls.Add(bottom);
    }

    private void ShowConfirmStep()
    {
        Controls.Clear();
        Controls.Add(_panelConfirm);
        _panelConfirm.Visible = true;
        _panelParams.Visible = false;
    }

    private void ShowParamsStep()
    {
        Controls.Clear();
        Controls.Add(_panelParams);
        _panelParams.Visible = true;
    }

    private void OnStartClick(object? sender, EventArgs e)
    {
        var dict = new Dictionary<int, double>();
        foreach (var kv in _edgeThicknessInputs)
            dict[kv.Key] = (double)kv.Value.Value;

        var offsets = new Dictionary<int, double>();
        foreach (var kv in _edgeOffsetInputs)
            offsets[kv.Key] = (double)kv.Value.Value;

        var ventStripping = new Dictionary<int, double>();
        foreach (var kv in _ventStrippingInputs)
            ventStripping[kv.Key] = (double)kv.Value.Value;

        var tool = new StoneToolSettings
        {
            StoneWidthMm = (double)_numStone.Value,
            BindirmeMm = (double)_numBindirme.Value
        };

        try
        {
            tool.Validate();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, L.Get("Title.ParamError"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        ThicknessByEdge = dict;
        OffsetByEdge = offsets;
        VentStrippingByIndex = ventStripping;
        Tool = tool;

        if (_purpose == SetupPurpose.Recipe)
        {
            CsvExportOptions = new CsvFileExporter.ExportOptions
            {
                KalinlikMm = (double)_numKalinlik.Value,
                IstenilenAdet = (int)_numAdet.Value,
                VentStrippingByIndex = ventStripping
            };
        }

        DialogResult = DialogResult.OK;
    }
}
