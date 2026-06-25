using otomasyon.Localization;
using otomasyon.Settings;

namespace otomasyon.UI;

public sealed class SettingsDialog : Form, ILocalizable
{
    private readonly Label _lblHint = new();
    private readonly ComboBox _cmbLanguage = new();
    private readonly ComboBox _cmbMachineDirection = new();
    private readonly NumericUpDown _numMaxWidth = new();
    private readonly NumericUpDown _numMaxHeight = new();
    private readonly Label _lblLanguage = new();
    private readonly Label _lblMachine = new();
    private readonly Label _lblMaxWidth = new();
    private readonly Label _lblMaxHeight = new();
    private readonly Button _btnFtpSettings = new();
    private readonly Button _btnCancel = new();
    private readonly Button _btnOk = new();

    public SettingsDialog()
    {
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(440, 300);

        _lblHint.Location = new Point(16, 12);
        _lblHint.Size = new Size(400, 32);
        _lblHint.ForeColor = Color.FromArgb(90, 90, 90);
        _lblHint.Font = new Font("Segoe UI", 9f);

        _lblLanguage.Location = new Point(16, 52);
        _lblLanguage.AutoSize = true;
        _cmbLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbLanguage.Location = new Point(200, 48);
        _cmbLanguage.Width = 210;

        _lblMachine.Location = new Point(16, 86);
        _lblMachine.AutoSize = true;
        _cmbMachineDirection.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbMachineDirection.Location = new Point(200, 82);
        _cmbMachineDirection.Width = 210;

        _lblMaxWidth.Location = new Point(16, 120);
        _lblMaxWidth.AutoSize = true;
        _numMaxWidth.Location = new Point(200, 116);
        _numMaxWidth.Width = 120;
        _numMaxWidth.DecimalPlaces = 2;
        _numMaxWidth.Minimum = 0;
        _numMaxWidth.Maximum = 999999;
        _numMaxWidth.Increment = 10;

        _lblMaxHeight.Location = new Point(16, 154);
        _lblMaxHeight.AutoSize = true;
        _numMaxHeight.Location = new Point(200, 150);
        _numMaxHeight.Width = 120;
        _numMaxHeight.DecimalPlaces = 2;
        _numMaxHeight.Minimum = 0;
        _numMaxHeight.Maximum = 999999;
        _numMaxHeight.Increment = 10;

        _btnFtpSettings.Location = new Point(16, 188);
        _btnFtpSettings.Size = new Size(394, 32);
        _btnFtpSettings.Click += OnFtpSettingsClick;

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = 48,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(12, 8, 12, 8)
        };

        _btnCancel.DialogResult = DialogResult.Cancel;
        _btnCancel.Width = 90;
        _btnOk.Width = 90;
        _btnOk.Click += OnOkClick;

        flow.Controls.Add(_btnCancel);
        flow.Controls.Add(_btnOk);

        Controls.Add(flow);
        Controls.Add(_btnFtpSettings);
        Controls.Add(_numMaxHeight);
        Controls.Add(_lblMaxHeight);
        Controls.Add(_numMaxWidth);
        Controls.Add(_lblMaxWidth);
        Controls.Add(_cmbMachineDirection);
        Controls.Add(_lblMachine);
        Controls.Add(_cmbLanguage);
        Controls.Add(_lblLanguage);
        Controls.Add(_lblHint);

        AcceptButton = _btnOk;
        CancelButton = _btnCancel;

