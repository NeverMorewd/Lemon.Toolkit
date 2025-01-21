using Avalonia.Controls;
using Avalonia.VisualTree;

namespace Lemon.Toolkit.Extensions
{
    public static class AvaloniauiExtension
    {
        public static T? FindVisualDescendant<T>(this Control control) where T : Control
        {
            foreach (var child in control.GetVisualChildren())
            {
                if (child is T foundControl)
                {
                    return foundControl;
                }

                if (child is Control childControl)
                {
                    var result = FindVisualDescendant<T>(childControl);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }
            return null;
        }
    }
}
