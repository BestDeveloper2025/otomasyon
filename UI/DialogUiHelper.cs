namespace otomasyon.UI;

internal static class DialogUiHelper
{
    public const int ButtonHeight = UiStyles.DialogButtonHeight;
    public const int ButtonBarHeight = UiStyles.DialogButtonBarHeight;

    public static FlowLayoutPanel CreateBottomButtonBar()
        => UiStyles.CreateDialogButtonBar();

    public static void ConfigureButton(Button button, int width)
        => UiStyles.ConfigureDialogButton(button, width);

    public static void ConfigurePrimaryButton(Button button, int width)
        => UiStyles.ConfigureDialogPrimaryButton(button, width);
}
