namespace otomasyon.Localization;

/// <summary>Kısa çeviri erişimi: L.Get("Key"), L.F("Key", arg0, ...)</summary>
public static class L
{
    public static string Get(string key) => LocalizationManager.Get(key);

    public static string F(string key, params object[] args) => LocalizationManager.Format(key, args);
}
