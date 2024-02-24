using Microsoft.UI.Xaml.Controls;
using ZoNo.ViewModels;

namespace ZoNo.Views
{
  // TODO: Set the URL for your privacy policy by updating SettingsPage_PrivacyTermsLink.NavigateUri in Resources.resw.
  public sealed partial class SettingsPage : Page
  {
    public SettingsPageViewModel ViewModel { get; }

    public SettingsPage()
    {
      ViewModel = App.GetService<SettingsPageViewModel>();
      InitializeComponent();
    }

    private async void Page_Loading(Microsoft.UI.Xaml.FrameworkElement sender, object args)
    {
      await ViewModel.LoadAsync();
    }
  }
}