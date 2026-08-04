#nullable enable

using otomasyon.UI;

namespace otomasyon
{
    partial class Form1
    {
        private System.ComponentModel.IContainer? components = null;

        private Panel _topPanel = null!;
        private Panel _logoPanel = null!;
        private Panel _toolbarSeparator = null!;
        private PictureBox _picLogo = null!;
        private FlowLayoutPanel _toolbarFlow = null!;
        private Button _btnSelectFile = null!;
        private Button _btnSetBaseEdge = null!;
        private Button _btnAddToRecipe = null!;
        private Button _btnImportCsv = null!;
        private Button _btnExportBatchCsv = null!;
        private Button _btnExportBatchDat = null!;
        private Button _btnSendFtp = null!;
        private Button _btnSettings = null!;
        private Label _lblToolbarFile = null!;
        private Label _lblToolbarShape = null!;
        private Label _lblToolbarOutput = null!;
        private Panel _settingsHost = null!;
        private Panel _topBarBody = null!;
        private Label _lblFilePath = null!;

        private DrawingPanel _drawPanel = null!;
        private HomeEmptyStatePanel _homeEmptyState = null!;
        private Button _btnCloseDxf = null!;

        private SplitContainer _splitMain = null!;
        private SplitContainer _splitRight = null!;

        private Panel _recipePanel = null!;
        private Label _lblRecipeHeader = null!;
        private Label _lblRecipeEmpty = null!;
        private ListView _lvRecipe = null!;
        private FlowLayoutPanel _recipeActions = null!;
        private Button _btnRemoveRecipe = null!;
        private Button _btnEditRecipe = null!;
        private Button _btnClearRecipe = null!;

        private TextBox _txtCoordinates = null!;
        private Panel _analysisPanel = null!;
        private Label _lblAnalysisHeader = null!;
        private Label _lblAnalysisEmpty = null!;

        private Panel _bottomPanel = null!;
        private Label _lblResults = null!;
        private Label _lblRecipeCount = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _picLogo.Image?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1180, 760);
            MinimumSize = new Size(900, 560);
            Text = "DXF Analysis and Recipe Tool";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            Font = UiStyles.FontUi;
            BackColor = UiStyles.Surface;

            // --- Üst araç çubuğu ---
            _topPanel = new Panel
            {
                Height = UiStyles.TopBarHeight,
                Dock = DockStyle.Top,
                BackColor = UiStyles.TopBarBack,
                Padding = new Padding(0, 0, 12, 0),
                MinimumSize = new Size(640, UiStyles.TopBarHeight)
            };

            _logoPanel = new Panel
            {
                Dock = DockStyle.Left,
                Width = UiStyles.LogoPanelWidth,
                Padding = new Padding(16, 8, 12, 8),
                BackColor = UiStyles.TopBarBack
            };

            _picLogo = new PictureBox
            {
                Dock = DockStyle.Fill,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = UiStyles.TopBarBack
            };

            _logoPanel.Controls.Add(_picLogo);

            _toolbarSeparator = new Panel
            {
                Dock = DockStyle.Left,
                Width = 1,
                BackColor = UiStyles.Separator
            };