        LocalizationManager.LanguageChanged += (_, _) => { if (!IsDisposed) ApplyLocalization(); };
        ApplyLocalization();
        LoadValues();
    }

    public void ApplyLocalization()
    {
        Text = L.Get("Dialog.Settings");
        _lblHint.Text = L.Get("Settings.RequiredHint");
        _lblLanguage.Text = L.Get("Lang.Label");
        _lblMachine.Text = RequiredLabel("Settings.MachineDirection");
        _lblMaxWidth.Text = RequiredLabel("Settings.MaxShapeWidth");
        _lblMaxHeight.Text = RequiredLabel("Settings.MaxShapeHeight");
        _btnFtpSettings.Text = L.Get("Settings.FtpButton");
        _btnCancel.Text = L.Get("Btn.Cancel");
        _btnOk.Text = L.Get("Btn.Save");

        PopulateCombos();
    }

    private static string RequiredLabel(string key)
        => L.Get(key) + " " + L.Get("Label.Required");

    private void LoadValues()
    {
        if (AppSettingsManager.Limits.IsValid)
        {
            _numMaxWidth.Value = (decimal)AppSettingsManager.Limits.MaxWidthMm;
            _numMaxHeight.Value = (decimal)AppSettingsManager.Limits.MaxHeightMm;
        }

        SelectMachineDirection(AppSettingsManager.MachineDirection);
    }

    private void OnOkClick(object? sender, EventArgs e)
    {
        if (_cmbMachineDirection.SelectedIndex < 0)
        {
            MessageBox.Show(this, L.Get("Error.MachineDirectionRequired"),
                L.Get("Dialog.Settings"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_numMaxWidth.Value <= 0 || _numMaxHeight.Value <= 0)
        {
            MessageBox.Show(this, L.Get("Error.LimitsRequired"),
                L.Get("Dialog.Settings"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var lang = _cmbLanguage.SelectedItem is LanguageComboItem langItem
            ? langItem.Language
            : LocalizationManager.CurrentLanguage;

        var direction = _cmbMachineDirection.SelectedItem is MachineComboItem machineItem
            ? machineItem.Direction
            : MachineDirection.LeftToRight;

        if (!AppSettingsManager.TrySave(
                lang,
                direction,
                (double)_numMaxWidth.Value,
                (double)_numMaxHeight.Value,
                out string? error))
        {
            MessageBox.Show(this, error ?? L.Get("Error.LimitsPositive"),
                L.Get("Dialog.Settings"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private void OnFtpSettingsClick(object? sender, EventArgs e)
    {
        using var dlg = new FtpSettingsDialog();
        dlg.ShowDialog(this);
    }

    private void PopulateCombos()
    {
        _cmbLanguage.Items.Clear();
        _cmbLanguage.Items.Add(new LanguageComboItem(AppLanguage.English));
        _cmbLanguage.Items.Add(new LanguageComboItem(AppLanguage.Turkish));
        _cmbLanguage.Items.Add(new LanguageComboItem(AppLanguage.German));
        SelectLanguage(LocalizationManager.CurrentLanguage);

        _cmbMachineDirection.Items.Clear();
        _cmbMachineDirection.Items.Add(new MachineComboItem(MachineDirection.LeftToRight));
        _cmbMachineDirection.Items.Add(new MachineComboItem(MachineDirection.RightToLeft));
        SelectMachineDirection(AppSettingsManager.MachineDirection);
    }

    private void SelectLanguage(AppLanguage language)
    {
        for (int i = 0; i < _cmbLanguage.Items.Count; i++)
        {
            if (_cmbLanguage.Items[i] is LanguageComboItem item && item.Language == language)
            {
                _cmbLanguage.SelectedIndex = i;
                return;
            }
        }
    }

    private void SelectMachineDirection(MachineDirection direction)
    {
        for (int i = 0; i < _cmbMachineDirection.Items.Count; i++)
        {
            if (_cmbMachineDirection.Items[i] is MachineComboItem item && item.Direction == direction)
            {
                _cmbMachineDirection.SelectedIndex = i;
                return;
            }
        }
    }

    private sealed class LanguageComboItem(AppLanguage language)
    {
        public AppLanguage Language { get; } = language;

        public override string ToString() => language switch
        {
            AppLanguage.Turkish => L.Get("Lang.Turkish"),
            AppLanguage.German => L.Get("Lang.German"),
            _ => L.Get("Lang.English")
        };
    }

    private sealed class MachineComboItem(MachineDirection direction)
    {
        public MachineDirection Direction { get; } = direction;

        public override string ToString() => direction switch
        {
            MachineDirection.RightToLeft => L.Get("Settings.MachineRtl"),
            _ => L.Get("Settings.MachineLtr")
        };
    }
}
