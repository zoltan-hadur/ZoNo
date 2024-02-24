using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Tracer.Contracts;

namespace ZoNo.Helpers
{
  // Helper class to set the navigation target for a NavigationViewItem.
  //
  // Usage in XAML:
  // <NavigationViewItem x:Uid="Shell_Main" Icon="Document" helpers:NavigationHelper.NavigateTo="AppName.ViewModels.MainViewModel" />
  //
  // Usage in code:
  // NavigationHelper.SetNavigateTo(navigationViewItem, typeof(MainViewModel).FullName);
  public class NavigationHelper
  {
    private static readonly ITraceFactory _traceFactory = App.GetService<ITraceFactory>();

    public static readonly DependencyProperty NavigateToProperty =
      DependencyProperty.RegisterAttached("NavigateTo", typeof(string), typeof(NavigationHelper), new PropertyMetadata(null));

    public static string GetNavigateTo(NavigationViewItem item)
    {
      using var trace = _traceFactory.CreateNew();
      var result = (string)item.GetValue(NavigateToProperty);
      trace.Debug(Format([result]));
      return result;
    }

    public static void SetNavigateTo(NavigationViewItem item, string value)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([value]));
      item.SetValue(NavigateToProperty, value);
    }
  }
}