using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using ZoNo.Contracts.ViewModels;

namespace ZoNo.Views
{
  /// <summary>
  /// Interaction logic for Home.xaml
  /// </summary>
  public partial class HomeV : UserControl, VBase<IHomeVM>
  {
    public IHomeVM ViewModel
    {
      get => (IHomeVM)DataContext;
      set => DataContext = value;
    }

    public HomeV()
    {
      InitializeComponent();
    }
  }
}
