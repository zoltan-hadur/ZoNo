using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;

namespace ZoNo.Contracts.Services
{
  public interface INavigationService
  {
    event NavigatedEventHandler Navigated;

    Frame Frame { get; set; }

    bool NavigateTo(string pageKey, object parameter = null, NavigationTransitionInfo infoOverride = null);
  }

  public interface ITopLevelNavigationService : INavigationService
  {

  }
}