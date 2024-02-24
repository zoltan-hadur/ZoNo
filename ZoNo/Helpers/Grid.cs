using Microsoft.UI.Xaml;
using Tracer.Contracts;

namespace ZoNo.Helpers
{
  public class Grid : DependencyObject
  {
    private static readonly ITraceFactory _traceFactory = App.GetService<ITraceFactory>();

    public static readonly DependencyProperty IsSharedSizeScopeProperty = DependencyProperty.RegisterAttached("IsSharedSizeScope", typeof(bool), typeof(Grid), new PropertyMetadata(false));
    public static readonly DependencyProperty SharedSizeScopeProperty = DependencyProperty.RegisterAttached("SharedSizeScope", typeof(Dictionary<int, double>), typeof(Grid), new PropertyMetadata(null));

    public static void SetIsSharedSizeScope(UIElement element, bool value)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([value]));
      element.SetValue(IsSharedSizeScopeProperty, value);
      if (value)
      {
        SetSharedSizeScope(element, new Dictionary<int, double>());
      }
    }

    public static bool GetIsSharedSizeScope(UIElement element)
    {
      using var trace = _traceFactory.CreateNew();
      var result = (bool)element.GetValue(IsSharedSizeScopeProperty);
      trace.Debug(Format([result]));
      return result;
    }

    public static void SetSharedSizeScope(UIElement element, Dictionary<int, double> value)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([value]));
      element.SetValue(SharedSizeScopeProperty, value);
    }

    public static Dictionary<int, double> GetSharedSizeScope(UIElement element)
    {
      using var trace = _traceFactory.CreateNew();
      var result = (Dictionary<int, double>)element.GetValue(SharedSizeScopeProperty);
      trace.Debug(Format([result.Count]));
      return result;
    }
  }
}
