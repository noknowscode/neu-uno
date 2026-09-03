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
/// Step 1 scaffold: this dictionary is intentionally empty. Tokens land in
/// step 2, depth collections in step 3, control styles from step 5 on. The
/// two properties below are declared now so consumer markup written against
/// them does not have to change later.
/// </remarks>
public partial class NeuTheme : ResourceDictionary
{
    /// <summary>
    /// Optional dictionary of color token overrides, merged after the Neu
    /// defaults so a consumer can restate any <c>Neu*</c> color without
    /// forking the theme.
    /// </summary>
    /// <remarks>Honoured from step 2, once there are tokens to override.</remarks>
    public Uri? ColorOverrideSource { get; set; }

    /// <summary>
    /// When <c>true</c>, also merges the implicit-style dictionary so stock
    /// controls pick up Neu templates without an explicit <c>Style</c>.
    /// </summary>
    /// <remarks>Honoured from step 9, once <c>Neu.Implicit.xaml</c> exists.</remarks>
    public bool UseImplicitStyles { get; set; }

    public NeuTheme()
    {
        // Step 2 merges Tokens.Colors/Metrics/Type here, step 3 Tokens.Shadows,
        // step 9 the implicit styles gated on UseImplicitStyles.
    }
}
