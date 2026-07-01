namespace otomasyon.UI;

internal static class DialogUiHelper
{
    public const int ButtonHeight = 36;
    public const int ButtonBarHeight = 58;

    public static FlowLayoutPanel CreateBottomButtonBar()
    {
        return new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            Height = ButtonBarHeight,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(12, 10, 12, 10),
            WrapContents = false
        };
    }

    public static void ConfigureButton(Button button, int width)
    {
        button.Width = width;
        button.Height = ButtonHeight;
        button.AutoSize = false;
        button.UseVisualStyleBackColor = true;
    }
}
