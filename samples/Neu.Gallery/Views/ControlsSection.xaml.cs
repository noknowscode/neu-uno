using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Neu.Gallery.Views;

public sealed partial class ControlsSection : UserControl
{
    public ControlsSection()
    {
        this.InitializeComponent();

#if DEBUG
        // Capture hook. Pressed cannot be reached without a pointer, so under
        // NEU_GALLERY_STATE=1 the tagged specimen is put into it 1.5s after load -
        // late enough that the control's own state machine has settled and will
        // not immediately undo it. Focus is the harder case; see below.
        if (Environment.GetEnvironmentVariable("NEU_GALLERY_STATE") == "1")
        {
            Loaded += (_, _) =>
            {
                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1500) };
                timer.Tick += (t, _) =>
                {
                    ((DispatcherTimer)t!).Stop();
                    VisualStateManager.GoToState(PressedButton, "Pressed", true);
                    FocusedButton.Focus(FocusState.Keyboard);

                    // Real focus, not GoToState: FocusStates is driven by the
                    // control's own focus manager, which re-asserts Unfocused over a
                    // forced state. This only takes when the window is foreground, so
                    // a headless capture run shows this specimen at rest - the ring
                    // itself is verified separately (see decisions.md, step 5).
                };
                timer.Start();
            };
        }
#endif
    }
}
