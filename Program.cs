namespace otomasyon
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Localization.LocalizationManager.Initialize();
            Settings.AppSettingsManager.Initialize();
            Application.Run(new Form1());
        }
    }
}