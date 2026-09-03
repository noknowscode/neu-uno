namespace Neu.Gallery;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();

#if DEBUG
        // Verification hook: pins the sheet to one theme so the light and dark
        // captures are deterministic without synthesized input, which cannot
        // reliably drive an Uno desktop window from a headless run.
        if (Environment.GetEnvironmentVariable("NEU_GALLERY_THEME") is { Length: > 0 } pinned &&
            Enum.TryParse<ElementTheme>(pinned, ignoreCase: true, out var theme))
        {
            Root.RequestedTheme = theme;
            ThemeToggle.Content = theme == ElementTheme.Dark ? "Light" : "Dark";
        }

        // Same reason: parks the sheet at a fixed offset so a capture can reach
        // the sections below the fold.
        if (Environment.GetEnvironmentVariable("NEU_GALLERY_SCROLL") is { Length: > 0 } raw &&
            double.TryParse(raw, out var offset))
        {
            Sheet.Loaded += (_, _) => Sheet.ChangeView(null, offset, null, disableAnimation: true);
        }
#endif
    }

    /// <summary>
    /// Flips the sheet between the two token sets. Scoped to the page root so
    /// the swatches re-resolve through ThemeResource and ActualThemeChanged.
    /// </summary>
    private void OnToggleTheme(object sender, RoutedEventArgs e)
    {
        var dark = Root.RequestedTheme != ElementTheme.Dark;

        Root.RequestedTheme = dark ? ElementTheme.Dark : ElementTheme.Light;
        ThemeToggle.Content = dark ? "Light" : "Dark";
    }
}
