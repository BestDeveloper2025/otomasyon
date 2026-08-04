namespace otomasyon.UI;

/// <summary>
/// Ürün genelinde kullanılan tasarım dili (renk, tipografi, spacing, kontrol stilleri).
/// Marka birincil rengi değiştirilmemeli.
/// </summary>
internal static class UiStyles
{
    // --- Marka ---
    /// <summary>Best Makina marka mavisi — değiştirme.</summary>
    public static readonly Color BrandPrimary = Color.FromArgb(0, 102, 180);
    public static readonly Color BrandPrimaryHover = Color.FromArgb(0, 88, 158);
    public static readonly Color BrandPrimaryPressed = Color.FromArgb(0, 76, 138);
    public static readonly Color BrandPrimaryMuted = Color.FromArgb(230, 240, 250);

    // Geriye dönük adlar
    public static readonly Color PrimaryButton = BrandPrimary;
    public static readonly Color PrimaryButtonHover = BrandPrimaryHover;
    public static readonly Color AccentText = Color.FromArgb(0, 102, 180);

    // --- Yüzeyler ---
    public static readonly Color Surface = Color.White;
    public static readonly Color SurfaceAlt = Color.FromArgb(248, 249, 251);
    public static readonly Color SurfaceRaised = Color.FromArgb(252, 253, 255);
    public static readonly Color CanvasBack = Color.White;
    public static readonly Color TopBarBack = SurfaceAlt;
    public static readonly Color BottomBarBack = Color.FromArgb(245, 246, 248);
    public static readonly Color EmptyStateBack = Color.FromArgb(248, 250, 252);
    public static readonly Color CardBack = SurfaceRaised;
    public static readonly Color DialogBack = Surface;
    public static readonly Color DialogFooterBack = SurfaceAlt;
    public static readonly Color StatusBarBack = Color.FromArgb(245, 248, 252);
    public static readonly Color LogBack = Color.FromArgb(252, 252, 252);

    // --- Kenarlık / ayırıcı ---
    public static readonly Color Border = Color.FromArgb(220, 223, 228);
    public static readonly Color BorderStrong = Color.FromArgb(200, 205, 212);
    public static readonly Color Separator = Border;

    // --- Metin ---
    public static readonly Color TextPrimary = Color.FromArgb(32, 45, 64);
    public static readonly Color TextSecondary = Color.FromArgb(70, 78, 90);
    public static readonly Color TextMuted = Color.FromArgb(90, 98, 110);
    public static readonly Color TextHint = Color.FromArgb(120, 126, 136);
    public static readonly Color SectionHeader = Color.FromArgb(35, 45, 58);
    public static readonly Color MutedText = TextSecondary;
    public static readonly Color OnBrand = Color.White;

    // --- Durum ---
    public static readonly Color WarningBack = Color.FromArgb(255, 248, 210);
    public static readonly Color DangerText = Color.FromArgb(180, 40, 40);

    // --- Tipografi ---
    public const string FontFamily = "Segoe UI";
    public const string MonoFontFamily = "Consolas";

