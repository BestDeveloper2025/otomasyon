using System.Text.Json;
using otomasyon.Localization;
using otomasyon.Logging;

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
    public static FtpSettings Ftp { get; private set; } = new();

    /// <summary>Ayarlar'daki FTP gönderimi anahtarı. Açıkken toolbar ve FTP ayarları görünür.</summary>
    public static bool EnableFtpDelivery { get; private set; }

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
        bool enableFtpDelivery,
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
        EnableFtpDelivery = enableFtpDelivery;
        IsConfigured = true;

        Persist();

        if (directionChanged)
            MachineDirectionChanged?.Invoke(null, EventArgs.Empty);

        SettingsChanged?.Invoke(null, EventArgs.Empty);
        AppLog.UserAction(
            "Makine ayarları kaydedildi",
            $"Dil={language.ToCode()}, Yön={machineDirection.ToCode()}, MaxGenişlik={maxWidthMm:0.##}mm, MaxYükseklik={maxHeightMm:0.##}mm, FtpGönderim={enableFtpDelivery}");
        return true;
    }

    public static bool TrySaveFtp(FtpSettings ftp, out string? error)
    {
        error = null;

        if (string.IsNullOrWhiteSpace(ftp.Host))
        {
            error = L.Get("Error.FtpHostRequired");
            return false;
        }

        if (ftp.Port is < 1 or > 65535)
        {
            error = L.Get("Error.FtpPortInvalid");
            return false;
        }

        if (string.IsNullOrWhiteSpace(ftp.Username))
        {
            error = L.Get("Error.FtpUsernameRequired");
            return false;
        }

        Ftp = new FtpSettings
        {
            Host = ftp.Host.Trim(),
            Port = ftp.Port,
            Username = ftp.Username.Trim(),
            Password = ftp.Password,
            RemoteDirectory = FtpSettings.NormalizeRemoteDirectory(ftp.RemoteDirectory)
        };

        Persist();
        SettingsChanged?.Invoke(null, EventArgs.Empty);
        AppLog.UserAction(
            "FTP ayarları kaydedildi",
            $"Host={Ftp.Host}, Port={Ftp.Port}, Kullanıcı={Ftp.Username}, UzakDizin={Ftp.RemoteDirectory}");
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
                IsConfigured = IsConfigured,
                EnableFtpDelivery = EnableFtpDelivery,
                FtpHost = Ftp.Host,
                FtpPort = Ftp.Port,
                FtpUsername = Ftp.Username,
                FtpPassword = Ftp.Password,
                FtpRemoteDirectory = Ftp.RemoteDirectory
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

                    EnableFtpDelivery = data.EnableFtpDelivery;

                    Ftp = new FtpSettings
                    {
                        Host = data.FtpHost ?? string.Empty,
                        Port = data.FtpPort is >= 1 and <= 65535 ? data.FtpPort : FtpSettings.DefaultPort,
                        Username = data.FtpUsername ?? string.Empty,
                        Password = data.FtpPassword ?? string.Empty,
                        RemoteDirectory = FtpSettings.NormalizeRemoteDirectory(data.FtpRemoteDirectory)
                    };

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
        public bool EnableFtpDelivery { get; set; }
        public string? FtpHost { get; set; }
        public int FtpPort { get; set; } = FtpSettings.DefaultPort;
        public string? FtpUsername { get; set; }
        public string? FtpPassword { get; set; }
        public string? FtpRemoteDirectory { get; set; }
    }
}
