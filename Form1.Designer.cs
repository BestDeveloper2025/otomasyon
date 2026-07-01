#nullable enable

namespace otomasyon
{
    partial class Form1
    {
        private System.ComponentModel.IContainer? components = null;

        private Panel _topPanel = null!;
        private PictureBox _picLogo = null!;
        private FlowLayoutPanel _toolbarFlow = null!;
        private Button _btnSelectFile = null!;
        private Button _btnSetBaseEdge = null!;
        private Button _btnAddToRecipe = null!;
        private Button _btnSimulation = null!;
        private Button _btnImportCsv = null!;
        private Button _btnExportBatchCsv = null!;
        private Button _btnExportBatchDat = null!;
        private Button _btnSendFtp = null!;
        private Button _btnSettings = null!;
        private Label _lblFilePath = null!;

        private DrawingPanel _drawPanel = null!;

        private SplitContainer _splitMain = null!;
        private SplitContainer _splitRight = null!;

        private Panel _recipePanel = null!;
        private Label _lblRecipeHeader = null!;
        private ListView _lvRecipe = null!;
        private FlowLayoutPanel _recipeActions = null!;
        private Button _btnRemoveRecipe = null!;
        private Button _btnEditRecipe = null!;
        private Button _btnClearRecipe = null!;

        private TextBox _txtCoordinates = null!;

