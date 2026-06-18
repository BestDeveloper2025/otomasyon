namespace otomasyon
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Localization.LocalizationManager.Initialize();
            Application.Run(new Form1());
        }
    }
}