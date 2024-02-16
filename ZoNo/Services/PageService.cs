using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Tracer.Contracts;
using ZoNo.Contracts.Services;

namespace ZoNo.Services
{
  public class PageService : IPageService, ITopLevelPageService
  {
    public class Builder
    {
      private readonly ITraceFactory _traceFactory;
      private readonly PageService _pageService;

      public Builder(ITraceFactory traceFactory)
      {
        using var trace = traceFactory.CreateNew();
        _traceFactory = traceFactory;
        _pageService = new PageService(traceFactory);
      }

      public Builder Configure<VM, V>() where VM : ObservableObject where V : Page
      {
        using var trace = _traceFactory.CreateNew();
        trace.Debug(Format([GetTypeName(typeof(VM)), GetTypeName(typeof(V))]));
        _pageService.Configure<VM, V>();
        return this;
      }

      public PageService Build()
      {
        using var trace = _traceFactory.CreateNew();
        return _pageService;
      }
    }

    private readonly ITraceFactory _traceFactory;
    private readonly Dictionary<string, Type> _pages = [];

    private PageService(ITraceFactory traceFactory)
    {
      using var trace = traceFactory.CreateNew();
      _traceFactory = traceFactory;
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