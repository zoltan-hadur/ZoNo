using Microsoft.UI.Xaml.Controls;
using ZoNo.ViewModels;

namespace ZoNo.Views
{
  public sealed partial class RuleEditor : UserControl
  {
    public RuleViewModel ViewModel { get; }

    public RuleEditor(RuleViewModel rule)
    {
      ViewModel = rule;
      InitializeComponent();
    }

    private void MenuFlyout_Opening(object sender, object e)
    {
      if (sender is MenuFlyout menuFlyout)
      {
        var dataContext = menuFlyout.Target?.DataContext ?? (menuFlyout.Target as ContentControl)?.Content;
        foreach (var item in menuFlyout.Items)
        {
          item.DataContext = dataContext;
        }
      }
    }
  }
}
