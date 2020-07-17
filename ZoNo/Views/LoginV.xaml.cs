using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using ZoNo.Contracts.ViewModels;

namespace ZoNo.Views
{
  /// <summary>
  /// Interaction logic for Login.xaml
  /// </summary>
  public partial class LoginV : UserControl, VBase<ILoginVM>
  {
    public ILoginVM ViewModel
    {
      get => (ILoginVM)DataContext;
      set => DataContext = value;
    }

    public LoginV()
    {
      InitializeComponent();
    }
  }
}
