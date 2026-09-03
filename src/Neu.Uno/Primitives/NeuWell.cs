using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Neu.Uno;

/// <summary>
/// A recess in the slab: an inset container for content that sits below the
/// surface rather than on it. Tracks, fields and grouped content live in one.
/// </summary>
/// <remarks>
/// Fixed at <see cref="NeuDepth.InsetMd"/> by design. A container whose depth
/// varies is a <see cref="NeuSurface"/>; a well is always a hole.
/// </remarks>
public partial class NeuWell : ContentControl
{
    public NeuWell()
    {
        DefaultStyleKey = typeof(NeuWell);
    }
}