        private Panel _bottomPanel = null!;
        private Label _lblResults = null!;
        private Label _lblRecipeCount = null!;

        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            var uiFont = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            var headerFont = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point);

            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1180, 760);
            MinimumSize = new Size(900, 560);
            Text = "DXF Analysis and Recipe Tool";
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            Font = uiFont;
            BackColor = Color.White;

            // --- Üst araç çubuğu ---
            _topPanel = new Panel
            {
                Height = 56,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(248, 249, 251),
                Padding = new Padding(8, 8, 12, 8)
            };

            _toolbarFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Left,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(0)
            };

            _picLogo = new PictureBox
            {
                Dock = DockStyle.Left,
                Width = 140,
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.Transparent,
                Margin = new Padding(0, 0, 8, 0),
                Visible = false
            };

            _btnSelectFile = CreateToolbarButton("Select File", 100);
            _btnSetBaseEdge = CreateToolbarButton("Set Base Edge", 120);
            _btnSetBaseEdge.Enabled = false;
            _btnAddToRecipe = CreateToolbarButton("Add to Recipe", 120);
            _btnAddToRecipe.Enabled = false;
            _btnSimulation = CreateToolbarButton("Simulation", 110);
            _btnSimulation.Enabled = false;
            _btnImportCsv = CreateToolbarButton("Import CSV", 120);
            _btnExportBatchCsv = CreateToolbarButton("Batch CSV Export", 130);
            _btnExportBatchCsv.Enabled = false;
            _btnExportBatchDat = CreateToolbarButton("Batch DAT Export", 130);
            _btnExportBatchDat.Enabled = false;
            _btnSendFtp = CreateToolbarButton("Send via FTP", 120);
            _btnSendFtp.Enabled = false;

            _toolbarFlow.Controls.Add(_btnSelectFile);
            _toolbarFlow.Controls.Add(_btnSetBaseEdge);
            _toolbarFlow.Controls.Add(_btnAddToRecipe);
            _toolbarFlow.Controls.Add(_btnSimulation);
            _toolbarFlow.Controls.Add(_btnImportCsv);
            _toolbarFlow.Controls.Add(_btnExportBatchCsv);
            _toolbarFlow.Controls.Add(_btnExportBatchDat);
            _toolbarFlow.Controls.Add(_btnSendFtp);

            _btnSettings = CreateToolbarButton("Settings", 90);
            _btnSettings.Dock = DockStyle.Right;
            _btnSettings.Margin = new Padding(8, 0, 0, 0);

            _lblFilePath = new Label
            {
                Dock = DockStyle.Fill,
                Text = "No file selected yet.",
                TextAlign = ContentAlignment.MiddleLeft,
                AutoEllipsis = true,
                Padding = new Padding(12, 0, 0, 0),
                ForeColor = Color.FromArgb(70, 70, 70),
                Font = uiFont
            };

            _topPanel.Controls.Add(_lblFilePath);
            _topPanel.Controls.Add(_btnSettings);
            _topPanel.Controls.Add(_toolbarFlow);
            _topPanel.Controls.Add(_picLogo);

            // --- Alt durum çubuğu ---
            _bottomPanel = new Panel
            {
                Height = 44,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(245, 246, 248),
                Padding = new Padding(12, 0, 12, 0)
            };

            _lblResults = new Label
            {
                Dock = DockStyle.Fill,
                Text = "Contour edges: — | Radius: — | Arc: — | Circle: — | Entity: —",
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(50, 50, 50),
                Font = uiFont
            };

            _lblRecipeCount = new Label
            {
                Dock = DockStyle.Right,
                Width = 160,
                Text = "Recipe: 0 shapes",
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.FromArgb(30, 100, 180),
                Font = headerFont
            };

            _bottomPanel.Controls.Add(_lblResults);
            _bottomPanel.Controls.Add(_lblRecipeCount);

            // --- Çizim alanı ---
            _drawPanel = new DrawingPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

            // --- Reçete listesi ---
            _recipePanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8, 8, 8, 4)
            };

            _lblRecipeHeader = new Label
            {
                Dock = DockStyle.Top,
                Height = 28,
                Text = "Recipe",
                Font = headerFont,
                ForeColor = Color.FromArgb(35, 35, 35)
            };

            _recipeActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 4, 0, 0)
            };

            _btnRemoveRecipe = CreateSmallButton("Remove Selected", 120);
            _btnRemoveRecipe.Enabled = false;
            _btnEditRecipe = CreateSmallButton("Edit Selected", 110);
            _btnEditRecipe.Enabled = false;
            _btnClearRecipe = CreateSmallButton("Clear All", 110);
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
                BorderStyle = BorderStyle.FixedSingle,
                Font = uiFont
            };
            _lvRecipe.Columns.Add("#", 36, HorizontalAlignment.Right);
            _lvRecipe.Columns.Add("File", 160, HorizontalAlignment.Left);
            _lvRecipe.Columns.Add("Edge", 48, HorizontalAlignment.Center);
            _lvRecipe.Columns.Add("Glass thickness", 78, HorizontalAlignment.Center);
            _lvRecipe.Columns.Add("Qty", 44, HorizontalAlignment.Center);
            _lvRecipe.Columns.Add("Source", 72, HorizontalAlignment.Left);

            _recipePanel.Controls.Add(_lvRecipe);
            _recipePanel.Controls.Add(_recipeActions);
            _recipePanel.Controls.Add(_lblRecipeHeader);

            // --- Analiz metni ---
            _txtCoordinates = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 9F, FontStyle.Regular, GraphicsUnit.Point),
                BackColor = Color.FromArgb(252, 252, 252)
            };

            _splitRight = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterWidth = 6,
                FixedPanel = FixedPanel.Panel1,
                Panel1MinSize = 120,
                Panel2MinSize = 120
            };
            _splitRight.Panel1.Controls.Add(_recipePanel);
            _splitRight.Panel2.Controls.Add(_txtCoordinates);

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

        private static Button CreateToolbarButton(string text, int width)
        {
            return new Button
            {
                Text = text,
                Width = width,
                Height = 34,
                Margin = new Padding(0, 0, 8, 0),
                FlatStyle = FlatStyle.System,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point)
            };
        }

        private static Button CreateSmallButton(string text, int width)
        {
            return new Button
            {
                Text = text,
                Width = width,
                Height = 28,
                Margin = new Padding(0, 0, 8, 0),
                FlatStyle = FlatStyle.System,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point)
            };
        }
    }
}
