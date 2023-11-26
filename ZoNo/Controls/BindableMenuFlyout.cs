using Microsoft.UI.Xaml.Controls;

namespace ZoNo.Controls
{
  public class BindableMenuFlyout : MenuFlyout
  {
    public BindableMenuFlyout()
    {
      Opening += BindableMenuFlyout_Opening;
    }

    private void BindableMenuFlyout_Opening(object sender, object e)
    {
      var dataContext = Target?.DataContext ?? (Target as ContentControl)?.Content;
      foreach (var item in Items)
      {
        item.DataContext = dataContext;
      }
    }
  }
}
