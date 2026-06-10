using otomasyon.Simulation;

namespace otomasyon.UI;

/// <summary>.dat çıktısı için genel kalınlık ve adet.</summary>
public sealed class ExportDatDialog : Form
{
    private readonly NumericUpDown _numKalinlik = new();
    private readonly NumericUpDown _numAdet = new();

    public DatFileExporter.ExportOptions Options { get; private set; } = null!;

    public ExportDatDialog()
    {
        var defaults = DatFileExporter.CreateDefaultOptions();

        Text = "DAT Çıktısı";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(360, 180);

        var lblInfo = new Label
        {
            Text = "Kenar kalınlıkları (SA), uzunluklar (L), radius (R) ve köşe açıları (A)\n" +
                   "simülasyondaki kontur verisinden otomatik doldurulur.",
            Location = new Point(16, 12),
            Size = new Size(320, 40),
            Font = new Font("Segoe UI", 9f)
        };

        Controls.Add(new Label { Text = "Genel kalınlık (mm):", Location = new Point(16, 62), AutoSize = true });
        _numKalinlik.Location = new Point(180, 58);
        _numKalinlik.Width = 120;
        _numKalinlik.DecimalPlaces = 2;
        _numKalinlik.Maximum = 99999;
        _numKalinlik.Minimum = 1;
        _numKalinlik.Value = (decimal)defaults.KalinlikMm;
        Controls.Add(_numKalinlik);

        Controls.Add(new Label { Text = "İstenilen adet:", Location = new Point(16, 96), AutoSize = true });
        _numAdet.Location = new Point(180, 92);
        _numAdet.Width = 120;
        _numAdet.Minimum = 1;
        _numAdet.Maximum = 99999;
        _numAdet.Value = defaults.IstenilenAdet;
        Controls.Add(_numAdet);

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 48,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(12, 8, 12, 8)
        };

        var btnCancel = new Button { Text = "İptal", DialogResult = DialogResult.Cancel, Width = 90 };
        var btnOk = new Button { Text = "Kaydet…", Width = 100 };
        btnOk.Click += (_, _) =>
        {
            Options = new DatFileExporter.ExportOptions
            {
                KalinlikMm = (double)_numKalinlik.Value,
                IstenilenAdet = (int)_numAdet.Value
            };
            DialogResult = DialogResult.OK;
            Close();
        };

        flow.Controls.Add(btnCancel);
        flow.Controls.Add(btnOk);

        Controls.Add(flow);
        Controls.Add(lblInfo);
        AcceptButton = btnOk;
        CancelButton = btnCancel;
    }
}
