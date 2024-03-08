using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using Tracer.Contracts;
using ZoNo.Contracts.Services;
using ZoNo.Contracts.ViewModels;
using ZoNo.Helpers;

namespace ZoNo.Services
{
  // For more information on navigation between pages see
  // https://github.com/microsoft/TemplateStudio/blob/main/docs/WinUI/navigation.md
  public class NavigationService(
    IPageService _pageService,
    ITraceFactory _traceFactory) : INavigationService
  {
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
        using var trace = _traceFactory.CreateNew();
        UnregisterFrameEvents();
        _frame = value;
        RegisterFrameEvents();
      }
    }

    private void RegisterFrameEvents()
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([_frame is null]));
      if (_frame is not null)
      {
        _frame.Navigated += OnNavigated;
      }
    }

    private void UnregisterFrameEvents()
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([_frame is null]));
      if (_frame is not null)
      {
        _frame.Navigated -= OnNavigated;
      }
    }

    public bool NavigateTo(string pageKey, object parameter = null, NavigationTransitionInfo infoOverride = null)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([pageKey, parameter, infoOverride]));
      var pageType = _pageService.GetPageType(pageKey);

      if (_frame is not null && (_frame.Content?.GetType() != pageType || (parameter is not null && !parameter.Equals(_lastParameterUsed))))
      {
        var vmBeforeNavigation = _frame.GetPageViewModel();
        var navigated = _frame.Navigate(pageType, parameter, infoOverride);
        trace.Debug(Format([navigated]));
        if (navigated)
        {
          _lastParameterUsed = parameter;
          if (vmBeforeNavigation is INavigationAware navigationAware)
          {
            trace.Debug($"Calling {nameof(navigationAware.OnNavigatedFrom)}");
            navigationAware.OnNavigatedFrom();
          }
        }

        return navigated;
      }

      return false;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
      using var trace = _traceFactory.CreateNew();
      if (sender is Frame frame)
      {
        if (frame.GetPageViewModel() is INavigationAware navigationAware)
        {
          trace.Debug($"Calling {nameof(navigationAware.OnNavigatedTo)}");
          navigationAware.OnNavigatedTo(e.Parameter);
        }

        trace.Debug($"Invoking {nameof(Navigated)} event");
        Navigated?.Invoke(sender, e);
      }
    }
  }
}