using System.Text.Json;
using otomasyon.Localization;

namespace otomasyon.Settings;

public static class AppSettingsManager
{
    private static readonly string SettingsDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "otomasyon");

    private static readonly string SettingsPath = Path.Combine(SettingsDir, "settings.json");
    private static readonly string LegacyLanguagePath = Path.Combine(SettingsDir, "language.txt");

    public static MachineDirection MachineDirection { get; private set; } = MachineDirection.LeftToRight;

    public static event EventHandler? MachineDirectionChanged;
    public static event EventHandler? SettingsChanged;

    public static void Initialize()
    {
        Load();
    }

    public static void SetMachineDirection(MachineDirection direction, bool save = true)
    {
        if (MachineDirection == direction)
            return;

        MachineDirection = direction;
        if (save)
            Save();

        MachineDirectionChanged?.Invoke(null, EventArgs.Empty);
        SettingsChanged?.Invoke(null, EventArgs.Empty);
    }

    internal static void Save()
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var data = new SettingsData
            {
                Language = LocalizationManager.CurrentLanguage.ToCode(),
                MachineDirection = MachineDirection.ToCode()
            };
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // Kayıt başarısızsa sessizce devam et.
        }
    }

    private static void Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                string json = File.ReadAllText(SettingsPath);
                var data = JsonSerializer.Deserialize<SettingsData>(json);
                if (data is not null)
                {
                    LocalizationManager.SetLanguage(AppLanguageExtensions.FromCode(data.Language), save: false);
                    MachineDirection = MachineDirectionExtensions.FromCode(data.MachineDirection);
                    return;
                }
            }

            if (File.Exists(LegacyLanguagePath))
            {
                string code = File.ReadAllText(LegacyLanguagePath).Trim();
                LocalizationManager.SetLanguage(AppLanguageExtensions.FromCode(code), save: false);
            }
        }
        catch
        {
            // Varsayılan ayarlar.
        }
    }

    private sealed class SettingsData
    {
        public string Language { get; set; } = "en";
        public string MachineDirection { get; set; } = "ltr";
    }
}
