using System.Windows;
using ZoNo.ViewModels;

namespace ZoNo.Views
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window, ViewBase<MainWindowVM>
  {
    public MainWindowVM ViewModel
    {
      get => (MainWindowVM)DataContext;
      set => DataContext = value;
    }

    public MainWindow(MainWindowVM mainWindowVM)
    {
      InitializeComponent();
      ViewModel = mainWindowVM;
    }
  }
}
