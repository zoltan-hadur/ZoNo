using Microsoft.UI.Xaml.Controls;

namespace ZoNo.Contracts.Services
{
  public interface INavigationViewService
  {
    IList<object> MenuItems { get; }
    object SettingsItem { get; }

    void Initialize(NavigationView navigationView);
    NavigationViewItem GetSelectedItem(Type pageType);
  }
}