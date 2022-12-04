using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Navigation;
using ZoNo2.Contracts.Services;
using ZoNo2.Views;

namespace ZoNo2.ViewModels
{
  public class ShellViewModel : ObservableRecipient
  {
    private object? _selected;

    public INavigationService NavigationService { get; }

    public INavigationViewService NavigationViewService { get; }

    public object? Selected
    {
      get => _selected;
      set => SetProperty(ref _selected, value);
    }

    public ShellViewModel(INavigationService navigationService, INavigationViewService navigationViewService)
    {
      NavigationService = navigationService;
      NavigationService.Navigated += OnNavigated;
      NavigationViewService = navigationViewService;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
      if (e.SourcePageType == typeof(SettingsPage))
      {
        Selected = NavigationViewService.SettingsItem;
        return;
      }

      var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
      if (selectedItem != null)
      {
        Selected = selectedItem;
      }
    }
  }
}