using otomasyon.Localization;
using otomasyon.Models.Simulation;
using otomasyon.Rendering;
using otomasyon.Simulation;

namespace otomasyon.UI;

public sealed class SimulationForm : Form, ILocalizable
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
    private readonly Button _btnPlay = new();
    private readonly Button _btnPause = new();
    private readonly Button _btnStep = new();
    private readonly Button _btnReset = new();
    private readonly Button _btnExportCsv = new();
    private readonly Button _btnExportDat = new();
    private readonly Label _lblSpeed = new();

    private bool _running;
    private int _lastLoggedTour = -1;
    private int _lastLoggedSegment = -1;
    private bool _reportShown;

    public SimulationForm(SimulationJob job)
    {
        _job = job;
        _engine = new SimulationEngine(job.Path, job.Plan);

        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1100, 720);
        MinimumSize = new Size(800, 500);

        BuildUi();
        LocalizationManager.LanguageChanged += (_, _) => { if (!IsDisposed) ApplyLocalization(); };
        ApplyLocalization();
        AppendPlanToLog();
        RefreshUi();
    }

    public void ApplyLocalization()
    {
        Text = L.F("Sim.Title", Path.GetFileName(_job.SourceFilePath));
        _btnPlay.Text = L.Get("Btn.Play");
        _btnPause.Text = L.Get("Btn.Pause");
        _btnStep.Text = L.Get("Btn.Step");
        _btnReset.Text = L.Get("Btn.Reset");
        _btnExportCsv.Text = L.Get("Btn.ExportCsv");
        _btnExportDat.Text = L.Get("Btn.ExportDat");
        _lblSpeed.Text = L.Get("Label.Speed");
    }

    private void BuildUi()
    {
        var top = new Panel { Dock = DockStyle.Top, Height = 52, BackColor = SystemColors.Control };

        _btnPlay.Location = new Point(10, 10);
        _btnPlay.Size = new Size(90, 32);
        _btnPause.Location = new Point(106, 10);
        _btnPause.Size = new Size(90, 32);
        _btnStep.Location = new Point(202, 10);
        _btnStep.Size = new Size(70, 32);
        _btnReset.Location = new Point(278, 10);
        _btnReset.Size = new Size(80, 32);
        _btnExportCsv.Location = new Point(364, 10);
        _btnExportCsv.Size = new Size(90, 32);
        _btnExportDat.Location = new Point(460, 10);
        _btnExportDat.Size = new Size(90, 32);

        _btnPlay.Click += (_, _) => { _running = true; _timer.Start(); };
        _btnPause.Click += (_, _) => { _running = false; _timer.Stop(); };
        _btnStep.Click += (_, _) => { DoStep(); };
        _btnExportCsv.Click += (_, _) => ExportCsv();
        _btnExportDat.Click += (_, _) => ExportDat();
        _btnReset.Click += (_, _) =>
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

        _lblSpeed.Location = new Point(526, 16);
        _lblSpeed.AutoSize = true;

        top.Controls.Add(_lblSpeed);
        top.Controls.Add(_trackSpeed);
        top.Controls.Add(_btnPlay);
        top.Controls.Add(_btnPause);
        top.Controls.Add(_btnStep);
        top.Controls.Add(_btnReset);
        top.Controls.Add(_btnExportCsv);
        top.Controls.Add(_btnExportDat);

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
        _txtLog.AppendText(Environment.NewLine + Environment.NewLine + L.Get("Sim.LogHeader") + Environment.NewLine);
    }

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
            MessageBox.Show(this,
                L.Get("Msg.SimCompleted"),
                L.Get("Title.Done"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
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
            SafeAppendLog(Environment.NewLine + L.F("Sim.TourHeader", snap.TourIndex + 1, snap.TourCount) + Environment.NewLine);

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
            Filter = L.Get("Filter.DatAll"),
            FileName = defaultName,
            Title = L.Get("Dialog.SaveDat")
        };

        if (saveDlg.ShowDialog(this) != DialogResult.OK)
            return;

        if (!DatFileExporter.TryWrite(_job, optionsDlg.Options, saveDlg.FileName, out string? error))
        {
            MessageBox.Show(this, error ?? L.Get("Msg.ExportSaveFailed"), L.Get("Title.DatExport"),
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        MessageBox.Show(this,
            L.F("Msg.DatSaved", saveDlg.FileName),
            L.Get("Title.DatExport"),
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
            Filter = L.Get("Filter.CsvAll"),
            FileName = defaultName,
            Title = L.Get("Dialog.SaveCsv")
        };

        if (saveDlg.ShowDialog(this) != DialogResult.OK)
            return;

        if (!CsvFileExporter.TryWrite(_job, optionsDlg.Options, saveDlg.FileName, out string? error))
        {
            MessageBox.Show(this, error ?? L.Get("Msg.ExportSaveFailed"), L.Get("Title.Export"),
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        MessageBox.Show(this,
            L.F("Msg.CsvSaved", saveDlg.FileName),
            L.Get("Title.Export"),
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
