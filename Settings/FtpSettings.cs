namespace otomasyon.Settings;

/// <summary>FTP sunucu bağlantı bilgileri (reçete dosyası yükleme).</summary>
public sealed class FtpSettings
{
    public const int DefaultPort = 21;

    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = DefaultPort;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;

    /// <summary>Sunucudaki hedef klasör (ör. programs veya /data/recipes).</summary>
    public string RemoteDirectory { get; init; } = string.Empty;

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Host)
        && Port is > 0 and <= 65535
        && !string.IsNullOrWhiteSpace(Username);

    public string GetRemoteFilePath(string fileName)
    {
        string file = fileName.Replace('\\', '/').TrimStart('/');
        string dir = NormalizeRemoteDirectory(RemoteDirectory);
        return string.IsNullOrEmpty(dir) ? file : $"{dir}/{file}";
    }

    public static string NormalizeRemoteDirectory(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        return path.Replace('\\', '/').Trim().Trim('/');
    }
}
