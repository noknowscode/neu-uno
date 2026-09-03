using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Neu.Uno;

/// <summary>
/// Maps a <see cref="bool"/> or a string to <see cref="Visibility"/> for the
/// library's own templates: true is visible, and a non-empty string is visible.
/// </summary>
/// <remarks>
/// <para>
/// Deliberately internal. It exists to work around two controls whose templates
/// cannot express what they need declaratively, not to be part of the library's
/// surface.
/// </para>
/// <para>
/// Both cases are the same shape - a control that leaves something to its
/// template and then does not drive it:
/// </para>
/// <list type="bullet">
/// <item><description>
/// Expander's reveal is normally driven by visual states declared in its default
/// template, but a Neu-templated Expander never enters them. Measured by
/// defaulting the body to visible and watching a collapsed Expander show its
/// content anyway. The body binds to <c>IsExpanded</c> instead.
/// </description></item>
/// <item><description>
/// ContentDialog's <c>ButtonsVisibilityStates</c> group does not hide the button
/// parts, so a dialog with no secondary button still renders an empty one. Each
/// button binds to its own <c>*ButtonText</c> instead, which is also
/// self-maintaining.
/// </description></item>
/// </list>
/// </remarks>
internal sealed partial class NeuVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
        => value switch
        {
            bool flag => flag ? Visibility.Visible : Visibility.Collapsed,
            string text => string.IsNullOrEmpty(text) ? Visibility.Collapsed : Visibility.Visible,
            null => Visibility.Collapsed,
            _ => Visibility.Visible,
        };

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException("NeuVisibilityConverter is one-way.");
}
