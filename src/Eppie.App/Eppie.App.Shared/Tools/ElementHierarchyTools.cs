using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Tuvi.App.Shared.Tools
{
	public static class ElementHierarchyTools
	{
		public static Frame GetMainFrame(FrameworkElement frameworkElement)
        {
            if (frameworkElement == null) return null;

            var frame = GetMainFrame(frameworkElement.Parent as FrameworkElement);
            if (frame != null)
            {
                return frame;
            }
            else
            {
                return frameworkElement as Frame;
            }
        }
    }
}
