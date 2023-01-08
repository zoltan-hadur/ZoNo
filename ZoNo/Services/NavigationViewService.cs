﻿using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System.Diagnostics.CodeAnalysis;
using ZoNo.Contracts.Services;
using ZoNo.Helpers;
using ZoNo.ViewModels;

namespace ZoNo.Services
{
  public class NavigationViewService : INavigationViewService
  {
    private readonly INavigationService _navigationService;
    private readonly IPageService _pageService;
    private NavigationView? _navigationView;

    public IList<object>? MenuItems => _navigationView?.MenuItems;

    public object? SettingsItem => _navigationView?.SettingsItem;

    public NavigationViewService(INavigationService navigationService, IPageService pageService)
    {
      _navigationService = navigationService;
      _pageService = pageService;
    }

    [MemberNotNull(nameof(_navigationView))]
    public void Initialize(NavigationView navigationView)
    {
      _navigationView = navigationView;
      _navigationView.ItemInvoked += OnItemInvoked;
    }

    public void UnregisterEvents()
    {
      if (_navigationView != null)
      {
        _navigationView.ItemInvoked -= OnItemInvoked;
      }
    }

    public NavigationViewItem? GetSelectedItem(Type pageType)
    {
      if (_navigationView != null)
      {
        return GetSelectedItem(_navigationView.MenuItems, pageType) ?? GetSelectedItem(_navigationView.FooterMenuItems, pageType);
      }

      return null;
    }

    private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
    {
      if (args.IsSettingsInvoked)
      {
        _navigationService.NavigateTo(typeof(SettingsViewModel).FullName!, infoOverride: new EntranceNavigationTransitionInfo());
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

    private NavigationViewItem? GetSelectedItem(IEnumerable<object> menuItems, Type pageType)
    {
      foreach (var item in menuItems.OfType<NavigationViewItem>())
      {
        if (IsMenuItemForPageType(item, pageType))
        {
          return item;
        }

        var selectedChild = GetSelectedItem(item.MenuItems, pageType);
        if (selectedChild != null)
        {
          return selectedChild;
        }
      }

      return null;
    }

    private bool IsMenuItemForPageType(NavigationViewItem menuItem, Type sourcePageType)
    {
      if (menuItem.GetValue(NavigationHelper.NavigateToProperty) is string pageKey)
      {
        return _pageService.GetPageType(pageKey) == sourcePageType;
      }

      return false;
    }
  }
}