#nullable enable

namespace otomasyon
{
    partial class Form1
    {
        /// <summary>
        /// Bileşenler için kullanılan kapsayıcı (dispose desteği).
        /// </summary>
        private System.ComponentModel.IContainer? components = null;

        private Panel _topPanel = null!;
        private Button _btnSelectFile = null!;
        private Label _lblFilePath = null!;

        private DrawingPanel _drawPanel = null!;

        private SplitContainer _splitMain = null!;

        private TextBox _txtCoordinates = null!;

        private Panel _bottomPanel = null!;
        private Label _lblResults = null!;

        /// <summary>
        /// Kaynakları serbest bırakır.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Tüm kontroller kod ile oluşturulur (Dock tabanlı yerleşim).
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();

            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 700);
            MinimumSize = new Size(800, 500);
            Text = "DXF Dosya Okuyucu ve Şekil Analiz Aracı";
            StartPosition = FormStartPosition.CenterScreen;

            // --- Üst: dosya seç ---
            _topPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Top,
                BackColor = SystemColors.Control
            };

            _btnSelectFile = new Button
            {
                Text = "Dosya Seç",
                Location = new Point(10, 10),
                Size = new Size(100, 30),
                Anchor = AnchorStyles.Left | AnchorStyles.Top
            };

            _lblFilePath = new Label
            {
                Location = new Point(120, 14),
                AutoSize = false,
                Size = new Size(860, 22),
                AutoEllipsis = true,
                Text = "Henüz dosya seçilmedi.",
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            _topPanel.Controls.Add(_btnSelectFile);
            _topPanel.Controls.Add(_lblFilePath);

            // --- Alt: özet ---
            _bottomPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(245, 245, 245)
            };

            _lblResults = new Label
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(12, 14, 12, 12),
                Text = "Toplam Kenar: — | Yay: — | Daire: — | Toplam Entity: —",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(40, 40, 40)
            };

            _bottomPanel.Controls.Add(_lblResults);

            // --- Orta: çizim + koordinat ---
            _drawPanel = new DrawingPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };

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

            // SplitterDistance / Panel*MinSize: genişlik 0 iken hata vermemesi için sadece Load’da ayarlanır.
            _splitMain = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterWidth = 6,
                FixedPanel = FixedPanel.None
            };
            _splitMain.Panel1.Controls.Add(_drawPanel);
            _splitMain.Panel2.Controls.Add(_txtCoordinates);

            // Z-order: önce fill, sonra alt/üst bandlar
            Controls.Add(_splitMain);
            Controls.Add(_bottomPanel);
            Controls.Add(_topPanel);

            Load += Form1_Load;
        }
    }
}
