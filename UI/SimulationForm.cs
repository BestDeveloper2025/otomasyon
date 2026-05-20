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
    private string? _lastLoggedLine;

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

        btnPlay.Click += (_, _) => { _running = true; _timer.Start(); };
        btnPause.Click += (_, _) => { _running = false; _timer.Stop(); };
        btnStep.Click += (_, _) => { DoStep(); };
        btnReset.Click += (_, _) => { _running = false; _timer.Stop(); _engine.Reset(); RefreshUi(); };

        _trackSpeed.Location = new Point(380, 14);
        _trackSpeed.Size = new Size(200, 32);
        _trackSpeed.Minimum = 1;
        _trackSpeed.Maximum = 20;
        _trackSpeed.Value = 5;
        _trackSpeed.TickFrequency = 2;

        top.Controls.Add(new Label { Text = "Hız:", Location = new Point(340, 16), AutoSize = true });
        top.Controls.Add(_trackSpeed);
        top.Controls.Add(btnPlay);
        top.Controls.Add(btnPause);
        top.Controls.Add(btnStep);
        top.Controls.Add(btnReset);

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

    private void RefreshUi()
    {
        var snap = _engine.Current;
        _lblStatus.Text = snap.StatusText;
        _drawPanel.Invalidate();

        string line = SimulationLogFormatter.FormatSnapshot(snap);
        if (line != _lastLoggedLine)
        {
            _lastLoggedLine = line;
            _txtLog.AppendText(line + Environment.NewLine);
            _txtLog.SelectionStart = _txtLog.Text.Length;
            _txtLog.ScrollToCaret();
        }
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
