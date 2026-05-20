using otomasyon.Models;
using otomasyon.Models.Simulation;

namespace otomasyon.UI;

/// <summary>Şekil onayı + kenar kalınlıkları + taş parametreleri.</summary>
public sealed class SimulationSetupDialog : Form
{
    private readonly DxfScene _scene;
    private readonly Panel _panelConfirm = new();
    private readonly Panel _panelParams = new();
    private readonly FlowLayoutPanel _edgeFlow = new();
    private readonly NumericUpDown _numStone = new();
    private readonly NumericUpDown _numBindirme = new();
    private readonly Dictionary<int, NumericUpDown> _edgeThicknessInputs = new();

    public IReadOnlyDictionary<int, double>? ThicknessByEdge { get; private set; }
    public StoneToolSettings? Tool { get; private set; }

    public SimulationSetupDialog(DxfScene scene)
    {
        _scene = scene;
        Text = "Simülasyon Parametreleri";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(480, 520);
        MinimumSize = new Size(400, 400);

        BuildConfirmStep();
        BuildParamsStep();
        ShowConfirmStep();
    }

    private void BuildConfirmStep()
    {
        _panelConfirm.Dock = DockStyle.Fill;
        var lbl = new Label
        {
            Text = "Şekil ve köşe numaraları (K1, K2, …) doğru mu?\n\n" +
                   "Kenar sırası CCW (0,0) referanslıdır. Radiuslar ayrı kenar sayılır.\n" +
                   "Hayır derseniz DXF dosyasını kontrol edin.",
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

        var btnNo = new Button { Text = "Hayır", DialogResult = DialogResult.Cancel, Width = 100 };
        var btnYes = new Button { Text = "Evet, devam", Width = 120 };
        btnYes.Click += (_, _) => ShowParamsStep();

        flow.Controls.Add(btnNo);
        flow.Controls.Add(btnYes);
        _panelConfirm.Controls.Add(flow);
        _panelConfirm.Controls.Add(lbl);
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

        inner.Controls.Add(new Label
        {
            Text = "Her kenar için işleme kalınlığı (mm). Radius dahil tüm kenarlar:",
            AutoSize = true,
            Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Margin = new Padding(0, 0, 0, 8)
        });

        _edgeFlow.FlowDirection = FlowDirection.TopDown;
        _edgeFlow.AutoSize = true;
        _edgeFlow.WrapContents = false;
        _edgeFlow.Width = 420;

        foreach (var edge in _scene.ContourEdges)
        {
            var row = new Panel { Width = 400, Height = 36 };
            string label = edge.IsRadiusSegment && edge.RadiusIndex is int ri
                ? $"L{edge.Index} (K{edge.CornerIndex}, R{ri})"
                : $"L{edge.Index} (K{edge.CornerIndex})";

            row.Controls.Add(new Label
            {
                Text = label + " mm:",
                Location = new Point(0, 8),
                AutoSize = true
            });

            var num = new NumericUpDown
            {
                Location = new Point(200, 4),
                Width = 120,
                DecimalPlaces = 2,
                Maximum = 99999,
                Minimum = 0,
                Value = 10,
                ThousandsSeparator = false
            };
            _edgeThicknessInputs[edge.Index] = num;
            row.Controls.Add(num);
            _edgeFlow.Controls.Add(row);
        }

        inner.Controls.Add(_edgeFlow);

        var toolPanel = new GroupBox
        {
            Text = "Taş (takım)",
            Dock = DockStyle.Top,
            Height = 110,
            Padding = new Padding(12),
            Margin = new Padding(0, 16, 0, 0)
        };

        toolPanel.Controls.Add(new Label { Text = "Taş genişliği (mm):", Location = new Point(16, 28), AutoSize = true });
        _numStone.Location = new Point(200, 24);
        _numStone.Width = 120;
        _numStone.DecimalPlaces = 2;
        _numStone.Minimum = 0.01m;
        _numStone.Maximum = 99999;
        _numStone.Value = 10;
        toolPanel.Controls.Add(_numStone);

        toolPanel.Controls.Add(new Label { Text = "Bindirme (mm):", Location = new Point(16, 58), AutoSize = true });
        _numBindirme.Location = new Point(200, 54);
        _numBindirme.Width = 120;
        _numBindirme.DecimalPlaces = 2;
        _numBindirme.Minimum = 0;
        _numBindirme.Maximum = 99998;
        _numBindirme.Value = 2;
        toolPanel.Controls.Add(_numBindirme);

        toolPanel.Controls.Add(new Label
        {
            Text = "Örn. taş 10 mm, bindirme 2 → geçişler: 10, 8, 6… mm (çizgiler arasında boşluk kalmaz).",
            Location = new Point(16, 82),
            Size = new Size(420, 32),
            ForeColor = Color.DimGray,
            Font = new Font("Segoe UI", 8f)
        });

        inner.Controls.Add(toolPanel);

        scroll.Controls.Add(inner);

        var bottom = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(12)
        };

        var btnCancel = new Button { Text = "İptal", DialogResult = DialogResult.Cancel, Width = 90 };
        var btnBack = new Button { Text = "Geri", Width = 90 };
        btnBack.Click += (_, _) => ShowConfirmStep();
        var btnOk = new Button { Text = "Simülasyonu Başlat", Width = 150 };
        btnOk.Click += OnStartClick;

        bottom.Controls.Add(btnCancel);
        bottom.Controls.Add(btnOk);
        bottom.Controls.Add(btnBack);

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
            MessageBox.Show(this, ex.Message, "Parametre Hatası", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        ThicknessByEdge = dict;
        Tool = tool;
        DialogResult = DialogResult.OK;
        Close();
    }
}
