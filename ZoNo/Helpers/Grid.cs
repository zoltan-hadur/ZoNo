using Microsoft.UI.Xaml;

namespace ZoNo.Helpers
{
  public class Grid : DependencyObject
  {
    public static readonly DependencyProperty IsSharedSizeScopeProperty = DependencyProperty.RegisterAttached("IsSharedSizeScope", typeof(bool), typeof(Grid), new PropertyMetadata(false));
    public static readonly DependencyProperty SharedSizeScopeProperty = DependencyProperty.RegisterAttached("SharedSizeScope", typeof(Dictionary<int, double>), typeof(Grid), new PropertyMetadata(null));

    public static void SetIsSharedSizeScope(UIElement element, bool value)
    {
      element.SetValue(IsSharedSizeScopeProperty, value);
      if (value)
      {
        SetSharedSizeScope(element, new Dictionary<int, double>());
      }
    }

    public static bool GetIsSharedSizeScope(UIElement element)
    {
      return (bool)element.GetValue(IsSharedSizeScopeProperty);
    }

    public static void SetSharedSizeScope(UIElement element, Dictionary<int, double> value)
    {
      element.SetValue(SharedSizeScopeProperty, value);
    }

    public static Dictionary<int, double> GetSharedSizeScope(UIElement element)
    {
      return (Dictionary<int, double>)element.GetValue(SharedSizeScopeProperty);
    }
  }
}
