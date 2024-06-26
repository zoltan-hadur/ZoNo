﻿using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System.Diagnostics.CodeAnalysis;
using Tracer.Contracts;
using ZoNo.Contracts.Services;
using ZoNo.Helpers;
using ZoNo.ViewModels;

namespace ZoNo.Services
{
  public class NavigationViewService(
    INavigationService _navigationService,
    IPageService _pageService,
    ITraceFactory _traceFactory) : INavigationViewService
  {
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

    public NavigationViewItem GetSelectedItem(Type pageType)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([_navigationView is null, GetTypeName(pageType)]));
      if (_navigationView is not null)
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
        trace.Debug(Format([selectedChild is null]));
        if (selectedChild is not null)
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