using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZoNo.ViewModels;

namespace ZoNo.Views
{
  public sealed partial class ShellPage : Page
  {
    public ShellPageViewModel ViewModel { get; }

    public ShellPage()
    {
      ViewModel = App.GetService<ShellPageViewModel>();
      ViewModel.Initialize();
      InitializeComponent();

      ViewModel.NavigationService.Frame = NavigationFrame;
      ViewModel.NavigationViewService.Initialize(NavigationViewControl);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
      ViewModel.NavigationService.NavigateTo(typeof(ImportPageViewModel).FullName);
    }
  }
}