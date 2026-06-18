namespace otomasyon.Localization;

public enum AppLanguage
{
    English,
    Turkish,
    German
}

public static class AppLanguageExtensions
{
    public static string ToCode(this AppLanguage language) => language switch
    {
        AppLanguage.Turkish => "tr",
        AppLanguage.German => "de",
        _ => "en"
    };

    public static AppLanguage FromCode(string? code) => code?.ToLowerInvariant() switch
    {
        "tr" => AppLanguage.Turkish,
        "de" => AppLanguage.German,
        _ => AppLanguage.English
    };
}
