using otomasyon.Localization;
using otomasyon.Settings;

namespace otomasyon.UI;

public sealed class FtpSettingsDialog : Form, ILocalizable
{
    private readonly Label _lblHint = new();
    private readonly Label _lblHost = new();
    private readonly Label _lblHostExample = new();
    private readonly TextBox _txtHost = new();
    private readonly Label _lblPort = new();
    private readonly Label _lblPortExample = new();
    private readonly NumericUpDown _numPort = new();
    private readonly Label _lblUsername = new();
    private readonly Label _lblUsernameExample = new();
    private readonly TextBox _txtUsername = new();
    private readonly Label _lblPassword = new();
    private readonly Label _lblPasswordExample = new();
    private readonly TextBox _txtPassword = new();
    private readonly Label _lblRemoteDirectory = new();
    private readonly Label _lblRemoteDirectoryExample = new();
    private readonly TextBox _txtRemoteDirectory = new();
    private readonly Button _btnCancel = new();
    private readonly Button _btnOk = new();

    private const int LabelX = 16;
    private const int FieldX = 200;
    private const int FieldWidth = 210;
    private const int RowHeight = 52;

    public FtpSettingsDialog()
    {
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(440, 360);

        _lblHint.Location = new Point(LabelX, 12);
        _lblHint.Size = new Size(400, 28);
        _lblHint.ForeColor = Color.FromArgb(90, 90, 90);
        _lblHint.Font = new Font("Segoe UI", 9f);

        int rowTop = 48;
        PlaceFieldRow(rowTop, _lblHost, _lblHostExample, _txtHost);
        rowTop += RowHeight;
        PlacePortRow(rowTop, _lblPort, _lblPortExample, _numPort);
        rowTop += RowHeight;
        PlaceFieldRow(rowTop, _lblUsername, _lblUsernameExample, _txtUsername);
        rowTop += RowHeight;
        PlaceFieldRow(rowTop, _lblPassword, _lblPasswordExample, _txtPassword);
        _txtPassword.UseSystemPasswordChar = true;
        rowTop += RowHeight;
        PlaceFieldRow(rowTop, _lblRemoteDirectory, _lblRemoteDirectoryExample, _txtRemoteDirectory);

        _numPort.Minimum = 1;
        _numPort.Maximum = 65535;
        _numPort.Value = FtpSettings.DefaultPort;
        _numPort.Width = 120;

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
        Controls.Add(_txtRemoteDirectory);
        Controls.Add(_lblRemoteDirectoryExample);
        Controls.Add(_lblRemoteDirectory);
        Controls.Add(_txtPassword);
        Controls.Add(_lblPasswordExample);
        Controls.Add(_lblPassword);
        Controls.Add(_txtUsername);
        Controls.Add(_lblUsernameExample);
        Controls.Add(_lblUsername);
        Controls.Add(_numPort);
        Controls.Add(_lblPortExample);
        Controls.Add(_lblPort);
        Controls.Add(_txtHost);
        Controls.Add(_lblHostExample);
        Controls.Add(_lblHost);
        Controls.Add(_lblHint);

        AcceptButton = _btnOk;
        CancelButton = _btnCancel;

        LocalizationManager.LanguageChanged += (_, _) => { if (!IsDisposed) ApplyLocalization(); };
        ApplyLocalization();
        LoadValues();
    }

    public void ApplyLocalization()
    {
        Text = L.Get("Dialog.FtpSettings");
        _lblHint.Text = L.Get("Settings.FtpHint");
        _lblHost.Text = L.Get("Settings.FtpHost");
        _lblHostExample.Text = L.Get("Settings.FtpHostExample");
        _lblPort.Text = L.Get("Settings.FtpPort");
        _lblPortExample.Text = L.Get("Settings.FtpPortExample");
        _lblUsername.Text = L.Get("Settings.FtpUsername");
        _lblUsernameExample.Text = L.Get("Settings.FtpUsernameExample");
        _lblPassword.Text = L.Get("Settings.FtpPassword");
        _lblPasswordExample.Text = L.Get("Settings.FtpPasswordExample");
        _lblRemoteDirectory.Text = L.Get("Settings.FtpRemoteDirectory");
        _lblRemoteDirectoryExample.Text = L.Get("Settings.FtpRemoteDirectoryExample");
        _btnCancel.Text = L.Get("Btn.Cancel");
        _btnOk.Text = L.Get("Btn.Save");
    }

    private void LoadValues()
    {
        var ftp = AppSettingsManager.Ftp;
        _txtHost.Text = ftp.Host;
        _numPort.Value = ftp.Port is >= 1 and <= 65535 ? ftp.Port : FtpSettings.DefaultPort;
        _txtUsername.Text = ftp.Username;
        _txtPassword.Text = ftp.Password;
        _txtRemoteDirectory.Text = ftp.RemoteDirectory;
    }

    private void OnOkClick(object? sender, EventArgs e)
    {
        var settings = new FtpSettings
        {
            Host = _txtHost.Text.Trim(),
            Port = (int)_numPort.Value,
            Username = _txtUsername.Text.Trim(),
            Password = _txtPassword.Text,
            RemoteDirectory = _txtRemoteDirectory.Text.Trim()
        };

        if (!AppSettingsManager.TrySaveFtp(settings, out string? error))
        {
            MessageBox.Show(this, error ?? L.Get("Error.FtpSaveFailed"),
                L.Get("Dialog.FtpSettings"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        DialogResult = DialogResult.OK;
        Close();
    }

    private static void PlaceFieldRow(int top, Label caption, Label example, TextBox input)
    {
        caption.Location = new Point(LabelX, top + 14);
        caption.AutoSize = true;

        example.Location = new Point(FieldX, top);
        example.AutoSize = false;
        example.Size = new Size(FieldWidth, 16);
        StyleExampleLabel(example);

        input.Location = new Point(FieldX, top + 18);
        input.Width = FieldWidth;
    }

    private static void PlacePortRow(int top, Label caption, Label example, NumericUpDown input)
    {
        caption.Location = new Point(LabelX, top + 14);
        caption.AutoSize = true;

        example.Location = new Point(FieldX, top);
        example.AutoSize = false;
        example.Size = new Size(FieldWidth, 16);
        StyleExampleLabel(example);

        input.Location = new Point(FieldX, top + 18);
    }

    private static void StyleExampleLabel(Label label)
    {
        label.ForeColor = Color.FromArgb(120, 120, 120);
        label.Font = new Font("Segoe UI", 8.25f, FontStyle.Italic);
    }
}
