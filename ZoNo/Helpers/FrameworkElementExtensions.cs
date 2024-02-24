using Microsoft.UI.Xaml;
using Tracer.Contracts;

namespace ZoNo.Helpers
{
  public static class FrameworkElementExtensions
  {
    private static readonly ITraceFactory _traceFactory = App.GetService<ITraceFactory>();

    public static void ReloadThemeResources(this FrameworkElement frameworkElement)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([frameworkElement.ActualTheme]));
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
