using otomasyon.Localization;
using otomasyon.Simulation;

namespace otomasyon.UI;

public sealed class ExportCsvDialog : Form, ILocalizable
{
    private readonly NumericUpDown _numKalinlik = new();
    private readonly NumericUpDown _numAdet = new();
    private readonly Label _lblInfo = new();
    private readonly Label _lblThickness = new();
    private readonly Label _lblQty = new();
    private readonly Button _btnCancel = new();
    private readonly Button _btnOk = new();

    public CsvFileExporter.ExportOptions Options { get; private set; } = null!;

    public ExportCsvDialog()
    {
        var defaults = CsvFileExporter.CreateDefaultOptions();

        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(360, 180);

        _lblInfo.Location = new Point(16, 12);
        _lblInfo.Size = new Size(320, 40);
        _lblInfo.Font = new Font("Segoe UI", 9f);

        _lblThickness.Location = new Point(16, 62);
        _lblThickness.AutoSize = true;
        _numKalinlik.Location = new Point(180, 58);
        _numKalinlik.Width = 120;
        _numKalinlik.DecimalPlaces = 2;
        _numKalinlik.Maximum = 99999;
        _numKalinlik.Minimum = 1;
        _numKalinlik.Value = (decimal)defaults.KalinlikMm;

        _lblQty.Location = new Point(16, 96);
        _lblQty.AutoSize = true;
        _numAdet.Location = new Point(180, 92);
        _numAdet.Width = 120;
        _numAdet.Minimum = 1;
        _numAdet.Maximum = 99999;
        _numAdet.Value = defaults.IstenilenAdet;

        var flow = DialogUiHelper.CreateBottomButtonBar();

        _btnCancel.DialogResult = DialogResult.Cancel;
        DialogUiHelper.ConfigureButton(_btnCancel, 90);
        DialogUiHelper.ConfigureButton(_btnOk, 100);
        _btnOk.Click += (_, _) =>
        {
            Options = new CsvFileExporter.ExportOptions
            {
                KalinlikMm = (double)_numKalinlik.Value,
                IstenilenAdet = (int)_numAdet.Value
            };
            DialogResult = DialogResult.OK;
            Close();
        };

        flow.Controls.Add(_btnCancel);
        flow.Controls.Add(_btnOk);

        Controls.Add(flow);
        Controls.Add(_lblQty);
        Controls.Add(_numAdet);
        Controls.Add(_lblThickness);
        Controls.Add(_numKalinlik);
        Controls.Add(_lblInfo);
        AcceptButton = _btnOk;
        CancelButton = _btnCancel;

        LocalizationManager.LanguageChanged += (_, _) => { if (!IsDisposed) ApplyLocalization(); };
        ApplyLocalization();
    }

    public void ApplyLocalization()
    {
        Text = L.Get("Dialog.CsvExport");
        _lblInfo.Text = L.Get("Export.CsvInfo");
        _lblThickness.Text = L.Get("Export.GeneralThickness");
        _lblQty.Text = L.Get("Setup.DesiredQty");
        _btnCancel.Text = L.Get("Btn.Cancel");
        _btnOk.Text = L.Get("Btn.Save");
    }
}
