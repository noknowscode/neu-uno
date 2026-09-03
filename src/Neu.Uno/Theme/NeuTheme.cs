using System;
using Microsoft.UI.Xaml;

namespace Neu.Uno;

/// <summary>
/// Markup entry point for the Neu theme. Merge it into
/// <c>Application.Resources</c> to bring in the Neu tokens and control styles:
/// <code>
/// &lt;Application.Resources&gt;
///   &lt;ResourceDictionary&gt;
///     &lt;ResourceDictionary.MergedDictionaries&gt;
///       &lt;neu:NeuTheme /&gt;
///     &lt;/ResourceDictionary.MergedDictionaries&gt;
///   &lt;/ResourceDictionary&gt;
/// &lt;/Application.Resources&gt;
/// </code>
/// </summary>
/// <remarks>
/// Merges the color, metric/radius/spacing, type and depth tokens, then one
/// keyed-style dictionary per control. Implicit styles are step 9.
/// </remarks>
public partial class NeuTheme : ResourceDictionary
{
    private const string TokensColors = "ms-appx:///Neu.Uno/Theme/Tokens.Colors.xaml";
    private const string TokensMetrics = "ms-appx:///Neu.Uno/Theme/Tokens.Metrics.xaml";
    private const string TokensType = "ms-appx:///Neu.Uno/Theme/Tokens.Type.xaml";
    private const string TokensShadows = "ms-appx:///Neu.Uno/Theme/Tokens.Shadows.xaml";

    // One file per control, merged after the tokens they reference.
    private static readonly string[] ControlStyles =
    [
        "ms-appx:///Neu.Uno/Styles/Neu.Button.xaml",
        "ms-appx:///Neu.Uno/Styles/Neu.ToggleButton.xaml",
        "ms-appx:///Neu.Uno/Styles/Neu.CheckBox.xaml",
        "ms-appx:///Neu.Uno/Styles/Neu.RadioButton.xaml",
        "ms-appx:///Neu.Uno/Styles/Neu.ToggleSwitch.xaml",
        "ms-appx:///Neu.Uno/Styles/Neu.Slider.xaml",
        "ms-appx:///Neu.Uno/Styles/Neu.ProgressBar.xaml",
        "ms-appx:///Neu.Uno/Styles/Neu.ProgressRing.xaml",
        "ms-appx:///Neu.Uno/Styles/Neu.TextBox.xaml",
        "ms-appx:///Neu.Uno/Styles/Neu.PasswordBox.xaml",
        "ms-appx:///Neu.Uno/Styles/Neu.NumberBox.xaml",
        "ms-appx:///Neu.Uno/Styles/Neu.ComboBox.xaml",
    ];

    private Uri? _colorOverrideSource;
    private ResourceDictionary? _colorOverrides;

    /// <summary>
    /// Optional dictionary of color token overrides, merged after the Neu
    /// defaults so a consumer can restate any <c>Neu*</c> color without
    /// forking the theme. Give it the same <c>ThemeDictionaries</c> shape as
    /// <c>Tokens.Colors.xaml</c> and restate only the keys you are changing.
    /// </summary>
    public Uri? ColorOverrideSource
    {
        get => _colorOverrideSource;
        set
        {
            _colorOverrideSource = value;

            if (_colorOverrides is not null)
            {
                MergedDictionaries.Remove(_colorOverrides);
                _colorOverrides = null;
            }

            if (value is not null)
            {
                // Appended last so overrides win over the defaults merged in the
                // constructor. XAML sets properties after construction, so this
                // ordering holds for markup use.
                _colorOverrides = new ResourceDictionary { Source = value };
                MergedDictionaries.Add(_colorOverrides);
            }
        }
    }

    /// <summary>
    /// When <c>true</c>, also merges the implicit-style dictionary so stock
    /// controls pick up Neu templates without an explicit <c>Style</c>.
    /// </summary>
    /// <remarks>Honoured from step 9, once <c>Neu.Implicit.xaml</c> exists.</remarks>
    public bool UseImplicitStyles { get; set; }

    public NeuTheme()
    {
        MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(TokensColors) });
        MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(TokensMetrics) });
        MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(TokensType) });

        // Shadows last: its collections resolve the colors and metrics above
        // by StaticResource at parse time.
        MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(TokensShadows) });

        foreach (var style in ControlStyles)
        {
            MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(style) });
        }

        // Step 9 merges the implicit styles here, gated on UseImplicitStyles.
    }
}
