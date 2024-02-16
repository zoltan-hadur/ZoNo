using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System.Diagnostics.CodeAnalysis;
using Tracer.Contracts;
using ZoNo.Contracts.Services;
using ZoNo.Helpers;
using ZoNo.ViewModels;

namespace ZoNo.Services
{
  public class NavigationViewService(
    INavigationService navigationService,
    IPageService pageService,
    ITraceFactory traceFactory) : INavigationViewService
  {
    private readonly INavigationService _navigationService = navigationService;
    private readonly IPageService _pageService = pageService;
    private readonly ITraceFactory _traceFactory = traceFactory;

    private NavigationView _navigationView;

    public IList<object> MenuItems => _navigationView?.MenuItems;

    public object SettingsItem => _navigationView?.SettingsItem;

    [MemberNotNull(nameof(_navigationView))]
    public void Initialize(NavigationView navigationView)
    {
      using var trace = _traceFactory.CreateNew();
      _navigationView = navigationView;
      _navigationView.ItemInvoked += OnItemInvoked;
    }

    public void UnregisterEvents()
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([_navigationView == null]));
      if (_navigationView != null)
      {
        _navigationView.ItemInvoked -= OnItemInvoked;
      }
    }

    public NavigationViewItem GetSelectedItem(Type pageType)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([_navigationView == null, GetTypeName(pageType)]));
      if (_navigationView != null)
      {
        return GetSelectedItem(_navigationView.MenuItems, pageType) ?? GetSelectedItem(_navigationView.FooterMenuItems, pageType);
      }

      return null;
    }

    private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([args.IsSettingsInvoked]));
      if (args.IsSettingsInvoked)
      {
        _navigationService.NavigateTo(typeof(SettingsPageViewModel).FullName, infoOverride: new EntranceNavigationTransitionInfo());
      }
      else
      {
        var selectedItem = args.InvokedItemContainer as NavigationViewItem;

        if (selectedItem?.GetValue(NavigationHelper.NavigateToProperty) is string pageKey)
        {
          _navigationService.NavigateTo(pageKey, infoOverride: new EntranceNavigationTransitionInfo());
        }
      }
    }

    private NavigationViewItem GetSelectedItem(IEnumerable<object> menuItems, Type pageType)
    {
      using var trace = _traceFactory.CreateNew();
      foreach (var item in menuItems.OfType<NavigationViewItem>())
      {
        if (IsMenuItemForPageType(item, pageType))
        {
          return item;
        }

        var selectedChild = GetSelectedItem(item.MenuItems, pageType);
        trace.Debug(Format([selectedChild == null]));
        if (selectedChild != null)
        {
          return selectedChild;
        }
      }

      return null;
    }

    private bool IsMenuItemForPageType(NavigationViewItem menuItem, Type sourcePageType)
    {
      using var trace = _traceFactory.CreateNew();
      if (menuItem.GetValue(NavigationHelper.NavigateToProperty) is string pageKey)
      {
        var result = _pageService.GetPageType(pageKey) == sourcePageType;
        trace.Debug(Format([GetTypeName(sourcePageType), result]));
        return result;
      }

      return false;
    }
  }
}