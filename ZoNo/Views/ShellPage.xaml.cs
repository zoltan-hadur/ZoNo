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
      InitializeComponent();

      ViewModel.NavigationService.Frame = NavigationFrame;
      ViewModel.NavigationViewService.Initialize(NavigationViewControl);
    }

    private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
      ViewModel.NavigationService.NavigateTo(typeof(ImportPageViewModel).FullName);
    }
  }
}