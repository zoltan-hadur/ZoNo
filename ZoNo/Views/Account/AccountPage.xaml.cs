using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZoNo.ViewModels;

namespace ZoNo.Views.Account
{
  public sealed partial class AccountPage : Page
  {
    public AccountPageViewModel ViewModel { get; }

    public AccountPage()
    {
      ViewModel = App.GetService<AccountPageViewModel>();
      InitializeComponent();
    }

    private async void Page_Loading(FrameworkElement sender, object args)
    {
      await ViewModel.Load();
    }
  }
}
