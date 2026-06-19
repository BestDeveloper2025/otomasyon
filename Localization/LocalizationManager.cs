using System.Globalization;
using System.Reflection;
using System.Text.Json;
using otomasyon.Settings;

namespace otomasyon.Localization;

public static class LocalizationManager
{
    private static Dictionary<string, Dictionary<string, string>> _catalog = new(StringComparer.Ordinal);

    public static AppLanguage CurrentLanguage { get; private set; } = AppLanguage.English;

    public static event EventHandler? LanguageChanged;

    public static void Initialize()
    {
        LoadCatalog();
    }

    public static void SetLanguage(AppLanguage language, bool save = true)
    {
        if (CurrentLanguage == language)
            return;

        CurrentLanguage = language;
        if (save)
            AppSettingsManager.Save();

        LanguageChanged?.Invoke(null, EventArgs.Empty);
    }

    public static string Get(string key)
    {
        if (_catalog.TryGetValue(key, out Dictionary<string, string>? entry) &&
            entry.TryGetValue(CurrentLanguage.ToCode(), out string? value) &&
            !string.IsNullOrEmpty(value))
            return value;

        if (entry is not null && entry.TryGetValue("en", out string? english) && !string.IsNullOrEmpty(english))
            return english;

        return key;
    }

    public static string Format(string key, params object[] args)
        => string.Format(CultureInfo.InvariantCulture, Get(key), args);

    private static void LoadCatalog()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith("Strings.json", StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException("Strings.json embedded resource not found.");

        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            throw new InvalidOperationException("Strings.json could not be loaded.");

        using var reader = new StreamReader(stream);
        string json = reader.ReadToEnd();
        var raw = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);
        _catalog = raw ?? new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
    }
}
