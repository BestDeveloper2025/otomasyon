using otomasyon.Logging;

namespace otomasyon
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            AppLog.Initialize();

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (_, e) =>
                AppLog.Error("İşlenmeyen arayüz hatası", e.Exception.ToString());
            AppDomain.CurrentDomain.UnhandledException += (_, e) =>
                AppLog.Error("İşlenmeyen sistem hatası", e.ExceptionObject?.ToString());

            Localization.LocalizationManager.Initialize();
            Settings.AppSettingsManager.Initialize();
            Application.Run(new Form1());
            AppLog.Info("Uygulama kapatıldı");
        }
    }
}