using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace Neu.Uno;

/// <summary>
/// Draws the determinate arc of a <see cref="ProgressRing"/> onto a stroked
/// <see cref="Shape"/>, by writing its <c>StrokeDashArray</c>.
/// </summary>
/// <remarks>
/// <para>
/// Point the shape at its ring and it keeps up on its own:
/// </para>
/// <code>
/// &lt;Ellipse Width="43" Height="43" StrokeThickness="5" StrokeDashCap="Flat"
///          neu:NeuProgressArc.Source="{Binding RelativeSource={RelativeSource TemplatedParent}}" /&gt;
/// </code>
/// <para>
/// This replaces the value converter the template used first, which could not
/// work. A converter sees one value, so it had to be told the ring's range up
/// front, and it defaulted to the control's own 0-100. Any app that gave its
/// ring a different range - <c>Maximum="1"</c> for a fraction, say - got an arc
/// scaled by a hundred and drew nothing, with no error and no way to correct it
/// short of retemplating, because the converter instance lived inside the
/// library's own template. This reads <see cref="ProgressRing.Minimum"/> and
/// <see cref="ProgressRing.Maximum"/> off the ring itself, so every range works.
/// </para>
/// <para>
/// Geometry comes from the shape - its <see cref="FrameworkElement.Width"/> and
/// <see cref="Shape.StrokeThickness"/> - rather than from properties that have
/// to be kept in step with the template by hand.
/// </para>
/// <para>
/// A dash array is measured in multiples of the stroke thickness and the stroke
/// is drawn on the centerline, so one full turn is
/// <c>PI * (Width - StrokeThickness) / StrokeThickness</c>. The pair written is
/// [drawn, remaining].
/// </para>
/// <para>
/// Give the shape <c>StrokeDashCap="Flat"</c>. A round cap on a zero-length dash
/// paints a full-stroke dot, so a ring sitting at its minimum shows progress it
/// does not have.
/// </para>
/// </remarks>
public static class NeuProgressArc
{
    /// <summary>The ring whose value this shape draws.</summary>
    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.RegisterAttached(
            "Source",
            typeof(ProgressRing),
            typeof(NeuProgressArc),
            new PropertyMetadata(null, OnSourceChanged));

    /// <summary>Gets the ring a shape is drawing.</summary>
    public static ProgressRing? GetSource(DependencyObject element) =>
        (ProgressRing?)element.GetValue(SourceProperty);

    /// <summary>Sets the ring a shape draws.</summary>
    public static void SetSource(DependencyObject element, ProgressRing? value) =>
        element.SetValue(SourceProperty, value);

    // The three subscriptions on the ring, kept so they can be dropped when the
    // shape is pointed somewhere else or torn down. Attached rather than held in
    // a static table, so nothing here outlives the shape it belongs to.
    private static readonly DependencyProperty TokensProperty =
        DependencyProperty.RegisterAttached(
            "Tokens",
            typeof(long[]),
            typeof(NeuProgressArc),
            new PropertyMetadata(null));

    private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not Shape shape)
        {
            return;
        }

        if (e.OldValue is ProgressRing old && shape.GetValue(TokensProperty) is long[] tokens)
        {
            old.UnregisterPropertyChangedCallback(RangeBaseValue, tokens[0]);
            old.UnregisterPropertyChangedCallback(RangeBaseMinimum, tokens[1]);
            old.UnregisterPropertyChangedCallback(RangeBaseMaximum, tokens[2]);
            shape.ClearValue(TokensProperty);
        }

        if (e.NewValue is not ProgressRing ring)
        {
            return;
        }

        void Redraw(DependencyObject _, DependencyProperty __) => Draw(shape, ring);

        shape.SetValue(TokensProperty, new[]
        {
            ring.RegisterPropertyChangedCallback(RangeBaseValue, Redraw),
            ring.RegisterPropertyChangedCallback(RangeBaseMinimum, Redraw),
            ring.RegisterPropertyChangedCallback(RangeBaseMaximum, Redraw),
        });

        // The shape may not have been measured yet, and Width can be NaN until
        // it is, so draw again once it has a size.
        shape.SizeChanged += (_, _) => Draw(shape, ring);

        Draw(shape, ring);
    }

    private static DependencyProperty RangeBaseValue => ProgressRing.ValueProperty;

    private static DependencyProperty RangeBaseMinimum => ProgressRing.MinimumProperty;

    private static DependencyProperty RangeBaseMaximum => ProgressRing.MaximumProperty;

    private static void Draw(Shape shape, ProgressRing ring)
    {
        var thickness = shape.StrokeThickness > 0 ? shape.StrokeThickness : 1;

        var diameter = double.IsNaN(shape.Width) || shape.Width <= 0
            ? shape.ActualWidth
            : shape.Width;

        if (diameter <= thickness)
        {
            return;
        }

        var turn = Math.PI * (diameter - thickness) / thickness;

        var range = ring.Maximum - ring.Minimum;
        var fraction = range > 0
            ? Math.Clamp((ring.Value - ring.Minimum) / range, 0d, 1d)
            : 0d;

        var drawn = turn * fraction;

        shape.StrokeDashArray = [drawn, Math.Max(turn - drawn, 0d)];
    }
}
