using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Neu.Gallery.Views;

public sealed partial class SurfacesSection : UserControl
{
    public SurfacesSection()
    {
        this.InitializeComponent();

        // NEU_GALLERY_PERF=<n> fills the list with n rows for the scroll test.
        // Every Neu row is a ShadowContainer, so this is the measurement behind
        // the depth scale's list verification, not a demo.
        var count = 3;
        if (Environment.GetEnvironmentVariable("NEU_GALLERY_PERF") is { Length: > 0 } raw &&
            int.TryParse(raw, out var requested) && requested > 0)
        {
            count = requested;
            RowsCaption.Text = $"{count} rows, every one a ShadowContainer";
        }

        // NEU_GALLERY_PERF_STOCK=1 drops the Neu style and measures the same list
        // on stock rows, so the scroll number has something to be compared with.
        // Without a baseline "it scrolls at N%" says nothing about the depth scale.
        if (Environment.GetEnvironmentVariable("NEU_GALLERY_PERF_STOCK") == "1")
        {
            Rows.Style = null;
            Rows.ItemContainerStyle = null;
            RowsCaption.Text = $"{count} stock rows (baseline)";
        }

        Rows.ItemsSource = Enumerable.Range(1, count).Select(i => $"Row {i}").ToList();
        Rows.SelectedIndex = 1;

        // A dialog cannot be captured without being shown, and it cannot be
        // clicked open from a headless run.
        if (Environment.GetEnvironmentVariable("NEU_GALLERY_DIALOG") == "1")
        {
            Loaded += async (_, _) =>
            {
                await ShowNeuDialogAsync();
            };
        }

        // NEU_GALLERY_PERF_SCROLL=1 drives the list continuously so scrolling can
        // be measured without synthesized input, which cannot be aimed at this
        // window. A timer, not CompositionTarget.Rendering: a held Rendering hook
        // pins the compositor at display refresh and would be measuring itself.
        if (Environment.GetEnvironmentVariable("NEU_GALLERY_PERF_SCROLL") == "1")
        {
            Loaded += (_, _) =>
            {
                var scroll = FindScrollViewer(Rows);
                if (scroll is null)
                {
                    return;
                }

                var offset = 0d;
                var step = 24d;
                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(33) };
                timer.Tick += (_, _) =>
                {
                    offset += step;
                    if (offset >= scroll.ScrollableHeight || offset <= 0)
                    {
                        step = -step;
                        offset = Math.Clamp(offset, 0, Math.Max(scroll.ScrollableHeight, 0));
                    }

                    scroll.ChangeView(null, offset, null, disableAnimation: true);
                };
                timer.Start();
            };
        }
    }

    private async void OnShowDialog(object sender, RoutedEventArgs e) => await ShowNeuDialogAsync();

    private async Task ShowNeuDialogAsync()
    {
        try
        {
            await ShowNeuDialogCoreAsync();
        }
        catch (Exception ex)
        {
            // An async void Click/Tick handler swallows this, which is how a
            // dialog that never opens reads as "the template is broken".
            var frame = (ex.StackTrace ?? string.Empty)
                .Split('\n')
                .FirstOrDefault(l => l.Contains("at ", StringComparison.Ordinal))?.Trim() ?? "no frame";
            RowsCaption.Text = $"dialog failed: {ex.GetType().Name} | xamlRoot={(XamlRoot is null ? "null" : "ok")} | {frame}";
        }
    }

    private async Task ShowNeuDialogCoreAsync()
    {
        if (!Application.Current.Resources.TryGetValue("NeuContentDialogStyle", out var styleObj) ||
            styleObj is not Style neuDialogStyle)
        {
            RowsCaption.Text = "dialog failed: NeuContentDialogStyle not found in Application.Resources";
            return;
        }

        var dialog = new ContentDialog
        {
            // Assigned explicitly, never left to an implicit style: implicit
            // styles match on exact type, so they never reach a ContentDialog
            // subclass - which is how an app ends up with one stock dialog it
            // cannot explain.
            Style = neuDialogStyle,
            XamlRoot = XamlRoot,
            Title = "Discard the slab?",
            Content = "Every control is cut from one surface. Discarding resets the depth scale to its defaults.",
            PrimaryButtonText = "Discard",
            CloseButtonText = "Keep",
        };

        await dialog.ShowAsync();
    }

    private static ScrollViewer? FindScrollViewer(DependencyObject root)
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is ScrollViewer viewer)
            {
                return viewer;
            }

            if (FindScrollViewer(child) is { } nested)
            {
                return nested;
            }
        }

        return null;
    }
}
