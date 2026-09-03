using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Neu.Uno;

/// <summary>One step of the Neu depth scale.</summary>
public enum NeuDepth
{
    /// <summary>No shadows. Disabled surfaces and high contrast.</summary>
    Flat,

    /// <summary>Offset 3, blur 6. List items, chips, inline inputs.</summary>
    RaisedSm,

    /// <summary>Offset 6, blur 12. Buttons, toggles, cards.</summary>
    RaisedMd,

    /// <summary>Offset 10, blur 24, spread -2. Dialogs and floating surfaces.</summary>
    RaisedLg,

    /// <summary>Offset 3, blur 6, inset. Pressed, checked, fields.</summary>
    InsetSm,

    /// <summary>Offset 6, blur 12, inset. Wells and tracks.</summary>
    InsetMd,
}

/// <summary>
/// A content container extruded from - or pressed into - the Neu slab.
/// </summary>
/// <remarks>
/// The template is a single <c>utu:ShadowContainer</c> carrying the surface brush,
/// as every Neu template must (CLAUDE.md rule 1). <see cref="Depth"/> maps onto a
/// visual state, and each state swaps the named <c>ShadowCollection</c> with a
/// <c>Setter</c>, so the whole depth scale stays overridable from markup.
/// </remarks>
public partial class NeuSurface : ContentControl
{
    public static readonly DependencyProperty DepthProperty =
        DependencyProperty.Register(
            nameof(Depth),
            typeof(NeuDepth),
            typeof(NeuSurface),
            new PropertyMetadata(NeuDepth.RaisedMd, OnDepthChanged));

    public NeuSurface()
    {
        DefaultStyleKey = typeof(NeuSurface);
    }

    /// <summary>How far the surface stands out of, or sinks into, the slab.</summary>
    public NeuDepth Depth
    {
        get => (NeuDepth)GetValue(DepthProperty);
        set => SetValue(DepthProperty, value);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        GoToDepthState(useTransitions: false);
    }

    private static void OnDepthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) =>
        ((NeuSurface)d).GoToDepthState(useTransitions: true);

    private void GoToDepthState(bool useTransitions) =>
        VisualStateManager.GoToState(this, StateName(Depth), useTransitions);

    // Spelled out rather than Depth.ToString() so the state names survive
    // trimming and stay greppable against Themes/Generic.xaml.
    private static string StateName(NeuDepth depth) => depth switch
    {
        NeuDepth.Flat => "Flat",
        NeuDepth.RaisedSm => "RaisedSm",
        NeuDepth.RaisedLg => "RaisedLg",
        NeuDepth.InsetSm => "InsetSm",
        NeuDepth.InsetMd => "InsetMd",
        _ => "RaisedMd",
    };
}
