using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Tracer.Contracts;
using ZoNo.Contracts.Services;
using ZoNo.ViewModels;
using ZoNo.Views;

namespace ZoNo.Services
{
  public class PageService : IPageService
  {
    private readonly ITraceFactory _traceFactory;
    private readonly Dictionary<string, Type> _pages = [];

    public PageService(ITraceFactory traceFactory)
    {
      using var trace = traceFactory.CreateNew();
      _traceFactory = traceFactory;

      Configure<ImportPageViewModel, ImportPage>();
      Configure<RulesPageViewModel, RulesPage>();
      Configure<QueryPageViewModel, QueryPage>();
      Configure<AccountPageViewModel, AccountPage>();
      Configure<SettingsPageViewModel, SettingsPage>();
    }

    public Type GetPageType(string key)
    {
      using var trace = _traceFactory.CreateNew();
      Type pageType;
      lock (_pages)
      {
        if (!_pages.TryGetValue(key, out pageType))
        {
          throw new ArgumentException($"Page not found: {key}. Did you forget to call PageService.Configure?");
        }
      }
      trace.Debug(Format([key, GetTypeName(pageType)]));
      return pageType;
    }

    private void Configure<VM, V>() where VM : ObservableObject where V : Page
    {
      using var trace = _traceFactory.CreateNew();
      lock (_pages)
      {
        var key = typeof(VM).FullName;
        if (_pages.ContainsKey(key))
        {
          throw new ArgumentException($"The key {key} is already configured in PageService");
        }

        var type = typeof(V);
        if (_pages.Any(p => p.Value == type))
        {
          throw new ArgumentException($"This type is already configured with key {_pages.First(p => p.Value == type).Key}");
        }

        trace.Debug(Format([key, GetTypeName(type)]));
        _pages.Add(key, type);
      }
    }
  }
}