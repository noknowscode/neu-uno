using Microsoft.UI.Xaml.Controls;

namespace Neu.Uno;

/// <summary>
/// A line scored into the slab: one hairline of shade with one hairline of
/// highlight under it. Divides the sections of a sheet.
/// </summary>
/// <remarks>
/// Deliberately not a <c>ShadowContainer</c>. A blurred inset shadow on a 2px
/// box has nothing to fall across, so the groove is drawn as the two shadow
/// token colors directly. It is the one place in the library where depth is not
/// a named <c>ShadowCollection</c>, because it carries no shadow at all.
/// </remarks>
public partial class NeuGroove : Control
{
    public NeuGroove()
    {
        DefaultStyleKey = typeof(NeuGroove);
    }
}
