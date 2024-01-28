using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Xaml.Interactivity;
using Tracer.Contracts;
using Tracer.Utilities;
using ZoNo.Contracts.Services;

namespace ZoNo.Behaviors
{
  public class NavigationViewHeaderBehavior : Behavior<NavigationView>
  {
    private readonly ITraceFactory _traceFactory = App.GetService<ITraceFactory>();
    private static NavigationViewHeaderBehavior _current;
    private Page _currentPage;
    private readonly INavigationService _navigationService = App.GetService<INavigationService>();

    public static readonly DependencyProperty DefaultHeaderProperty =
        DependencyProperty.Register("DefaultHeader", typeof(object), typeof(NavigationViewHeaderBehavior), new PropertyMetadata(null, (d, e) => _current.UpdateHeader()));
    public static readonly DependencyProperty HeaderModeProperty =
        DependencyProperty.RegisterAttached("HeaderMode", typeof(bool), typeof(NavigationViewHeaderBehavior), new PropertyMetadata(NavigationViewHeaderMode.Always, (d, e) => _current.UpdateHeader()));
    public static readonly DependencyProperty HeaderContextProperty =
        DependencyProperty.RegisterAttached("HeaderContext", typeof(object), typeof(NavigationViewHeaderBehavior), new PropertyMetadata(null, (d, e) => _current.UpdateHeader()));
    public static readonly DependencyProperty HeaderTemplateProperty =
        DependencyProperty.RegisterAttached("HeaderTemplate", typeof(DataTemplate), typeof(NavigationViewHeaderBehavior), new PropertyMetadata(null, (d, e) => _current.UpdateHeaderTemplate()));

    public DataTemplate DefaultHeaderTemplate { get; set; }

    public object DefaultHeader
    {
      get => GetValue(DefaultHeaderProperty);
      set => SetValue(DefaultHeaderProperty, value);
    }

    public static NavigationViewHeaderMode GetHeaderMode(Page item)
    {
      return (NavigationViewHeaderMode)item.GetValue(HeaderModeProperty);
    }

    public static void SetHeaderMode(Page item, NavigationViewHeaderMode value)
    {
      item.SetValue(HeaderModeProperty, value);
    }

    public static object GetHeaderContext(Page item)
    {
      return item.GetValue(HeaderContextProperty);
    }

    public static void SetHeaderContext(Page item, object value)
    {
      item.SetValue(HeaderContextProperty, value);
    }

    public static DataTemplate GetHeaderTemplate(Page item)
    {
      return (DataTemplate)item.GetValue(HeaderTemplateProperty);
    }

    public static void SetHeaderTemplate(Page item, DataTemplate value)
    {
      item.SetValue(HeaderTemplateProperty, value);
    }

    protected override void OnAttached()
    {
      using var trace = _traceFactory.CreateNew();
      base.OnAttached();
      _navigationService.Navigated += OnNavigated;
      _current = this;
    }

    protected override void OnDetaching()
    {
      using var trace = _traceFactory.CreateNew();
      base.OnDetaching();
      _navigationService.Navigated -= OnNavigated;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
      using var trace = _traceFactory.CreateNew();
      if (sender is Frame frame && frame.Content is Page page)
      {
        trace.Debug(Format([page.GetType().Name]));
        _currentPage = page;
        UpdateHeader();
        UpdateHeaderTemplate();
      }
    }

    private void UpdateHeader()
    {
      using var trace = _traceFactory.CreateNew();
      if (_currentPage != null)
      {
        var headerMode = GetHeaderMode(_currentPage);
        if (headerMode == NavigationViewHeaderMode.Never)
        {
          AssociatedObject.Header = null;
          AssociatedObject.AlwaysShowHeader = false;
        }
        else
        {
          var headerFromPage = GetHeaderContext(_currentPage);
          if (headerFromPage != null)
          {
            AssociatedObject.Header = headerFromPage;
          }
          else
          {
            AssociatedObject.Header = DefaultHeader;
          }

          if (headerMode == NavigationViewHeaderMode.Always)
          {
            AssociatedObject.AlwaysShowHeader = true;
          }
          else
          {
            AssociatedObject.AlwaysShowHeader = false;
          }
        }
      }
    }

    private void UpdateHeaderTemplate()
    {
      using var trace = _traceFactory.CreateNew();
      if (_currentPage != null)
      {
        var headerTemplate = GetHeaderTemplate(_currentPage);
        AssociatedObject.HeaderTemplate = headerTemplate ?? DefaultHeaderTemplate;
      }
    }
  }
}