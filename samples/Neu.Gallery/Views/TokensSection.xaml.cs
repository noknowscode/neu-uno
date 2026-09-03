using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace Neu.Gallery.Views;

/// <summary>One row of the color specimen list.</summary>
public sealed record TokenSwatch(string Name, string Hex, Brush Fill, string Note, Brush NoteBrush);

/// <summary>One row of the depth or spacing specimen lists.</summary>
public sealed record MetricRow(string Name, string Value, double Size);

public sealed partial class TokensSection : UserControl
{
    /// <summary>
    /// Color specimens: token, the surface it is measured against (null when the
    /// pairing is not text), and the floor from CLAUDE.md rule 12 (null when the
    /// pairing is reported for information only).
    /// </summary>
    private static readonly (string Token, string? Against, double? Floor, string? Note)[] ColorSpecs =
    [
        ("NeuCanvas", null, null, "canvas and control share one color"),
        ("NeuSurface", null, null, "the slab every control is cut from"),
        ("NeuShadowLight", null, null, "top-left highlight"),
        ("NeuShadowDark", null, null, "bottom-right shade"),
        ("NeuInk", "NeuSurface", null, null),
        ("NeuInkSecondary", "NeuSurface", 4.5, null),
        ("NeuAccent", "NeuSurface", null, "fill only, never ink"),
        ("NeuAlert", "NeuSurface", 4.5, null),
        ("NeuOnAccent", "NeuAccent", 4.5, null),
        ("NeuOnAlert", "NeuAlert", 4.5, null),
    ];

    private static readonly (string Name, string[] Parts)[] DepthSpecs =
    [
        ("NeuRaisedSm", ["NeuDepthSmOffset", "NeuDepthSmBlur", "NeuDepthSmSpread"]),
        ("NeuRaisedMd", ["NeuDepthMdOffset", "NeuDepthMdBlur", "NeuDepthMdSpread"]),
        ("NeuRaisedLg", ["NeuDepthLgOffset", "NeuDepthLgBlur", "NeuDepthLgSpread"]),
        ("NeuInsetSm", ["NeuDepthSmOffset", "NeuDepthSmBlur", "NeuDepthSmSpread"]),
        ("NeuInsetMd", ["NeuDepthMdOffset", "NeuDepthMdBlur", "NeuDepthMdSpread"]),
    ];

    private static readonly string[] SpacingTokens =
        ["NeuSpace3", "NeuSpace5", "NeuSpace8", "NeuSpace13", "NeuSpace21", "NeuSpace34"];

    public TokensSection()
    {
        this.InitializeComponent();

        Loaded += (_, _) => Rebuild();
        ActualThemeChanged += (_, _) => Rebuild();
    }

    private void Rebuild()
    {
        DepthRows.ItemsSource = BuildDepthRows();
        SpacingBars.ItemsSource = BuildSpacingRows();
        ColorSwatches.ItemsSource = BuildColorSwatches();
    }

    // --- Color -------------------------------------------------------------

    private List<TokenSwatch> BuildColorSwatches()
    {
        var themeName = ActualTheme == ElementTheme.Dark ? "Dark" : "Light";
        var tokens = FindThemeDictionary(Application.Current.Resources, themeName);

        var neutral = (Brush)Application.Current.Resources["NeuInkSecondaryBrush"];
        var alert = (Brush)Application.Current.Resources["NeuAlertBrush"];

        if (tokens is null)
        {
            return [new TokenSwatch("Neu color tokens", "-", neutral, "not merged into Application.Resources", alert)];
        }

        var rows = new List<TokenSwatch>(ColorSpecs.Length);

        foreach (var (token, against, floor, note) in ColorSpecs)
        {
            if (tokens[token] is not Color color)
            {
                continue;
            }

            var text = note;
            var failed = false;

            if (against is not null && tokens[against] is Color surface)
            {
                var ratio = Contrast.Ratio(color, surface);
                var effectiveFloor = floor ?? FloorFor(token, themeName);

                text = $"{ratio:0.00}:1 on {against}";

                if (effectiveFloor is { } f)
                {
                    failed = ratio < f;
                    text += $" · floor {f:0.0} · {(failed ? "FAIL" : "pass")}";
                }
                else if (note is not null)
                {
                    text += $" · {note}";
                }
            }
            else if (token is "NeuShadowLight" or "NeuShadowDark")
            {
                var opacityKey = token + "Opacity";
                if (tokens[opacityKey] is double opacity)
                {
                    text = $"{note} · opacity {opacity:0.00}";
                }
            }

            rows.Add(new TokenSwatch(
                token,
                Contrast.ToHex(color),
                new SolidColorBrush(color),
                text ?? string.Empty,
                failed ? alert : neutral));
        }

        return rows;
    }

    /// <summary>
    /// The one floor that varies by theme: NeuInk on NeuSurface is 7:1 light,
    /// 10:1 dark (CLAUDE.md rule 12).
    /// </summary>
    private static double? FloorFor(string token, string theme) =>
        token is "NeuInk" ? (theme == "Dark" ? 10.0 : 7.0) : null;

    // --- Metrics -----------------------------------------------------------

    private static List<MetricRow> BuildDepthRows()
    {
        var rows = new List<MetricRow>();

        foreach (var (name, parts) in DepthSpecs)
        {
            var offset = Metric(parts[0]);
            var blur = Metric(parts[1]);
            var spread = Metric(parts[2]);
            var inner = name.StartsWith("NeuInset", StringComparison.Ordinal) ? " · inner" : string.Empty;

            rows.Add(new MetricRow(name, $"offset {offset:0.#} · blur {blur:0.#} · spread {spread:0.#}{inner}", 0));
        }

        rows.Add(new MetricRow("NeuFlat", "no shadows", 0));
        rows.Add(new MetricRow(
            "focus ring",
            $"spread {Metric("NeuFocusRingSpread"):0.#} · blur {Metric("NeuFocusRingBlur"):0.#} · NeuAccent",
            0));

        return rows;
    }

    private static List<MetricRow> BuildSpacingRows()
    {
        var rows = new List<MetricRow>(SpacingTokens.Length);

        foreach (var token in SpacingTokens)
        {
            var value = Metric(token);
            rows.Add(new MetricRow(token, $"{value:0.#}", value));
        }

        return rows;
    }

    private static double Metric(string key) =>
        Application.Current.Resources[key] is double d ? d : 0;

    // --- Token lookup ------------------------------------------------------

    /// <summary>
    /// Walks the merged dictionaries for the Neu color theme dictionary of the
    /// given theme, so the page can report both themes' literal values rather
    /// than whatever the ambient lookup happens to resolve.
    /// </summary>
    private static ResourceDictionary? FindThemeDictionary(ResourceDictionary root, string theme)
    {
        if (root.ThemeDictionaries.TryGetValue(theme, out var entry) &&
            entry is ResourceDictionary themed &&
            themed.ContainsKey("NeuInk"))
        {
            return themed;
        }

        foreach (var merged in root.MergedDictionaries)
        {
            if (FindThemeDictionary(merged, theme) is { } found)
            {
                return found;
            }
        }

        return null;
    }
}
