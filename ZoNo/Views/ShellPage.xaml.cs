using Microsoft.UI.Xaml.Controls;
using ZoNo.ViewModels;
using ZoNo.ViewModels.Import;

namespace ZoNo.Views
{
  public sealed partial class ShellPage : Page
  {
    public ShellViewModel ViewModel { get; }

    public ShellPage()
    {
      ViewModel = App.GetService<ShellViewModel>();
      InitializeComponent();

      ViewModel.NavigationService.Frame = NavigationFrame;
      ViewModel.NavigationViewService.Initialize(NavigationViewControl);
    }

    private void OnLoaded(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
      ViewModel.NavigationService.NavigateTo(typeof(ImportViewModel).FullName!);
    }
  }
}