    public static readonly Font FontUi = new(FontFamily, 9F, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font FontUiBold = new(FontFamily, 9F, FontStyle.Bold, GraphicsUnit.Point);
    public static readonly Font FontHeader = new(FontFamily, 9.5F, FontStyle.Bold, GraphicsUnit.Point);
    public static readonly Font FontSection = new(FontFamily, 10F, FontStyle.Bold, GraphicsUnit.Point);
    public static readonly Font FontTitle = new(FontFamily, 16F, FontStyle.Bold, GraphicsUnit.Point);
    public static readonly Font FontSubtitle = new(FontFamily, 10F, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font FontSmall = new(FontFamily, 8.5F, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font FontSmallBold = new(FontFamily, 8.5F, FontStyle.Bold, GraphicsUnit.Point);
    public static readonly Font FontHint = new(FontFamily, 9F, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font FontHintItalic = new(FontFamily, 8.25F, FontStyle.Italic, GraphicsUnit.Point);
    public static readonly Font FontMono = new(MonoFontFamily, 9F, FontStyle.Regular, GraphicsUnit.Point);
    public static readonly Font FontButtonPrimary = new(FontFamily, 9F, FontStyle.Bold, GraphicsUnit.Point);

    // --- Ölçüler ---
    public const int TopBarHeight = 110;
    public const int LogoPanelWidth = 480;
    public const int ToolbarButtonHeight = 34;
    public const int SmallButtonHeight = 32;
    public const int DialogButtonHeight = 36;
    public const int DialogButtonBarHeight = 58;
    public const int RecipeActionsBarHeight = 44;
    public const int SpaceXs = 4;
    public const int SpaceSm = 8;
    public const int SpaceMd = 12;
    public const int SpaceLg = 16;
    public const int SpaceXl = 24;

    // --- Toolbar / küçük butonlar ---

    public static void ConfigureToolbarButton(Button button, int width)
    {
        button.Width = width;
        button.Height = ToolbarButtonHeight;
        button.AutoSize = false;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = BorderStrong;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = SurfaceAlt;
        button.BackColor = Surface;
        button.ForeColor = TextPrimary;
        button.Font = FontUi;
        button.Margin = new Padding(0, 0, SpaceSm, 0);
        button.Cursor = Cursors.Hand;
        button.UseVisualStyleBackColor = false;
    }

    public static void ConfigureSmallButton(Button button, int width)
    {
        button.Width = width;
        button.Height = SmallButtonHeight;
        button.AutoSize = false;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = BorderStrong;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = SurfaceAlt;
        button.BackColor = Surface;
        button.ForeColor = TextPrimary;
        button.Font = FontSmall;
        button.Margin = new Padding(0, 0, SpaceSm, 0);
        button.Cursor = Cursors.Hand;
        button.UseVisualStyleBackColor = false;
    }

    public static void ConfigurePrimaryToolbarButton(Button button, int width)
    {
        ConfigureToolbarButton(button, width);
        ApplyBrandButton(button);
    }

    public static void ConfigureDialogButton(Button button, int width)
    {
        button.Width = width;
        button.Height = DialogButtonHeight;
        button.AutoSize = false;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderColor = BorderStrong;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.MouseOverBackColor = SurfaceAlt;
        button.BackColor = Surface;
        button.ForeColor = TextPrimary;
        button.Font = FontUi;
        button.Margin = new Padding(SpaceSm, 0, 0, 0);
        button.Cursor = Cursors.Hand;
        button.UseVisualStyleBackColor = false;
    }

    public static void ConfigureDialogPrimaryButton(Button button, int width)
    {
        ConfigureDialogButton(button, width);
        ApplyBrandButton(button);
    }

    public static void ApplyBrandButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.FlatAppearance.MouseOverBackColor = BrandPrimaryHover;
        button.FlatAppearance.MouseDownBackColor = BrandPrimaryPressed;
        button.BackColor = BrandPrimary;
        button.ForeColor = OnBrand;
        button.Font = FontButtonPrimary;
        button.Cursor = Cursors.Hand;
        button.UseVisualStyleBackColor = false;
    }

    public static Panel CreateToolbarDivider()
    {
        return new Panel
        {
            Width = 1,
            Height = 24,
            Margin = new Padding(SpaceXs, 5, 10, 0),
            BackColor = Separator
        };
    }

    public static Label CreateToolbarGroupLabel()
    {
        return new Label
        {
            AutoSize = true,
            Font = FontSmallBold,
            ForeColor = TextMuted,
            Margin = new Padding(2, 9, 6, 0),
            TextAlign = ContentAlignment.MiddleLeft
        };
    }

    public static void FitToolbarButton(Button button, int minWidth = 88)
    {
        Size textSize = TextRenderer.MeasureText(button.Text, button.Font);
        button.Width = Math.Max(minWidth, textSize.Width + 28);
        button.Height = ToolbarButtonHeight;
    }

    public static void FitSmallButton(Button button, int minWidth = 72)
    {
        Size textSize = TextRenderer.MeasureText(button.Text, button.Font);
        button.Width = Math.Max(minWidth, textSize.Width + 24);
        button.Height = SmallButtonHeight;
    }

    // --- Etiket / diyalog yardımcıları ---

    public static void ApplySectionHeader(Label label)
    {
        label.Font = FontSection;
        label.ForeColor = SectionHeader;
    }

    public static void ApplyHintLabel(Label label)
    {
        label.Font = FontHint;
        label.ForeColor = TextMuted;
    }

    public static void ApplyExampleLabel(Label label)
    {
        label.Font = FontHintItalic;
        label.ForeColor = TextHint;
    }

    public static void ApplyMutedLabel(Label label)
    {
        label.Font = FontUi;
        label.ForeColor = TextMuted;
    }

    public static void ApplyTitleLabel(Label label)
    {
        label.Font = FontTitle;
        label.ForeColor = TextPrimary;
    }

    public static void ApplySubtitleLabel(Label label)
    {
        label.Font = FontSubtitle;
        label.ForeColor = TextSecondary;
    }

    public static Panel CreateSectionCard(int contentWidth = 460)
    {
        return new Panel
        {
            Width = contentWidth,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = CardBack,
            Padding = new Padding(SpaceMd),
            Margin = new Padding(0, 0, 0, SpaceMd)
        };
    }

    public static Label CreateSectionCardTitle()
    {
        return new Label
        {
            AutoSize = true,
            Font = FontHeader,
            ForeColor = SectionHeader,
            Margin = new Padding(0, 0, 0, SpaceSm),
            Dock = DockStyle.Top
        };
    }

    public static NumericUpDown CreateNumeric(decimal value, decimal min, decimal max, int decimalPlaces = 2, int width = 110)
    {
        return new NumericUpDown
        {
            Width = width,
            DecimalPlaces = decimalPlaces,
            Minimum = min,
            Maximum = max,
            Value = Math.Clamp(value, min, max),
            Font = FontUi
        };
    }

    public static void ApplyDialogChrome(Form form)
    {
        form.Font = FontUi;
        form.BackColor = DialogBack;
        form.ForeColor = TextPrimary;
    }

    public static FlowLayoutPanel CreateDialogButtonBar()
    {
        return new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = DialogButtonBarHeight,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(SpaceMd, 10, SpaceMd, 10),
            WrapContents = false,
            BackColor = DialogFooterBack
        };
    }
}
