using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZoNo2.ViewModels;

namespace ZoNo2.Views
{
  public sealed partial class AccountPage : Page
  {
    public AccountViewModel ViewModel { get; }

    public AccountPage()
    {
      ViewModel = App.GetService<AccountViewModel>();
      InitializeComponent();
    }

    private async void Page_Loading(FrameworkElement sender, object args)
    {
      await ViewModel.Load();
    }
  }
}
