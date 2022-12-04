using Microsoft.UI.Xaml.Controls;
using ZoNo2.ViewModels;

namespace ZoNo2.Views
{
  public sealed partial class LoginPage : Page
  {
    public LoginViewModel ViewModel { get; }

    public LoginPage()
    {
      ViewModel = App.GetService<LoginViewModel>();
      InitializeComponent();
    }
  }
}