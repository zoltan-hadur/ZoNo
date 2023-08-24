using Microsoft.UI.Xaml;

namespace ZoNo.Helpers
{
  public static class FrameworkElementExtensions
  {
    public static void ReloadThemeResources(this FrameworkElement frameworkElement)
    {
      switch (frameworkElement.ActualTheme)
      {
        case ElementTheme.Light:
          frameworkElement.RequestedTheme = ElementTheme.Dark;
          frameworkElement.RequestedTheme = ElementTheme.Light;
          break;
        default:
        case ElementTheme.Default:
        case ElementTheme.Dark:
          frameworkElement.RequestedTheme = ElementTheme.Light;
          frameworkElement.RequestedTheme = ElementTheme.Dark;
          break;
      }
      frameworkElement.RequestedTheme = ElementTheme.Default;
    }
  }
}
