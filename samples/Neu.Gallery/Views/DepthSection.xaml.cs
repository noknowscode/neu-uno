using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Neu.Uno;

namespace Neu.Gallery.Views;

public sealed partial class DepthSection : UserControl
{
    public DepthSection()
    {
        this.InitializeComponent();

#if DEBUG
        // Step 4 gate. The depth specimens above prove a VSM Setter applies at
        // template time; this proves it still applies when the state changes
        // well after first paint, which is what a press has to do. Set
        // NEU_GALLERY_PRESS=1 and the swap specimen inverts 1.5s after load.
        if (Environment.GetEnvironmentVariable("NEU_GALLERY_PRESS") == "1")
        {
            Loaded += (_, _) =>
            {
                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1500) };
                timer.Tick += (t, _) =>
                {
                    ((DispatcherTimer)t!).Stop();
                    // Only Depth changes. Nothing else on this row may move,
                    // or a before/after pixel diff measures layout, not depth.
                    SwapSpecimen.Depth = NeuDepth.InsetSm;
                };
                timer.Start();
            };
        }
#endif
    }
}
