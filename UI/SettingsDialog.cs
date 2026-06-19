using otomasyon.Localization;
using otomasyon.Settings;

namespace otomasyon.UI;

public sealed class SettingsDialog : Form, ILocalizable
{
    private readonly ComboBox _cmbLanguage = new();
    private readonly ComboBox _cmbMachineDirection = new();
    private readonly Label _lblLanguage = new();
    private readonly Label _lblMachine = new();
    private readonly Button _btnCancel = new();
    private readonly Button _btnOk = new();

    public SettingsDialog()
    {
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(420, 170);

        _lblLanguage.Location = new Point(16, 20);
        _lblLanguage.AutoSize = true;
        _cmbLanguage.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbLanguage.Location = new Point(180, 16);
        _cmbLanguage.Width = 210;

        _lblMachine.Location = new Point(16, 58);
        _lblMachine.AutoSize = true;
        _cmbMachineDirection.DropDownStyle = ComboBoxStyle.DropDownList;
        _cmbMachineDirection.Location = new Point(180, 54);
        _cmbMachineDirection.Width = 210;

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
        _btnOk.Click += (_, _) =>
        {
            if (_cmbLanguage.SelectedItem is LanguageComboItem langItem)
                LocalizationManager.SetLanguage(langItem.Language);

            if (_cmbMachineDirection.SelectedItem is MachineComboItem machineItem)
                AppSettingsManager.SetMachineDirection(machineItem.Direction);

            DialogResult = DialogResult.OK;
            Close();
        };

        flow.Controls.Add(_btnCancel);
        flow.Controls.Add(_btnOk);

        Controls.Add(flow);
        Controls.Add(_cmbMachineDirection);
        Controls.Add(_lblMachine);
        Controls.Add(_cmbLanguage);
        Controls.Add(_lblLanguage);

        AcceptButton = _btnOk;
        CancelButton = _btnCancel;

        LocalizationManager.LanguageChanged += (_, _) => { if (!IsDisposed) ApplyLocalization(); };
        ApplyLocalization();
        PopulateCombos();
    }

    public void ApplyLocalization()
    {
        Text = L.Get("Dialog.Settings");
        _lblLanguage.Text = L.Get("Lang.Label");
        _lblMachine.Text = L.Get("Settings.MachineDirection");
        _btnCancel.Text = L.Get("Btn.Cancel");
        _btnOk.Text = L.Get("Btn.Save");

        PopulateCombos();
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
