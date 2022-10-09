using System.Windows.Controls;
using ZoNo.ViewModels;

namespace ZoNo.Views
{
  /// <summary>
  /// Interaction logic for Login.xaml
  /// </summary>
  public partial class Login : UserControl, ViewBase<LoginVM>
  {
    public LoginVM ViewModel
    {
      get => (LoginVM)DataContext;
      set => DataContext = value;
    }

    public Login()
    {
      InitializeComponent();
    }
  }
}
