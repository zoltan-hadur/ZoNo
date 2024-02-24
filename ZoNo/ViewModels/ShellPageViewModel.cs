using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.UI.Xaml.Navigation;
using Tracer.Contracts;
using ZoNo.Contracts.Services;
using ZoNo.Views;

namespace ZoNo.ViewModels
{
  public partial class ShellPageViewModel(
    INavigationService navigationService,
    INavigationViewService navigationViewService,
    ITraceFactory traceFactory) : ObservableRecipient
  {
    public INavigationService NavigationService { get; } = navigationService;
    public INavigationViewService NavigationViewService { get; } = navigationViewService;
    private readonly ITraceFactory _traceFactory = traceFactory;

    [ObservableProperty]
    private object _selected;

    public void Initialize()
    {
      using var trace = _traceFactory.CreateNew();
      NavigationService.Navigated += OnNavigated;
    }

    private void OnNavigated(object sender, NavigationEventArgs e)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([e.SourcePageType]));
      if (e.SourcePageType == typeof(SettingsPage))
      {
        Selected = NavigationViewService.SettingsItem;
        return;
      }

      var selectedItem = NavigationViewService.GetSelectedItem(e.SourcePageType);
      if (selectedItem is not null)
      {
        Selected = selectedItem;
      }
    }
  }
}