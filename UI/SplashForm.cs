using otomasyon;

namespace otomasyon.UI;

/// <summary>Açılışta logo ve kısa dolma çubuğu gösteren splash ekranı.</summary>
internal sealed class SplashForm : Form
{
    private const int DurationMs = 1000;
    private const int TickMs = 16;

    private readonly PictureBox _picLogo = new();
    private readonly Panel _progressTrack = new();
    private readonly Panel _progressFill = new();
    private readonly System.Windows.Forms.Timer _timer = new();
    private int _elapsedMs;

    public SplashForm()
    {
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterScreen;
        ShowInTaskbar = false;
        TopMost = true;
        ClientSize = new Size(480, 280);
        BackColor = UiStyles.Surface;
        DoubleBuffered = true;

        var logoHost = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiStyles.Surface,
            Padding = new Padding(48, 40, 48, 28)
        };

        _picLogo.Dock = DockStyle.Fill;
        _picLogo.SizeMode = PictureBoxSizeMode.Zoom;
        _picLogo.BackColor = UiStyles.Surface;
        TryLoadLogo();
        logoHost.Controls.Add(_picLogo);

        _progressTrack.Dock = DockStyle.Bottom;
        _progressTrack.Height = 6;
        _progressTrack.BackColor = UiStyles.BrandPrimaryMuted;
        _progressTrack.Padding = Padding.Empty;

        _progressFill.Height = 6;
        _progressFill.Width = 0;
        _progressFill.BackColor = UiStyles.BrandPrimary;
        _progressFill.Dock = DockStyle.Left;
        _progressTrack.Controls.Add(_progressFill);

        Controls.Add(logoHost);
        Controls.Add(_progressTrack);

        Paint += (_, e) =>
        {
            using var pen = new Pen(UiStyles.Border, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
        };

        _timer.Interval = TickMs;
        _timer.Tick += OnTick;
        Shown += (_, _) => _timer.Start();
    }

    private void TryLoadLogo()
    {
        string? path = AppAssets.FindLogoPath();
        if (path is null)
            return;

        try
        {
            using var stream = File.OpenRead(path);
            _picLogo.Image = Image.FromStream(stream);
        }
        catch
        {
            // Logo yoksa boş splash ile devam
        }
    }

    private void OnTick(object? sender, EventArgs e)
    {
        _elapsedMs += TickMs;
        int w = (int)(_progressTrack.ClientSize.Width * Math.Min(1.0, _elapsedMs / (double)DurationMs));
        _progressFill.Width = Math.Max(0, w);

        if (_elapsedMs < DurationMs)
            return;

        _timer.Stop();
        DialogResult = DialogResult.OK;
        Close();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Stop();
            _timer.Dispose();
            _picLogo.Image?.Dispose();
        }

        base.Dispose(disposing);
    }
}
