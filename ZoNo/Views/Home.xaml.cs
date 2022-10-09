using System.Windows.Controls;
using ZoNo.ViewModels;

namespace ZoNo.Views
{
  /// <summary>
  /// Interaction logic for Home.xaml
  /// </summary>
  public partial class Home : UserControl, ViewBase<HomeVM>
  {
    public HomeVM ViewModel
    {
      get => (HomeVM)DataContext;
      set => DataContext = value;
    }

    public Home()
    {
      InitializeComponent();
    }

    private void UserDropDown_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
      e.Handled = true;
      UserDropDownPopUp.IsOpen = true;
    }
  }
}
