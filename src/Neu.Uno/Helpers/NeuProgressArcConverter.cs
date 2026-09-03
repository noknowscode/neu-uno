using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Neu.Uno;

/// <summary>
/// Turns a <see cref="Microsoft.UI.Xaml.Controls.ProgressRing"/> value into the
/// <c>StrokeDashArray</c> that draws that fraction of a circle, for the
/// determinate arc in <c>Neu.ProgressRing.xaml</c>.
/// </summary>
/// <remarks>
/// <para>
/// A dash array is measured in multiples of the stroke thickness, and the arc
/// is drawn on the stroke centerline, so one full turn is
/// <c>PI * (Diameter - StrokeThickness) / StrokeThickness</c>. The converter
/// emits [drawn, remaining] against that total.
/// </para>
/// <para>
/// The geometry is set here rather than measured because the ring's template
/// fixes it. A consumer who resizes the ring should set matching
/// <see cref="Diameter"/> and <see cref="StrokeThickness"/> on their own
/// instance of this converter. <see cref="Minimum"/> and <see cref="Maximum"/>
/// mirror the control's range and default to the control's own 0-100; a ring
/// with a different range needs them set to match, since a converter sees only
/// the one bound value.
/// </para>
/// </remarks>
public sealed partial class NeuProgressArcConverter : IValueConverter
{
    /// <summary>Outer diameter of the ring, in pixels. Matches the template.</summary>
    public double Diameter { get; set; } = 48;

    /// <summary>Stroke thickness of the arc, in pixels. Matches the template.</summary>
    public double StrokeThickness { get; set; } = 5;

    /// <summary>Low end of the bound control's range.</summary>
    public double Minimum { get; set; }

    /// <summary>High end of the bound control's range.</summary>
    public double Maximum { get; set; } = 100;

    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var thickness = StrokeThickness > 0 ? StrokeThickness : 1;
        var total = Math.PI * (Diameter - thickness) / thickness;

        var range = Maximum - Minimum;
        var current = value is double d ? d : 0d;
        var fraction = range > 0 ? Math.Clamp((current - Minimum) / range, 0d, 1d) : 0d;

        var drawn = total * fraction;

        return new DoubleCollection { drawn, Math.Max(total - drawn, 0d) };
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException("NeuProgressArcConverter is one-way.");
}
