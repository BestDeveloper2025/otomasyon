using otomasyon.Models.Simulation;
using otomasyon.Rendering;
using otomasyon.Simulation;

namespace otomasyon.UI;

public sealed class SimulationForm : Form
{
    private const double PaddingPixels = 20d;
    private const double DefaultStepMm = 2.0;

    private readonly SimulationJob _job;
    private readonly SimulationEngine _engine;
    private readonly SimulationSceneRenderer _renderer = new();
    private readonly DrawingPanel _drawPanel = new();
    private readonly TextBox _txtLog = new();
    private readonly Label _lblStatus = new();
    private readonly System.Windows.Forms.Timer _timer = new();
    private readonly TrackBar _trackSpeed = new();
    private bool _running;
    private int _lastLoggedTour = -1;
    private int _lastLoggedSegment = -1;

    public SimulationForm(SimulationJob job)
    {
        _job = job;
        _engine = new SimulationEngine(job.Path, job.Plan);

        Text = "Taş Simülasyonu — " + Path.GetFileName(job.SourceFilePath);
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1100, 720);
        MinimumSize = new Size(800, 500);

        BuildUi();
        AppendPlanToLog();
        RefreshUi();
    }

    private void BuildUi()
    {
        var top = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = SystemColors.Control };

        var btnPlay = new Button { Text = "▶ Oynat", Location = new Point(10, 10), Size = new Size(90, 32) };
        var btnPause = new Button { Text = "⏸ Durdur", Location = new Point(106, 10), Size = new Size(90, 32) };
        var btnStep = new Button { Text = "Adım", Location = new Point(202, 10), Size = new Size(70, 32) };
        var btnReset = new Button { Text = "Sıfırla", Location = new Point(278, 10), Size = new Size(80, 32) };
        var btnExportCsv = new Button { Text = "CSV Çıktı", Location = new Point(364, 10), Size = new Size(90, 32) };
        var btnExportDat = new Button { Text = "DAT Çıktı", Location = new Point(460, 10), Size = new Size(90, 32) };

        btnPlay.Click += (_, _) => { _running = true; _timer.Start(); };
        btnPause.Click += (_, _) => { _running = false; _timer.Stop(); };
        btnStep.Click += (_, _) => { DoStep(); };
        btnExportCsv.Click += (_, _) => ExportCsv();
        btnExportDat.Click += (_, _) => ExportDat();
        btnReset.Click += (_, _) =>
        {
            _running = false;
            _timer.Stop();
            _engine.Reset();
            _reportShown = false;
            _lastLoggedTour = -1;
            _lastLoggedSegment = -1;
            AppendPlanToLog();
            RefreshUi();
        };

        _trackSpeed.Location = new Point(566, 14);
        _trackSpeed.Size = new Size(200, 32);
        _trackSpeed.Minimum = 1;
        _trackSpeed.Maximum = 20;
        _trackSpeed.Value = 5;
        _trackSpeed.TickFrequency = 2;

        top.Controls.Add(new Label { Text = "Hız:", Location = new Point(526, 16), AutoSize = true });
        top.Controls.Add(_trackSpeed);
        top.Controls.Add(btnPlay);
        top.Controls.Add(btnPause);
        top.Controls.Add(btnStep);
        top.Controls.Add(btnReset);
        top.Controls.Add(btnExportCsv);
        top.Controls.Add(btnExportDat);

        _lblStatus.Dock = DockStyle.Bottom;
        _lblStatus.Height = 48;
        _lblStatus.Padding = new Padding(12, 10, 12, 8);
        _lblStatus.Font = new Font("Segoe UI", 9.5f);
        _lblStatus.BackColor = Color.FromArgb(245, 248, 252);

        _txtLog.Dock = DockStyle.Fill;
        _txtLog.Multiline = true;
        _txtLog.ReadOnly = true;
        _txtLog.ScrollBars = ScrollBars.Vertical;
        _txtLog.Font = new Font("Consolas", 9f);
        _txtLog.BackColor = Color.FromArgb(252, 252, 252);

        _drawPanel.Dock = DockStyle.Fill;
        _drawPanel.BackColor = Color.White;
        _drawPanel.BorderStyle = BorderStyle.FixedSingle;
        _drawPanel.Paint += DrawPanel_Paint;

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterWidth = 6
        };
        split.Panel1.Controls.Add(_drawPanel);
        split.Panel2.Controls.Add(_txtLog);

        Controls.Add(split);
        Controls.Add(_lblStatus);
        Controls.Add(top);

        Load += (_, _) =>
        {
            int w = split.ClientSize.Width;
            split.SplitterDistance = Math.Clamp((int)(w * 0.65), 200, w - 200);
        };

        _timer.Interval = 40;
        _timer.Tick += (_, _) =>
        {
            if (!_running)
                return;

            double step = DefaultStepMm * _trackSpeed.Value / 5.0;
            if (!_engine.Step(step))
            {
                _running = false;
                _timer.Stop();
            }

            RefreshUi();
        };
    }

    private void DoStep()
    {
        double step = DefaultStepMm * _trackSpeed.Value / 5.0;
        _engine.Step(step);
        RefreshUi();
    }

    private void AppendPlanToLog()
    {
        _txtLog.Text = SimulationLogFormatter.FormatPlan(_job.Plan, _job.Tool);
        _txtLog.AppendText(Environment.NewLine + Environment.NewLine + "--- Simülasyon ---" + Environment.NewLine);
    }

    private bool _reportShown;

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _running = false;
        _timer.Stop();
        base.OnFormClosing(e);
    }

    private void RefreshUi()
    {
        if (IsDisposed || Disposing || !_txtLog.IsHandleCreated || _txtLog.IsDisposed)
            return;

        var snap = _engine.Current;
        _lblStatus.Text = snap.StatusText;
        _drawPanel.Invalidate();

        LogEdgeEntryIfChanged(snap);

        if (snap.IsFinished && !_reportShown)
        {
            _reportShown = true;
            string report = SimulationReportBuilder.BuildReport(_job, snap);
            SafeAppendLog(Environment.NewLine + report);
            MessageBox.Show(this, "Simülasyon tamamlandı. Log ekranından detaylı raporu inceleyebilirsiniz.", 
                            "Bitti", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void LogEdgeEntryIfChanged(SimulationSnapshot snap)
    {
        if (IsDisposed || Disposing || !_txtLog.IsHandleCreated || _txtLog.IsDisposed)
            return;

        if (snap.IsFinished)
            return;

        if (snap.TourIndex == _lastLoggedTour && snap.SegmentIndex == _lastLoggedSegment)
            return;

        bool newTour = snap.TourIndex != _lastLoggedTour;
        _lastLoggedTour = snap.TourIndex;
        _lastLoggedSegment = snap.SegmentIndex;

        if (snap.SegmentIndex < 0 || snap.SegmentIndex >= _job.Path.Segments.Count)
            return;

        var seg = _job.Path.Segments[snap.SegmentIndex];
        bool cutting = MachiningTourPlanner.IsCuttingOnEdge(_job.Plan, seg.EdgeIndex, snap.TourIndex);
        double depth = MachiningTourPlanner.GetDepthOnEdge(_job.Plan, seg.EdgeIndex, snap.TourIndex);

        if (newTour)
            SafeAppendLog(Environment.NewLine + $"=== Tur {snap.TourIndex + 1}/{snap.TourCount} (tam kontur, CCW) ===" + Environment.NewLine);

        SafeAppendLog(SimulationLogFormatter.FormatEdgeEntry(snap, seg, cutting, depth) + Environment.NewLine);
    }

    private void SafeAppendLog(string text)
    {
        try
        {
            if (IsDisposed || Disposing || !_txtLog.IsHandleCreated || _txtLog.IsDisposed)
                return;

            _txtLog.AppendText(text);
            _txtLog.SelectionStart = _txtLog.Text.Length;
            _txtLog.ScrollToCaret();
        }
        catch (ObjectDisposedException)
        {
            // Form kapanırken timer bir tick daha alabilir; log yazımını yoksay.
        }
    }

    private void ExportDat()
    {
        using var optionsDlg = new ExportCsvDialog();
        if (optionsDlg.ShowDialog(this) != DialogResult.OK)
            return;

        string defaultName = Path.ChangeExtension(Path.GetFileName(_job.SourceFilePath), ".dat");
        using var saveDlg = new SaveFileDialog
        {
            Filter = "DAT dosyası (*.dat)|*.dat|Tüm dosyalar (*.*)|*.*",
            FileName = defaultName,
            Title = "DAT çıktısını kaydet"
        };

        if (saveDlg.ShowDialog(this) != DialogResult.OK)
            return;

        if (!DatFileExporter.TryWrite(_job, optionsDlg.Options, saveDlg.FileName, out string? error))
        {
            MessageBox.Show(this, error ?? "Çıktı kaydedilemedi.", "DAT Çıktı",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        MessageBox.Show(this,
            $"DAT dosyası kaydedildi:\n{saveDlg.FileName}",
            "DAT Çıktı",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void ExportCsv()
    {
        using var optionsDlg = new ExportCsvDialog();
        if (optionsDlg.ShowDialog(this) != DialogResult.OK)
            return;

        string defaultName = Path.ChangeExtension(Path.GetFileName(_job.SourceFilePath), ".csv");
        using var saveDlg = new SaveFileDialog
        {
            Filter = "CSV dosyası (*.csv)|*.csv|Tüm dosyalar (*.*)|*.*",
            FileName = defaultName,
            Title = "CSV çıktısını kaydet"
        };

        if (saveDlg.ShowDialog(this) != DialogResult.OK)
            return;

        if (!CsvFileExporter.TryWrite(_job, optionsDlg.Options, saveDlg.FileName, out string? error))
        {
            MessageBox.Show(this, error ?? "Çıktı kaydedilemedi.", "Çıktı Al",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        MessageBox.Show(this,
            $"CSV dosyası kaydedildi:\n{saveDlg.FileName}",
            "Çıktı Al",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private void DrawPanel_Paint(object? sender, PaintEventArgs e)
    {
        var rect = _drawPanel.ClientRectangle;
        if (!WorldToScreenTransform.TryCreate(rect, _job.Scene.Bounds, PaddingPixels, out var transform))
        {
            e.Graphics.Clear(Color.White);
            return;
        }

        _renderer.Paint(e.Graphics, _job, _engine.Current, rect, transform);
    }
}
