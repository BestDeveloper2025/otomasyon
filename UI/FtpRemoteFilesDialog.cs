using otomasyon.Localization;
using otomasyon.Logging;
using otomasyon.Settings;
using otomasyon.Simulation;

namespace otomasyon.UI;

public sealed class FtpRemoteFilesDialog : Form, ILocalizable
{
    private readonly FtpSettings _settings;
    private readonly Label _lblHint = new();
    private readonly Label _lblPath = new();
    private readonly ListView _lvFiles = new();
    private readonly Label _lblStatus = new();
    private readonly Button _btnClose = new();
    private readonly Button _btnDelete = new();
    private readonly Button _btnRefresh = new();

    public FtpRemoteFilesDialog(FtpSettings settings)
    {
        _settings = settings;

        FormBorderStyle = FormBorderStyle.Sizable;
        MaximizeBox = true;
        MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new Size(520, 420);
        MinimumSize = new Size(420, 320);
        UiStyles.ApplyDialogChrome(this);

        _lblHint.Dock = DockStyle.Top;
        _lblHint.Height = 32;
        _lblHint.Padding = new Padding(12, 8, 12, 0);
        UiStyles.ApplyHintLabel(_lblHint);

        _lblPath.Dock = DockStyle.Top;
        _lblPath.Height = 24;
        _lblPath.Padding = new Padding(12, 0, 12, 0);
        _lblPath.Font = UiStyles.FontUiBold;
        _lblPath.ForeColor = UiStyles.TextPrimary;

        _lvFiles.Dock = DockStyle.Fill;
        _lvFiles.View = View.Details;
        _lvFiles.FullRowSelect = true;
        _lvFiles.HideSelection = false;
        _lvFiles.MultiSelect = false;
        _lvFiles.Font = UiStyles.FontUi;
        _lvFiles.Columns.Add("name", 360);

        _lblStatus.Dock = DockStyle.Bottom;
        _lblStatus.Height = 24;
        _lblStatus.Padding = new Padding(12, 0, 12, 0);
        UiStyles.ApplyMutedLabel(_lblStatus);

        var bottom = DialogUiHelper.CreateBottomButtonBar();

        _btnClose.DialogResult = DialogResult.Cancel;
        DialogUiHelper.ConfigureButton(_btnClose, 90);
        DialogUiHelper.ConfigureButton(_btnDelete, 110);
        _btnDelete.Enabled = false;
        _btnDelete.Click += async (_, _) => await DeleteSelectedAsync();
        DialogUiHelper.ConfigureButton(_btnRefresh, 100);
        _btnRefresh.Click += async (_, _) => await RefreshListAsync();

        _lvFiles.SelectedIndexChanged += (_, _) =>
            _btnDelete.Enabled = _lvFiles.SelectedItems.Count > 0;

        bottom.Controls.Add(_btnClose);
        bottom.Controls.Add(_btnDelete);
        bottom.Controls.Add(_btnRefresh);

        Controls.Add(_lvFiles);
        Controls.Add(_lblStatus);
        Controls.Add(bottom);
        Controls.Add(_lblPath);
        Controls.Add(_lblHint);

        CancelButton = _btnClose;

        Shown += async (_, _) => await RefreshListAsync();

        LocalizationManager.LanguageChanged += (_, _) => { if (!IsDisposed) ApplyLocalization(); };
        ApplyLocalization();
    }

    public void ApplyLocalization()
    {
        Text = L.Get("Dialog.FtpRemoteFiles");
        _lblHint.Text = L.Get("Ftp.RemoteFilesHint");
        _btnClose.Text = L.Get("Btn.Close");
        _btnDelete.Text = L.Get("Btn.DeleteSelected");
        _btnRefresh.Text = L.Get("Btn.Refresh");

        if (_lvFiles.Columns.Count > 0)
            _lvFiles.Columns[0].Text = L.Get("Ftp.ColFileName");

        UpdatePathLabel();
    }

    private void UpdatePathLabel()
    {
        string dir = FtpSettings.NormalizeRemoteDirectory(_settings.RemoteDirectory);
        _lblPath.Text = string.IsNullOrEmpty(dir)
            ? L.F("Ftp.RemotePathRoot", _settings.Host)
            : L.F("Ftp.RemotePath", _settings.Host, dir);
    }

    private async Task RefreshListAsync()
    {
        _btnRefresh.Enabled = false;
        _btnDelete.Enabled = false;
        _lblStatus.Text = L.Get("Ftp.Loading");
        _lvFiles.BeginUpdate();
        _lvFiles.Items.Clear();

        try
        {
            var result = await Task.Run(() =>
            {
                bool ok = FtpClientHelper.TryListFileNames(_settings, out IReadOnlyList<string>? names, out string? err);
                return (ok, names, err);
            }).ConfigureAwait(true);

            if (!result.ok)
            {
                _lblStatus.Text = result.err ?? L.Get("Error.FtpListFailed");
                MessageBox.Show(this,
                    result.err ?? L.Get("Error.FtpListFailed"),
                    L.Get("Dialog.FtpRemoteFiles"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            foreach (string name in result.names)
                _lvFiles.Items.Add(new ListViewItem(name));

            _lblStatus.Text = result.names.Count == 0
                ? L.Get("Ftp.EmptyFolder")
                : L.F("Ftp.FileCount", result.names.Count);
        }
        finally
        {
            _lvFiles.EndUpdate();
            _btnRefresh.Enabled = true;
            _btnDelete.Enabled = _lvFiles.SelectedItems.Count > 0;
        }
    }

    private async Task DeleteSelectedAsync()
    {
        if (_lvFiles.SelectedItems.Count == 0)
            return;

        string fileName = _lvFiles.SelectedItems[0].Text;
        var confirm = MessageBox.Show(this,
            L.F("Msg.FtpDeleteConfirm", fileName),
            L.Get("Dialog.FtpRemoteFiles"),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirm != DialogResult.Yes)
            return;

        _btnDelete.Enabled = false;
        _btnRefresh.Enabled = false;
        _lblStatus.Text = L.Get("Ftp.Deleting");

        try
        {
            string? error = await Task.Run(() =>
            {
                bool ok = FtpClientHelper.TryDeleteFile(_settings, fileName, out string? err);
                return ok ? null : err;
            }).ConfigureAwait(true);

            if (error is not null)
            {
                AppLog.Warn("FTP uzak dosya silinemedi", $"Dosya={fileName}, Host={_settings.Host}, Hata={error}");
                MessageBox.Show(this, error, L.Get("Dialog.FtpRemoteFiles"),
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            AppLog.UserAction("FTP uzak dosya silindi", $"Dosya={fileName}, Host={_settings.Host}");

            MessageBox.Show(this,
                L.F("Msg.FtpDeleteSuccess", fileName),
                L.Get("Dialog.FtpRemoteFiles"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            await RefreshListAsync();
        }
        finally
        {
            _btnRefresh.Enabled = true;
            _btnDelete.Enabled = _lvFiles.SelectedItems.Count > 0;
        }
    }
}
