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
/// As of step 2 this merges the color, metric/radius/spacing and type tokens.
/// Depth collections land in step 3 and control styles from step 5 on.
/// </remarks>
public partial class NeuTheme : ResourceDictionary
{
    private const string TokensColors = "ms-appx:///Neu.Uno/Theme/Tokens.Colors.xaml";
    private const string TokensMetrics = "ms-appx:///Neu.Uno/Theme/Tokens.Metrics.xaml";
    private const string TokensType = "ms-appx:///Neu.Uno/Theme/Tokens.Type.xaml";

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

        // Step 3 merges Tokens.Shadows.xaml here; step 9 the implicit styles,
        // gated on UseImplicitStyles.
    }
}
