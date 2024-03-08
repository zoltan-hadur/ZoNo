using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZoNo.ViewModels;

namespace ZoNo.Views
{
  public sealed partial class SettingsPage : Page
  {
    public SettingsPageViewModel ViewModel { get; }

    public SettingsPage()
    {
      ViewModel = App.GetService<SettingsPageViewModel>();
      InitializeComponent();
    }

    private async void Page_Loading(FrameworkElement sender, object args)
    {
      await ViewModel.LoadAsync();
    }
  }
}