            _toolbarFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Padding = new Padding(8, 10, 8, 4)
            };

            _topBarBody = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = UiStyles.TopBarBack,
                Padding = new Padding(0, 8, 0, 6)
            };

            _lblFilePath = new Label
            {
                Dock = DockStyle.Top,
                Height = 24,
                Text = "No file selected yet.",
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
                Padding = new Padding(12, 0, 8, 0),
                ForeColor = UiStyles.MutedText,
                Font = UiStyles.FontUi
            };

            _topBarBody.Controls.Add(_lblFilePath);
            _topBarBody.Controls.Add(_toolbarFlow);

            _btnSelectFile = CreateToolbarButton("Open DXF", 96);
            UiStyles.ConfigurePrimaryToolbarButton(_btnSelectFile, 96);
            _btnSetBaseEdge = CreateToolbarButton("Base Edge", 100);
            _btnSetBaseEdge.Enabled = false;
            _btnAddToRecipe = CreateToolbarButton("Add to Recipe", 118);
            _btnAddToRecipe.Enabled = false;

            _btnImportCsv = CreateToolbarButton("Open CSV", 96);
            _btnExportBatchCsv = CreateToolbarButton("Save CSV", 96);
            _btnExportBatchCsv.Enabled = false;
            _btnExportBatchDat = CreateToolbarButton("Save DAT", 96);
            _btnExportBatchDat.Enabled = false;
            _btnSendFtp = CreateToolbarButton("Send FTP", 96);
            _btnSendFtp.Enabled = false;

            _lblToolbarFile = UiStyles.CreateToolbarGroupLabel();
            _lblToolbarShape = UiStyles.CreateToolbarGroupLabel();
            _lblToolbarOutput = UiStyles.CreateToolbarGroupLabel();

            // Dosya → Şekil → Çıktı
            _toolbarFlow.Controls.Add(_lblToolbarFile);
            _toolbarFlow.Controls.Add(_btnSelectFile);
            _toolbarFlow.Controls.Add(_btnImportCsv);
            _toolbarFlow.Controls.Add(UiStyles.CreateToolbarDivider());
            _toolbarFlow.Controls.Add(_lblToolbarShape);
            _toolbarFlow.Controls.Add(_btnSetBaseEdge);
            _toolbarFlow.Controls.Add(_btnAddToRecipe);
            _toolbarFlow.Controls.Add(UiStyles.CreateToolbarDivider());
            _toolbarFlow.Controls.Add(_lblToolbarOutput);
            _toolbarFlow.Controls.Add(_btnExportBatchCsv);
            _toolbarFlow.Controls.Add(_btnExportBatchDat);
            _toolbarFlow.Controls.Add(_btnSendFtp);

            _btnSettings = CreateToolbarButton("Settings", 96);

            // Toolbar ile aynı dikey hizada: topBarBody padding (8) + host padding (10)
            _settingsHost = new Panel
            {
                Dock = DockStyle.Right,
                Width = 108,
                Padding = new Padding(0, 10, 0, 0),
                BackColor = UiStyles.TopBarBack
            };
            _btnSettings.Location = new Point(0, 0);
            _btnSettings.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            _settingsHost.Controls.Add(_btnSettings);
            _topBarBody.Controls.Add(_settingsHost);

            _topPanel.Controls.Add(_topBarBody);
            _topPanel.Controls.Add(_toolbarSeparator);
            _topPanel.Controls.Add(_logoPanel);

            // --- Alt durum çubuğu ---
            _bottomPanel = new Panel
            {
                Height = 44,
                Dock = DockStyle.Bottom,
                BackColor = UiStyles.BottomBarBack,
                Padding = new Padding(12, 0, 12, 0)
            };

            _lblResults = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Contour edges: — | Radius: — | Arc: — | Circle: — | Entity: —",
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = UiStyles.TextPrimary,
                Font = UiStyles.FontUi
            };

            _lblRecipeCount = new Label
            {
                Dock = DockStyle.Right,
                MinimumSize = new Size(140, 0),
                AutoSize = true,
                Text = "Recipe: 0 shapes",
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = UiStyles.AccentText,
                Font = UiStyles.FontHeader,
                Padding = new Padding(8, 0, 0, 0)
            };

            _bottomPanel.Controls.Add(_lblResults);
            _bottomPanel.Controls.Add(_lblRecipeCount);

            // --- Çizim alanı ---
            _drawPanel = new DrawingPanel
            {
                Dock = DockStyle.Fill,
                BackColor = UiStyles.CanvasBack,
                BorderStyle = BorderStyle.FixedSingle
            };

            _homeEmptyState = new HomeEmptyStatePanel();

            _btnCloseDxf = new Button
            {
                Text = "✕",
                Size = new Size(30, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = UiStyles.TextSecondary,
                BackColor = UiStyles.Surface,
                Cursor = Cursors.Hand,
                TabStop = false,
                Visible = false
            };
            _btnCloseDxf.FlatAppearance.BorderColor = UiStyles.BorderStrong;
            _btnCloseDxf.FlatAppearance.BorderSize = 1;
            _btnCloseDxf.FlatAppearance.MouseOverBackColor = Color.FromArgb(255, 235, 235);
            _btnCloseDxf.FlatAppearance.MouseDownBackColor = Color.FromArgb(255, 210, 210);

            _drawPanel.Controls.Add(_homeEmptyState);
            _drawPanel.Controls.Add(_btnCloseDxf);
            _drawPanel.Resize += (_, _) => PositionCloseDxfButton();
            PositionCloseDxfButton();

            // --- Reçete listesi ---
            _recipePanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 8, 8, 4),
                BackColor = UiStyles.CardBack
            };

            _lblRecipeHeader = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                Text = "Recipe"
            };
            UiStyles.ApplySectionHeader(_lblRecipeHeader);

            _lblRecipeEmpty = new Label
            {
                Dock = DockStyle.Fill,
                Text = "No recipe items yet.",
                TextAlign = ContentAlignment.TopCenter,
                ForeColor = UiStyles.MutedText,
                Font = UiStyles.FontUi,
                Padding = new Padding(12, 28, 12, 0)
            };

            _recipeActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = UiStyles.RecipeActionsBarHeight,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 6, 0, 0)
            };

            _btnRemoveRecipe = CreateSmallButton("Remove", 80);
            _btnRemoveRecipe.Enabled = false;
            _btnEditRecipe = CreateSmallButton("Edit", 80);
            _btnEditRecipe.Enabled = false;
            _btnClearRecipe = CreateSmallButton("Clear", 80);
            _btnClearRecipe.Enabled = false;

            _recipeActions.Controls.Add(_btnRemoveRecipe);
            _recipeActions.Controls.Add(_btnEditRecipe);
            _recipeActions.Controls.Add(_btnClearRecipe);

            _lvRecipe = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
                MultiSelect = false,
                BorderStyle = BorderStyle.None,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                Font = UiStyles.FontUi
            };
            _lvRecipe.Columns.Add("#", 36, HorizontalAlignment.Right);
            _lvRecipe.Columns.Add("File", 160, HorizontalAlignment.Left);
            _lvRecipe.Columns.Add("Edge", 48, HorizontalAlignment.Center);
            _lvRecipe.Columns.Add("Glass thickness", 78, HorizontalAlignment.Center);
            _lvRecipe.Columns.Add("Qty", 44, HorizontalAlignment.Center);
            _lvRecipe.Columns.Add("Source", 72, HorizontalAlignment.Left);

            _recipePanel.Controls.Add(_lvRecipe);
            _recipePanel.Controls.Add(_lblRecipeEmpty);
            _recipePanel.Controls.Add(_recipeActions);
            _recipePanel.Controls.Add(_lblRecipeHeader);

            // --- Analiz metni ---
            _analysisPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 8, 8, 8),
                BackColor = UiStyles.CardBack
            };

            _lblAnalysisHeader = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                Text = "Shape analysis"
            };
            UiStyles.ApplySectionHeader(_lblAnalysisHeader);

            _lblAnalysisEmpty = new Label
            {
                Dock = DockStyle.Top,
                Height = 36,
                Text = "Analysis data will appear here after you load a DXF file.",
                ForeColor = UiStyles.MutedText,
                Font = UiStyles.FontUi,
                Padding = new Padding(0, 0, 0, 4)
            };

            _txtCoordinates = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                BorderStyle = BorderStyle.FixedSingle,
                Font = UiStyles.FontMono,
                BackColor = UiStyles.LogBack
            };

            _analysisPanel.Controls.Add(_txtCoordinates);
            _analysisPanel.Controls.Add(_lblAnalysisEmpty);
            _analysisPanel.Controls.Add(_lblAnalysisHeader);

            _splitRight = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterWidth = 6,
                FixedPanel = FixedPanel.None
            };
            _splitRight.Panel1.Controls.Add(_recipePanel);
            _splitRight.Panel2.Controls.Add(_analysisPanel);

            _splitMain = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 6,
                FixedPanel = FixedPanel.None
            };
            _splitMain.Panel1.Controls.Add(_drawPanel);
            _splitMain.Panel2.Controls.Add(_splitRight);

            Controls.Add(_splitMain);
            Controls.Add(_bottomPanel);
            Controls.Add(_topPanel);

            Load += Form1_Load;
            Shown += Form1_Shown;
        }

        private void PositionCloseDxfButton()
        {
            if (_btnCloseDxf is null || _drawPanel is null)
                return;

            _btnCloseDxf.Location = new Point(
                Math.Max(8, _drawPanel.ClientSize.Width - _btnCloseDxf.Width - 8),
                8);
        }

        private static Button CreateToolbarButton(string text, int width)
        {
            var button = new Button { Text = text };
            UiStyles.ConfigureToolbarButton(button, width);
            return button;
        }

        private static Button CreateSmallButton(string text, int width)
        {
            var button = new Button { Text = text };
            UiStyles.ConfigureSmallButton(button, width);
            return button;
        }
    }
}
