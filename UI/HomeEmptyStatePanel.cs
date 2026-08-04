using otomasyon.Localization;

namespace otomasyon.UI;

/// <summary>DXF yüklenmeden önce çizim alanında gösterilen karşılama paneli.</summary>
internal sealed class HomeEmptyStatePanel : Panel, ILocalizable
{
    private readonly TableLayoutPanel _layout = new();
    private readonly Label _lblTitle = new();
    private readonly Label _lblSubtitle = new();
    private readonly Label _lblSteps = new();
    private readonly FlowLayoutPanel _actions = new();
    private readonly Button _btnSelectFile = new();
    private readonly Button _btnImportCsv = new();

    public event EventHandler? SelectFileRequested;
    public event EventHandler? ImportCsvRequested;

    public HomeEmptyStatePanel()
    {
        Dock = DockStyle.Fill;
        BackColor = UiStyles.EmptyStateBack;
        Padding = new Padding(UiStyles.SpaceXl);

        _layout.ColumnCount = 1;
        _layout.RowCount = 4;
        _layout.Dock = DockStyle.Fill;
        _layout.AutoSize = true;
        _layout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _layout.Anchor = AnchorStyles.None;

        _lblTitle.AutoSize = true;
        UiStyles.ApplyTitleLabel(_lblTitle);
        _lblTitle.Margin = new Padding(0, 0, 0, UiStyles.SpaceSm);
        _lblTitle.TextAlign = ContentAlignment.MiddleCenter;
        _lblTitle.Anchor = AnchorStyles.None;

        _lblSubtitle.AutoSize = true;
        UiStyles.ApplySubtitleLabel(_lblSubtitle);
        _lblSubtitle.Margin = new Padding(0, 0, 0, UiStyles.SpaceLg);
        _lblSubtitle.MaximumSize = new Size(520, 0);
        _lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;
        _lblSubtitle.Anchor = AnchorStyles.None;

        _lblSteps.AutoSize = true;
        UiStyles.ApplyMutedLabel(_lblSteps);
        _lblSteps.Font = UiStyles.FontSubtitle;
        _lblSteps.Margin = new Padding(0, 0, 0, 20);
        _lblSteps.MaximumSize = new Size(480, 0);
        _lblSteps.TextAlign = ContentAlignment.MiddleLeft;
        _lblSteps.Anchor = AnchorStyles.None;

        _actions.AutoSize = true;
        _actions.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _actions.FlowDirection = FlowDirection.LeftToRight;
        _actions.WrapContents = true;
        _actions.Anchor = AnchorStyles.None;
        _actions.Margin = new Padding(0);

        UiStyles.ConfigurePrimaryToolbarButton(_btnSelectFile, 150);
        UiStyles.ConfigureToolbarButton(_btnImportCsv, 140);
        _btnSelectFile.Margin = new Padding(0, 0, UiStyles.SpaceMd, 0);
        _btnSelectFile.Click += (_, _) => SelectFileRequested?.Invoke(this, EventArgs.Empty);
        _btnImportCsv.Click += (_, _) => ImportCsvRequested?.Invoke(this, EventArgs.Empty);

        _actions.Controls.Add(_btnSelectFile);
        _actions.Controls.Add(_btnImportCsv);

        _layout.Controls.Add(_lblTitle, 0, 0);
        _layout.Controls.Add(_lblSubtitle, 0, 1);
        _layout.Controls.Add(_lblSteps, 0, 2);
        _layout.Controls.Add(_actions, 0, 3);

        Controls.Add(_layout);
        Resize += (_, _) => CenterLayout();
        LocalizationManager.LanguageChanged += (_, _) => { if (!IsDisposed) ApplyLocalization(); };
        ApplyLocalization();
    }

    public void ApplyLocalization()
    {
        _lblTitle.Text = L.Get("Welcome.Title");
        _lblSubtitle.Text = L.Get("Welcome.Subtitle");
        _lblSteps.Text = L.Get("Welcome.Steps");
        _btnSelectFile.Text = L.Get("Btn.SelectFile");
        _btnImportCsv.Text = L.Get("Btn.ImportCsv");
    }

    private void CenterLayout()
    {
        int maxW = Math.Min(560, Math.Max(280, ClientSize.Width - 48));
        _lblSubtitle.MaximumSize = new Size(maxW, 0);
        _lblSteps.MaximumSize = new Size(maxW, 0);

        int x = Math.Max(0, (ClientSize.Width - _layout.PreferredSize.Width) / 2);
        int y = Math.Max(0, (ClientSize.Height - _layout.PreferredSize.Height) / 2);
        _layout.Location = new Point(x, y);
        _layout.Size = _layout.PreferredSize;
    }

    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);
        if (Visible)
            CenterLayout();
    }
}
