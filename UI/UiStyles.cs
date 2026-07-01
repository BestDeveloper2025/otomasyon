namespace otomasyon.UI;

internal static class UiStyles
{
    public static readonly Color TopBarBack = Color.FromArgb(248, 249, 251);
    public static readonly Color BottomBarBack = Color.FromArgb(245, 246, 248);
    public static readonly Color Separator = Color.FromArgb(220, 223, 228);
    public static readonly Color MutedText = Color.FromArgb(70, 70, 70);
    public static readonly Color AccentText = Color.FromArgb(30, 100, 180);
    public static readonly Color PrimaryButton = Color.FromArgb(0, 102, 180);
    public static readonly Color PrimaryButtonHover = Color.FromArgb(0, 88, 158);
    public static readonly Color EmptyStateBack = Color.FromArgb(248, 250, 252);
    public static readonly Color CardBack = Color.FromArgb(252, 253, 255);
    public static readonly Color SectionHeader = Color.FromArgb(35, 45, 58);

    public const int TopBarHeight = 96;
    public const int LogoPanelWidth = 400;
    public const int ToolbarButtonHeight = 34;
    public const int SmallButtonHeight = 32;
    public const int RecipeActionsBarHeight = 44;

    public static void ConfigureToolbarButton(Button button, int width)
    {
        button.Width = width;
        button.Height = ToolbarButtonHeight;
        button.AutoSize = false;
        button.FlatStyle = FlatStyle.System;
        button.Margin = new Padding(0, 0, 8, 0);
        button.UseVisualStyleBackColor = true;
    }

    public static void ConfigureSmallButton(Button button, int width)
    {
        button.Width = width;
        button.Height = SmallButtonHeight;
        button.AutoSize = false;
        button.FlatStyle = FlatStyle.System;
        button.Margin = new Padding(0, 0, 8, 0);
        button.UseVisualStyleBackColor = true;
    }

    public static void ConfigurePrimaryToolbarButton(Button button, int width)
    {
        ConfigureToolbarButton(button, width);
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = PrimaryButton;
        button.ForeColor = Color.White;
        button.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        button.Cursor = Cursors.Hand;
    }

    public static Panel CreateToolbarDivider()
    {
        return new Panel
        {
            Width = 1,
            Height = 24,
            Margin = new Padding(4, 5, 10, 0),
            BackColor = Separator
        };
    }

    public static void ApplySectionHeader(Label label)
    {
        label.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
        label.ForeColor = SectionHeader;
    }
}
