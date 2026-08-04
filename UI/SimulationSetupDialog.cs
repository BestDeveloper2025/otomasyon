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
    private const int ColThicknessX = 150;
    private const int ColOffsetX = 290;
    private const int InputWidth = 110;

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
    private readonly NumericUpDown _numApplyThickness = new();
    private readonly Dictionary<int, NumericUpDown> _edgeThicknessInputs = new();
    private readonly Dictionary<int, NumericUpDown> _edgeOffsetInputs = new();
    private readonly Dictionary<int, NumericUpDown> _ventStrippingInputs = new();
    private readonly List<(Label Label, ContourEdge Edge)> _edgeRowLabels = new();

    private Label _lblConfirm = null!;
    private Button _btnNo = null!;
    private Button _btnYes = null!;
    private Label? _lblRecipeHint;
    private Label _lblEdgesHint = null!;
    private Label _lblEdgesSubHint = null!;
    private Label? _lblVentsHint;
    private Label? _lblColVent;
    private Label? _lblColVentThickness;
    private Label _lblColEdge = null!;
    private Label _lblColThickness = null!;
    private Label _lblColOffset = null!;
    private Button _btnApplyThickness = null!;
    private Label _lblToolTitle = null!;
    private Label _lblStoneWidth = null!;
    private Label _lblOverlap = null!;
    private Label _lblToolHint = null!;
    private Label? _lblExportTitle;
    private Label? _lblGlassThickness;
    private Label? _lblDesiredQty;
    private Panel? _exportCard;
    private Panel _toolCard = null!;
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
        ClientSize = purpose == SetupPurpose.Recipe ? new Size(520, 680) : new Size(520, 600);
        MinimumSize = new Size(460, 420);
        UiStyles.ApplyDialogChrome(this);

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
        if (_lblRecipeHint is not null)
            _lblRecipeHint.Text = L.Get("Setup.RecipeHint");
        _lblEdgesHint.Text = L.Get("Setup.EdgesHint");
        _lblEdgesSubHint.Text = L.Get("Setup.EdgesSubHint");
        _btnApplyThickness.Text = L.Get("Setup.ApplyThicknessToAll");
        if (_lblVentsHint is not null)
            _lblVentsHint.Text = L.Get("Setup.VentsHint");
        if (_lblColVent is not null)
            _lblColVent.Text = L.Get("Setup.ColVent");
        if (_lblColVentThickness is not null)
            _lblColVentThickness.Text = L.Get("Setup.ColThickness");
        _lblColEdge.Text = L.Get("Setup.ColEdge");
        _lblColThickness.Text = L.Get("Setup.ColThickness");
        _lblColOffset.Text = L.Get("Setup.ColOffset");
        _lblToolTitle.Text = L.Get("Setup.ToolGroup");
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

        if (_lblExportTitle is not null)
            _lblExportTitle.Text = L.Get("Setup.CsvGroup");
        if (_lblGlassThickness is not null)
            _lblGlassThickness.Text = L.Get("Setup.GlassThickness");
        if (_lblDesiredQty is not null)
            _lblDesiredQty.Text = L.Get("Setup.DesiredQty");

        int applyW = Math.Max(150, TextRenderer.MeasureText(_btnApplyThickness.Text, _btnApplyThickness.Font).Width + 24);
        _btnApplyThickness.Width = applyW;
        DialogUiHelper.ConfigurePrimaryButton(_btnOk, Math.Max(150, TextRenderer.MeasureText(_btnOk.Text, _btnOk.Font).Width + 28));
        DialogUiHelper.ConfigureButton(_btnCancel, Math.Max(90, TextRenderer.MeasureText(_btnCancel.Text, _btnCancel.Font).Width + 24));
        DialogUiHelper.ConfigureButton(_btnBack, Math.Max(90, TextRenderer.MeasureText(_btnBack.Text, _btnBack.Font).Width + 24));
        DialogUiHelper.ConfigurePrimaryButton(_btnYes, Math.Max(120, TextRenderer.MeasureText(_btnYes.Text, _btnYes.Font).Width + 28));
        DialogUiHelper.ConfigureButton(_btnNo, Math.Max(100, TextRenderer.MeasureText(_btnNo.Text, _btnNo.Font).Width + 24));

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
        _panelConfirm.BackColor = UiStyles.DialogBack;

        _lblConfirm = new Label
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            Font = UiStyles.FontSubtitle,
            ForeColor = UiStyles.TextPrimary
        };

        var flow = DialogUiHelper.CreateBottomButtonBar();

        _btnNo = new Button { DialogResult = DialogResult.Cancel };
        DialogUiHelper.ConfigureButton(_btnNo, 100);
        _btnYes = new Button();
        DialogUiHelper.ConfigurePrimaryButton(_btnYes, 120);
        _btnYes.Click += (_, _) => ShowParamsStep();

        flow.Controls.Add(_btnNo);
        flow.Controls.Add(_btnYes);
        _panelConfirm.Controls.Add(_lblConfirm);
        _panelConfirm.Controls.Add(flow);
    }

    private void BuildParamsStep()
    {
        _panelParams.Dock = DockStyle.Fill;
        _panelParams.Visible = false;
        _panelParams.BackColor = UiStyles.DialogBack;

        var scroll = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            BackColor = UiStyles.DialogBack,
            Padding = new Padding(12, 12, 12, 4)
        };

        // TableLayoutPanel AutoSize + Dock.Top WinForms'ta güvenilir çalışır
        var inner = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            GrowStyle = TableLayoutPanelGrowStyle.AddRows,
            Padding = new Padding(0),
            BackColor = UiStyles.DialogBack
        };
        inner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));

        if (_purpose == SetupPurpose.Recipe)
        {
            _lblRecipeHint = new Label
            {
                AutoSize = true,
                MaximumSize = new Size(470, 0),
                Margin = new Padding(0, 0, 0, 10)
            };
            UiStyles.ApplyHintLabel(_lblRecipeHint);
            inner.Controls.Add(_lblRecipeHint);

            _exportCard = BuildExportCard();
            inner.Controls.Add(_exportCard);
        }

        inner.Controls.Add(BuildEdgesCard());

        if (_scene.VentFeatures.Count > 0)
            inner.Controls.Add(BuildVentsCard());

        _toolCard = BuildToolCard();
        inner.Controls.Add(_toolCard);

        scroll.Controls.Add(inner);

        var bottom = DialogUiHelper.CreateBottomButtonBar();

        _btnCancel = new Button { DialogResult = DialogResult.Cancel };
        DialogUiHelper.ConfigureButton(_btnCancel, 90);
        _btnBack = new Button();
        DialogUiHelper.ConfigureButton(_btnBack, 90);
        _btnBack.Click += (_, _) => ShowConfirmStep();
        _btnBack.Visible = _purpose == SetupPurpose.Simulation;
        _btnOk = new Button { DialogResult = DialogResult.None };
        DialogUiHelper.ConfigurePrimaryButton(_btnOk, 150);
        _btnOk.Click += OnStartClick;

        bottom.Controls.Add(_btnCancel);
        bottom.Controls.Add(_btnOk);
        bottom.Controls.Add(_btnBack);

        // Önce Fill, sonra Bottom: Bottom son eklenince kenara oturur
        _panelParams.Controls.Add(scroll);
        _panelParams.Controls.Add(bottom);
    }

    private Panel BuildExportCard()
    {
        var card = CreateCardPanel(120);

        _lblExportTitle = new Label
        {
            Location = new Point(12, 10),
            AutoSize = true,
            Font = UiStyles.FontHeader,
            ForeColor = UiStyles.SectionHeader
        };
        card.Controls.Add(_lblExportTitle);

        var defaults = CsvFileExporter.CreateDefaultOptions();

        _lblGlassThickness = new Label { Location = new Point(12, 42), AutoSize = true };
        _numKalinlik.Location = new Point(220, 38);
        _numKalinlik.Width = 140;
        _numKalinlik.DecimalPlaces = 2;
        _numKalinlik.Minimum = 1;
        _numKalinlik.Maximum = 99999;
        _numKalinlik.Value = (decimal)defaults.KalinlikMm;
        _numKalinlik.Font = UiStyles.FontUi;
        card.Controls.Add(_lblGlassThickness);
        card.Controls.Add(_numKalinlik);

        _lblDesiredQty = new Label { Location = new Point(12, 78), AutoSize = true };
        _numAdet.Location = new Point(220, 74);
        _numAdet.Width = 140;
        _numAdet.DecimalPlaces = 0;
        _numAdet.Minimum = 1;
        _numAdet.Maximum = 99999;
        _numAdet.Value = defaults.IstenilenAdet;
        _numAdet.Font = UiStyles.FontUi;
        card.Controls.Add(_lblDesiredQty);
        card.Controls.Add(_numAdet);

        return card;
    }

    private Panel BuildEdgesCard()
    {
        int edgeCount = Math.Max(1, _scene.ContourEdges.Count);
        int cardHeight = 118 + edgeCount * 36;
        var card = CreateCardPanel(cardHeight);

        _lblEdgesHint = new Label
        {
            Location = new Point(12, 10),
            AutoSize = true,
            Font = UiStyles.FontHeader,
            ForeColor = UiStyles.SectionHeader
        };
        card.Controls.Add(_lblEdgesHint);

        _lblEdgesSubHint = new Label
        {
            Location = new Point(12, 34),
            AutoSize = false,
            Size = new Size(450, 20),
            Font = UiStyles.FontUi,
            ForeColor = UiStyles.TextMuted
        };
        card.Controls.Add(_lblEdgesSubHint);

        _numApplyThickness.Location = new Point(12, 58);
        _numApplyThickness.Width = InputWidth;
        _numApplyThickness.DecimalPlaces = 2;
        _numApplyThickness.Minimum = 0;
        _numApplyThickness.Maximum = 99999;
        _numApplyThickness.Value = 10;
        _numApplyThickness.Font = UiStyles.FontUi;
        card.Controls.Add(_numApplyThickness);

        _btnApplyThickness = new Button { Location = new Point(130, 56) };
        UiStyles.ConfigureSmallButton(_btnApplyThickness, 170);
        _btnApplyThickness.Click += (_, _) => ApplyThicknessToAll();
        card.Controls.Add(_btnApplyThickness);

        var header = new Panel { Location = new Point(12, 92), Size = new Size(460, 22) };
        _lblColEdge = new Label { Location = new Point(0, 4), AutoSize = true, Font = UiStyles.FontSmallBold, ForeColor = UiStyles.TextMuted };
        _lblColThickness = new Label { Location = new Point(ColThicknessX, 4), AutoSize = true, Font = UiStyles.FontSmallBold, ForeColor = UiStyles.TextMuted };
        _lblColOffset = new Label { Location = new Point(ColOffsetX, 4), AutoSize = true, Font = UiStyles.FontSmallBold, ForeColor = UiStyles.TextMuted };
        header.Controls.Add(_lblColEdge);
        header.Controls.Add(_lblColThickness);
        header.Controls.Add(_lblColOffset);
        card.Controls.Add(header);

        _edgeFlow.Location = new Point(12, 114);
        _edgeFlow.FlowDirection = FlowDirection.TopDown;
        _edgeFlow.AutoSize = true;
        _edgeFlow.WrapContents = false;
        _edgeFlow.Width = 460;

        int rowIndex = 0;
        foreach (var edge in _scene.ContourEdges)
        {
            var row = CreateDataRow(rowIndex++);
            var lblEdge = new Label
            {
                Location = new Point(4, 8),
                AutoSize = false,
                Size = new Size(130, 20),
                Font = UiStyles.FontUi,
                ForeColor = UiStyles.TextPrimary
            };
            _edgeRowLabels.Add((lblEdge, edge));
            row.Controls.Add(lblEdge);

            var num = CreateRowNumeric(ColThicknessX, 10);
            _edgeThicknessInputs[edge.Index] = num;
            row.Controls.Add(num);

            var numOffset = CreateRowNumeric(ColOffsetX, 0);
            _edgeOffsetInputs[edge.Index] = numOffset;
            row.Controls.Add(numOffset);

            _edgeFlow.Controls.Add(row);
        }

        card.Controls.Add(_edgeFlow);
        return card;
    }

    private Panel BuildVentsCard()
    {
        int ventCount = Math.Max(1, _scene.VentFeatures.Count);
        int cardHeight = 70 + ventCount * 36;
        var card = CreateCardPanel(cardHeight);

        _lblVentsHint = new Label
        {
            Location = new Point(12, 10),
            AutoSize = true,
            Font = UiStyles.FontHeader,
            ForeColor = UiStyles.SectionHeader
        };
        card.Controls.Add(_lblVentsHint);

        var ventHeader = new Panel { Location = new Point(12, 38), Size = new Size(460, 22) };
        _lblColVent = new Label { Location = new Point(0, 4), AutoSize = true, Font = UiStyles.FontSmallBold, ForeColor = UiStyles.TextMuted };
        _lblColVentThickness = new Label { Location = new Point(ColThicknessX, 4), AutoSize = true, Font = UiStyles.FontSmallBold, ForeColor = UiStyles.TextMuted };
        ventHeader.Controls.Add(_lblColVent);
        ventHeader.Controls.Add(_lblColVentThickness);
        card.Controls.Add(ventHeader);

        _ventFlow.Location = new Point(12, 60);
        _ventFlow.FlowDirection = FlowDirection.TopDown;
        _ventFlow.AutoSize = true;
        _ventFlow.WrapContents = false;
        _ventFlow.Width = 460;

        int rowIndex = 0;
        foreach (var vent in _scene.VentFeatures.OrderBy(v => v.Index))
        {
            var row = CreateDataRow(rowIndex++);
            var lblVent = new Label
            {
                Location = new Point(4, 8),
                AutoSize = false,
                Size = new Size(130, 20),
                Font = UiStyles.FontUi,
                ForeColor = UiStyles.TextPrimary,
                Text = L.F("Setup.VentLabel", vent.Index)
            };
            row.Controls.Add(lblVent);

            var numVent = CreateRowNumeric(ColThicknessX, 10);
            _ventStrippingInputs[vent.Index] = numVent;
            row.Controls.Add(numVent);
            _ventFlow.Controls.Add(row);
        }

        card.Controls.Add(_ventFlow);
        return card;
    }

    private Panel BuildToolCard()
    {
        var card = CreateCardPanel(140);

        _lblToolTitle = new Label
        {
            Location = new Point(12, 10),
            AutoSize = true,
            Font = UiStyles.FontHeader,
            ForeColor = UiStyles.SectionHeader
        };
        card.Controls.Add(_lblToolTitle);

        _lblStoneWidth = new Label { Location = new Point(12, 42), AutoSize = true };
        _numStone.Location = new Point(220, 38);
        _numStone.Width = 140;
        _numStone.DecimalPlaces = 2;
        _numStone.Minimum = 0.01m;
        _numStone.Maximum = 99999;
        _numStone.Value = 10;
        _numStone.Font = UiStyles.FontUi;
        card.Controls.Add(_lblStoneWidth);
        card.Controls.Add(_numStone);

        _lblOverlap = new Label { Location = new Point(12, 78), AutoSize = true };
        _numBindirme.Location = new Point(220, 74);
        _numBindirme.Width = 140;
        _numBindirme.DecimalPlaces = 2;
        _numBindirme.Minimum = 0;
        _numBindirme.Maximum = 99998;
        _numBindirme.Value = 2;
        _numBindirme.Font = UiStyles.FontUi;
        card.Controls.Add(_lblOverlap);
        card.Controls.Add(_numBindirme);

        _lblToolHint = new Label
        {
            Location = new Point(12, 108),
            Size = new Size(450, 24)
        };
        UiStyles.ApplyExampleLabel(_lblToolHint);
        card.Controls.Add(_lblToolHint);

        return card;
    }

    private static Panel CreateCardPanel(int height)
    {
        return new Panel
        {
            Width = 480,
            Height = height,
            Margin = new Padding(0, 0, 0, 12),
            BackColor = UiStyles.CardBack,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(0)
        };
    }

    private static Panel CreateDataRow(int index)
    {
        return new Panel
        {
            Width = 460,
            Height = 34,
            Margin = new Padding(0, 0, 0, 2),
            BackColor = index % 2 == 0 ? UiStyles.Surface : Color.FromArgb(245, 247, 250)
        };
    }

    private static NumericUpDown CreateRowNumeric(int x, decimal value)
    {
        return new NumericUpDown
        {
            Location = new Point(x, 4),
            Width = InputWidth,
            DecimalPlaces = 2,
            Minimum = 0,
            Maximum = 99999,
            Value = value,
            Font = UiStyles.FontUi
        };
    }

    private void ApplyThicknessToAll()
    {
        decimal value = _numApplyThickness.Value;
        foreach (var num in _edgeThicknessInputs.Values)
            num.Value = ClampDecimal(num, (double)value);
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
        _panelParams.BringToFront();
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
