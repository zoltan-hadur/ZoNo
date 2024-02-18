using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Navigation;
using ZoNo.Contracts.Services;
using ZoNo.Views;

namespace ZoNo.ViewModels
{
  public partial class ShellPageViewModel(
    INavigationService navigationService,
    INavigationViewService navigationViewService) : ObservableRecipient
  {
    public INavigationService NavigationService { get; } = navigationService;
    public INavigationViewService NavigationViewService { get; } = navigationViewService;

    [ObservableProperty]
    private object _selected;

    public void Initialize()
    {
      NavigationService.Navigated += OnNavigated;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
      if (e.SourcePageType == typeof(SettingsPage))
      {
        Selected = NavigationViewService.SettingsItem;
        return;
      }

      var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
      if (selectedItem is not null)
      {
        Selected = selectedItem;
      }
    }
  }
}