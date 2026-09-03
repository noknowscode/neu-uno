using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Neu.Uno;

namespace Neu.Gallery.Views;

public sealed partial class ImplicitSection : UserControl
{
    public ImplicitSection()
    {
        this.InitializeComponent();

        // The report is the evidence, not the capture. A bare Button carrying a
        // ShadowContainer in its template is the only thing that proves the
        // implicit style was both merged and matched - the flag alone would
        // only prove the setter ran.
        Loaded += (_, _) =>
        {
            var theme = Application.Current.Resources.MergedDictionaries
                .OfType<NeuTheme>()
                .FirstOrDefault();

            Report.Text =
                $"IMPLICIT={theme?.UseImplicitStyles.ToString() ?? "no NeuTheme"} " +
                $"bare Button templated Neu={HasShadowContainer(BareButton)} " +
                $"HIGHCONTRAST={App.HighContrastMerged}";
        };
    }

    private static bool HasShadowContainer(DependencyObject root)
    {
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);

            if (child is global::Uno.Toolkit.UI.ShadowContainer || HasShadowContainer(child))
            {
                return true;
            }
        }

        return false;
    }
}
