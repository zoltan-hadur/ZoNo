using Microsoft.UI.Xaml.Controls;
using ZoNo.ViewModels.Import;

namespace ZoNo.Views.Import
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