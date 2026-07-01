namespace otomasyon;

/// <summary>Assets klasöründeki görseller ve diğer dosyalar.</summary>
public static class AppAssets
{
    public const string LogoFileName = "bestlogo.png";

    private static readonly string[] ImageExtensions = [".png", ".jpg", ".jpeg", ".bmp", ".webp"];

    public static string AssetsDirectory => Path.Combine(AppContext.BaseDirectory, "Assets");

    public static string? FindLogoPath()
    {
        foreach (string dir in EnumerateAssetDirectories())
        {
            string? path = FindLogoInDirectory(dir);
            if (path is not null)
                return path;
        }

        return null;
    }

    private static IEnumerable<string> EnumerateAssetDirectories()
    {
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string dir in CollectDirectories())
        {
            if (!Directory.Exists(dir))
                continue;

            string full = Path.GetFullPath(dir);
            if (seen.Add(full))
                yield return full;
        }
    }

    private static IEnumerable<string> CollectDirectories()
    {
        yield return AssetsDirectory;

        string? current = AppContext.BaseDirectory;
        for (int i = 0; i < 6 && current is not null; i++)
        {
            yield return Path.Combine(current, "Assets");
            current = Directory.GetParent(current)?.FullName;
        }
    }

    private static string? FindLogoInDirectory(string directory)
    {
        string primary = Path.Combine(directory, LogoFileName);
        if (File.Exists(primary))
            return primary;

        string baseName = Path.GetFileNameWithoutExtension(LogoFileName);
        foreach (string ext in ImageExtensions)
        {
            string candidate = Path.Combine(directory, baseName + ext);
            if (File.Exists(candidate))
                return candidate;
        }

        foreach (string file in Directory.EnumerateFiles(directory))
        {
            string ext = Path.GetExtension(file).ToLowerInvariant();
            if (ImageExtensions.Contains(ext))
                return file;
        }

        return null;
    }
}
