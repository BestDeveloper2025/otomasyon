using System.Text;
using otomasyon.Localization;

namespace otomasyon.Simulation;

/// <summary>FTP gönderimi sırasında reçete dosyasını masaüstündeki otomasyonreceteler klasörüne yedekler.</summary>
public static class RecipeBackupSaver
{
    public const string BackupFolderName = "otomasyonreceteler";

    private static readonly UTF8Encoding Utf8WithBom = new(encoderShouldEmitUTF8Identifier: true);

    public static string GetBackupDirectoryPath()
    {
        string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        return Path.Combine(desktop, BackupFolderName);
    }

    public static bool TrySaveToDesktopFolder(string fileName, string content, out string savedPath, out string? error)
    {
        savedPath = string.Empty;
        error = null;

        string safeName = Path.GetFileName(fileName.Trim());
        if (string.IsNullOrWhiteSpace(safeName))
        {
            error = L.Get("Error.FtpInvalidFileName");
            return false;
        }

        try
        {
            string folderPath = GetBackupDirectoryPath();
            Directory.CreateDirectory(folderPath);
            savedPath = Path.Combine(folderPath, safeName);
            File.WriteAllText(savedPath, content, Utf8WithBom);
            return true;
        }
        catch (Exception ex)
        {
            error = L.F("Error.RecipeBackupFailed", ex.Message);
            return false;
        }
    }
}
