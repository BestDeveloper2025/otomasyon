using System.Reflection;
using System.Text;

namespace otomasyon.Logging;

public static class AppLog
{
    private static readonly object Gate = new();
    private static readonly string LogDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "otomasyon");

    private static readonly string LogPath = Path.Combine(LogDir, "app.log");
    private const long MaxLogBytes = 5 * 1024 * 1024;
    private const int MaxRotatedFiles = 3;

    public static string LogFilePath => LogPath;

    public static void Initialize()
    {
        string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "?";
        Info("Uygulama başlatıldı", $"Sürüm={version}, OS={Environment.OSVersion}, Kullanıcı={Environment.UserName}");
    }

    public static void UserAction(string action, string? detail = null)
        => Write("ACTION", action, detail);

    public static void Info(string message, string? detail = null)
        => Write("INFO", message, detail);

    public static void Warn(string message, string? detail = null)
        => Write("WARN", message, detail);

    public static void Error(string message, string? detail = null)
        => Write("ERROR", message, detail);

    private static void Write(string level, string message, string? detail)
    {
        try
        {
            var line = new StringBuilder();
            line.Append('[').Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("] ");
            line.Append('[').Append(level).Append("] ");
            line.Append(Sanitize(message));
            if (!string.IsNullOrWhiteSpace(detail))
                line.Append(" | ").Append(Sanitize(detail));

            lock (Gate)
            {
                Directory.CreateDirectory(LogDir);
                RotateIfNeeded();
                File.AppendAllText(LogPath, line.ToString() + Environment.NewLine, Encoding.UTF8);
            }
        }
        catch
        {
            // Log yazılamazsa uygulama devam etsin.
        }
    }

    private static string Sanitize(string value)
        => value.Replace('\r', ' ').Replace('\n', ' ').Trim();

    private static void RotateIfNeeded()
    {
        if (!File.Exists(LogPath))
            return;

        if (new FileInfo(LogPath).Length < MaxLogBytes)
            return;

        for (int i = MaxRotatedFiles - 1; i >= 1; i--)
        {
            string source = Path.Combine(LogDir, $"app.log.{i}");
            string dest = Path.Combine(LogDir, $"app.log.{i + 1}");
            if (!File.Exists(source))
                continue;

            if (File.Exists(dest))
                File.Delete(dest);

            File.Move(source, dest);
        }

        string first = Path.Combine(LogDir, "app.log.1");
        if (File.Exists(first))
            File.Delete(first);

        File.Move(LogPath, first);
    }
}
