using Microsoft.UI.Xaml.Controls;
using ZoNo.ViewModels;

namespace ZoNo.Views
{
  public sealed partial class ImportPage : Page
  {
    public ImportViewModel ViewModel { get; }

    public ImportPage()
    {
      ViewModel = App.GetService<ImportViewModel>();
      InitializeComponent();
    }
  }
}