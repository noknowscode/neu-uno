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
                    // Only one section may take focus per run, or the two hooks
                    // race and the loser's specimen silently shows no ring.
                    if (Environment.GetEnvironmentVariable("NEU_GALLERY_FOCUS") == "button")
                    {
                        var accepted = FocusedButton.Focus(FocusState.Keyboard);
                        ButtonFocusReport.Text = $"Focus()={accepted} state={FocusedButton.FocusState}";
                    }

                    // Real focus, not GoToState. Verified in step 7: Focus()
                    // succeeds from a headless run and the ring paints, so the
                    // step 5 note claiming otherwise was wrong.
                };
                timer.Start();
            };
        }
#endif
    }
}
