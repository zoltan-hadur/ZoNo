using Microsoft.UI.Xaml.Controls;

using ZoNo2.ViewModels;

namespace ZoNo2.Views;

public sealed partial class ImportPage : Page
{
  public ImportViewModel ViewModel { get; }

  public ImportPage()
  {
    ViewModel = App.GetService<ImportViewModel>();
    InitializeComponent();
  }
}
