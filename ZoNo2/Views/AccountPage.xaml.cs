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
  }
}
