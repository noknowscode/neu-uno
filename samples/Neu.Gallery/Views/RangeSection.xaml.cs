using Microsoft.UI.Xaml.Controls;

namespace Neu.Gallery.Views;

public sealed partial class RangeSection : UserControl
{
    public RangeSection()
    {
        this.InitializeComponent();

        // The two indeterminate specimens each pin the compositor at display
        // refresh, which would swamp any other measurement on the sheet. A perf
        // run turns them off so the number it reports belongs to the thing being
        // measured.
        if (Environment.GetEnvironmentVariable("NEU_GALLERY_PERF") is { Length: > 0 })
        {
            BusyBar.IsIndeterminate = false;
            BusyRing.IsActive = false;
        }
    }
}
