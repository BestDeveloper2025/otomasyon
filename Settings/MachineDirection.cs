namespace otomasyon.Settings;

public enum MachineDirection
{
    LeftToRight,
    RightToLeft
}

public static class MachineDirectionExtensions
{
    public static string ToCode(this MachineDirection direction) => direction switch
    {
        MachineDirection.RightToLeft => "rtl",
        _ => "ltr"
    };

    public static MachineDirection FromCode(string? code) => code?.ToLowerInvariant() switch
    {
        "rtl" => MachineDirection.RightToLeft,
        _ => MachineDirection.LeftToRight
    };
}
