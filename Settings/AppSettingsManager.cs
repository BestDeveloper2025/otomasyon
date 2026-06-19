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

    public static bool IsConfigured { get; private set; }
    public static MachineDirection MachineDirection { get; private set; } = MachineDirection.LeftToRight;
    public static ShapeLimits Limits { get; private set; } = new();

    public static event EventHandler? MachineDirectionChanged;
    public static event EventHandler? SettingsChanged;

    public static void Initialize()
    {
        Load();
    }

    public static bool TrySave(
        AppLanguage language,
        MachineDirection machineDirection,
        double maxWidthMm,
        double maxHeightMm,
        out string? error)
    {
        error = null;

        if (maxWidthMm <= 0 || maxHeightMm <= 0)
        {
            error = L.Get("Error.LimitsPositive");
            return false;
        }

        bool directionChanged = MachineDirection != machineDirection;

        LocalizationManager.SetLanguage(language, save: false);
        MachineDirection = machineDirection;
        Limits = new ShapeLimits { MaxWidthMm = maxWidthMm, MaxHeightMm = maxHeightMm };
        IsConfigured = true;

        Persist();

        if (directionChanged)
            MachineDirectionChanged?.Invoke(null, EventArgs.Empty);

        SettingsChanged?.Invoke(null, EventArgs.Empty);
        return true;
    }

    public static void SetMachineDirection(MachineDirection direction, bool save = true)
    {
        if (MachineDirection == direction)
            return;

        MachineDirection = direction;
        if (save)
            Persist();

        MachineDirectionChanged?.Invoke(null, EventArgs.Empty);
        SettingsChanged?.Invoke(null, EventArgs.Empty);
    }

    internal static void Persist()
    {
        try
        {
            Directory.CreateDirectory(SettingsDir);
            var data = new SettingsData
            {
                Language = LocalizationManager.CurrentLanguage.ToCode(),
                MachineDirection = MachineDirection.ToCode(),
                MaxShapeWidthMm = Limits.MaxWidthMm,
                MaxShapeHeightMm = Limits.MaxHeightMm,
                IsConfigured = IsConfigured
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

                    if (data.IsConfigured
                        && data.MaxShapeWidthMm > 0
                        && data.MaxShapeHeightMm > 0)
                    {
                        IsConfigured = true;
                        MachineDirection = MachineDirectionExtensions.FromCode(data.MachineDirection);
                        Limits = new ShapeLimits
                        {
                            MaxWidthMm = data.MaxShapeWidthMm,
                            MaxHeightMm = data.MaxShapeHeightMm
                        };
                    }

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
        public double MaxShapeWidthMm { get; set; }
        public double MaxShapeHeightMm { get; set; }
        public bool IsConfigured { get; set; }
    }
}
