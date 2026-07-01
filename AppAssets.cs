namespace otomasyon;

/// <summary>Assets klasöründeki görseller ve diğer dosyalar.</summary>
public static class AppAssets
{
    public const string LogoFileName = "best-makina-logo.png";

    public static string AssetsDirectory => Path.Combine(AppContext.BaseDirectory, "Assets");

    public static string? FindLogoPath()
    {
        if (!Directory.Exists(AssetsDirectory))
            return null;

        string primary = Path.Combine(AssetsDirectory, LogoFileName);
        if (File.Exists(primary))
            return primary;

        string baseName = Path.GetFileNameWithoutExtension(LogoFileName);
        foreach (string ext in new[] { ".png", ".jpg", ".jpeg", ".bmp", ".webp" })
        {
            string candidate = Path.Combine(AssetsDirectory, baseName + ext);
            if (File.Exists(candidate))
                return candidate;
        }

        foreach (string file in Directory.EnumerateFiles(AssetsDirectory))
        {
            string ext = Path.GetExtension(file).ToLowerInvariant();
            if (ext is ".png" or ".jpg" or ".jpeg" or ".bmp" or ".webp")
                return file;
        }

        return null;
    }
}
