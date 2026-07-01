using otomasyon.Localization;

namespace otomasyon.UI;

public sealed class FtpUploadDialog : Form, ILocalizable
{
    private readonly Label _lblHint = new();
    private readonly Label _lblFileName = new();
    private readonly Label _lblFileNameExample = new();
    private readonly TextBox _txtFileName = new();
    private readonly Button _btnCancel = new();
    private readonly Button _btnOk = new();

    public string FileName { get; private set; } = string.Empty;

    public FtpUploadDialog(string suggestedFileName)
    {
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(420, 168);

        _lblHint.Location = new Point(16, 12);
        _lblHint.Size = new Size(380, 32);
        _lblHint.ForeColor = Color.FromArgb(90, 90, 90);
        _lblHint.Font = new Font("Segoe UI", 9f);

        _lblFileName.Location = new Point(16, 52);
        _lblFileName.AutoSize = true;

        _lblFileNameExample.Location = new Point(16, 72);
        _lblFileNameExample.AutoSize = false;
        _lblFileNameExample.Size = new Size(380, 16);
        _lblFileNameExample.ForeColor = Color.FromArgb(120, 120, 120);
        _lblFileNameExample.Font = new Font("Segoe UI", 8.25f, FontStyle.Italic);

        _txtFileName.Location = new Point(16, 90);
        _txtFileName.Width = 380;
        _txtFileName.Text = suggestedFileName;

        var flow = DialogUiHelper.CreateBottomButtonBar();

        _btnCancel.DialogResult = DialogResult.Cancel;
        DialogUiHelper.ConfigureButton(_btnCancel, 90);
        DialogUiHelper.ConfigureButton(_btnOk, 100);
        _btnOk.Click += OnOkClick;

        flow.Controls.Add(_btnCancel);
        flow.Controls.Add(_btnOk);

        Controls.Add(flow);
        Controls.Add(_txtFileName);
        Controls.Add(_lblFileNameExample);
        Controls.Add(_lblFileName);
        Controls.Add(_lblHint);

        AcceptButton = _btnOk;
        CancelButton = _btnCancel;

        LocalizationManager.LanguageChanged += (_, _) => { if (!IsDisposed) ApplyLocalization(); };
        ApplyLocalization();
    }

    public void ApplyLocalization()
    {
        Text = L.Get("Dialog.FtpUpload");
        _lblHint.Text = L.Get("Ftp.UploadHint");
        _lblFileName.Text = L.Get("Ftp.FileName");
        _lblFileNameExample.Text = L.Get("Ftp.FileNameExample");
        _btnCancel.Text = L.Get("Btn.Cancel");
        _btnOk.Text = L.Get("Btn.Send");
    }

    private void OnOkClick(object? sender, EventArgs e)
    {
        string name = NormalizeFileName(_txtFileName.Text);
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show(this, L.Get("Error.FtpFileNameRequired"),
                L.Get("Dialog.FtpUpload"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        FileName = name;
        DialogResult = DialogResult.OK;
        Close();
    }

    private static string NormalizeFileName(string raw)
    {
        string name = raw.Trim();
        if (string.IsNullOrEmpty(name))
            return string.Empty;

        name = Path.GetFileName(name);
        foreach (char c in Path.GetInvalidFileNameChars())
            name = name.Replace(c, '_');

        if (!name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            name += ".csv";

        return name;
    }
}
