using Windows.UI;

namespace Neu.Gallery.Views;

/// <summary>
/// WCAG 2.x relative luminance and contrast ratio. The C# twin of
/// tools/contrast-check.ps1, so the gallery reports the same numbers the
/// build-time check does.
/// </summary>
internal static class Contrast
{
    public static double Ratio(Color foreground, Color background)
    {
        var l1 = Luminance(foreground);
        var l2 = Luminance(background);
        if (l2 > l1)
        {
            (l1, l2) = (l2, l1);
        }

        return (l1 + 0.05) / (l2 + 0.05);
    }

    public static string ToHex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

    private static double Luminance(Color c) =>
        0.2126 * Channel(c.R / 255.0) +
        0.7152 * Channel(c.G / 255.0) +
        0.0722 * Channel(c.B / 255.0);

    private static double Channel(double srgb) =>
        srgb <= 0.03928 ? srgb / 12.92 : Math.Pow((srgb + 0.055) / 1.055, 2.4);
}
