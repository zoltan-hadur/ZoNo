using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using ZoNo.Contracts.Services;
using ZoNo.Contracts.ViewModels;
using ZoNo.Helpers;

namespace ZoNo.Services
{
  // For more information on navigation between pages see
  // https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/navigation.md
  public class NavigationService : INavigationService, ITopLevelNavigationService
  {
    private readonly IPageService _pageService;
    private object _lastParameterUsed;
    private Frame _frame;

    public event NavigatedEventHandler Navigated;

    public Frame Frame
    {
      get
      {
        return _frame;
      }

      set
      {
        UnregisterFrameEvents();
        _frame = value;
        RegisterFrameEvents();
      }
    }

    public NavigationService(IPageService pageService)
    {
      _pageService = pageService;
    }

    private void RegisterFrameEvents()
    {
      if (_frame != null)
      {
        _frame.Navigated += OnNavigated;
      }
    }

    private void UnregisterFrameEvents()
    {
      if (_frame != null)
      {
        _frame.Navigated -= OnNavigated;
      }
    }

    public bool NavigateTo(string pageKey, object parameter = null, NavigationTransitionInfo infoOverride = null)
    {
      var pageType = _pageService.GetPageType(pageKey);

      if (_frame != null && (_frame.Content?.GetType() != pageType || (parameter != null && !parameter.Equals(_lastParameterUsed))))
      {
        var vmBeforeNavigation = _frame.GetPageViewModel();
        var navigated = _frame.Navigate(pageType, parameter, infoOverride);
        if (navigated)
        {
          _lastParameterUsed = parameter;
          if (vmBeforeNavigation is INavigationAware navigationAware)
          {
            navigationAware.OnNavigatedFrom();
          }
        }

        return navigated;
      }

      return false;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
      if (sender is Frame frame)
      {
        if (frame.GetPageViewModel() is INavigationAware navigationAware)
        {
          navigationAware.OnNavigatedTo(e.Parameter);
        }

        Navigated?.Invoke(sender, e);
      }
    }
  }
}