using System.Globalization;

namespace AlgorithmsLab3.App.ViewModels;

internal static class DisplayFormat
{
    public static string Milliseconds(double value) => value switch
    {
        < 0.001 => $"{value * 1_000_000:F1} нс",
        < 1 => $"{value * 1000:F1} мкс",
        < 1000 => $"{value:F3} мс",
        _ => $"{value / 1000:F3} с",
    };

    public static string Number(double value) => value.ToString("N3", CultureInfo.CurrentCulture);

    public static string Percent(double value) => value.ToString("+0.0;-0.0;0.0", CultureInfo.CurrentCulture) + " %";

    public static string Limit(string text, int maxLength = 220) =>
        text.Length <= maxLength ? text : text[..maxLength] + "…";
}
