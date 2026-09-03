using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Neu.Gallery.Views;

public sealed partial class InputsSection : UserControl
{
    public InputsSection()
    {
        this.InitializeComponent();

#if DEBUG
        // Focus diagnostic. Step 5 left it open whether the Focused visual state
        // is ever entered in a headless run; this reports the answer instead of
        // leaving a capture to be read as "the ring must be broken". The caption
        // shows what Focus() returned and the FocusState the control settled on.
        if (Environment.GetEnvironmentVariable("NEU_GALLERY_STATE") == "1")
        {
            Loaded += (_, _) =>
            {
                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1500) };
                timer.Tick += (t, _) =>
                {
                    ((DispatcherTimer)t!).Stop();
                    // Default owner of focus for a capture run; NEU_GALLERY_FOCUS=button
                    // hands it to the Buttons row instead. Only one may take it.
                    if (Environment.GetEnvironmentVariable("NEU_GALLERY_FOCUS") != "button")
                    {
                        var accepted = FocusProbeBox.Focus(FocusState.Keyboard);
                        FocusReport.Text = $"Focus()={accepted} state={FocusProbeBox.FocusState}";
                    }
                };
                timer.Start();
            };
        }
#endif
    }
}
