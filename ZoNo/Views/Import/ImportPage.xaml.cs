using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZoNo.ViewModels.Import;

namespace ZoNo.Views.Import
{
  public sealed partial class ImportPage : Page
  {
    public ImportPageViewModel ViewModel { get; }

    public ImportPage()
    {
      ViewModel = App.GetService<ImportPageViewModel>();
      InitializeComponent();
    }

    private async void Page_Loading(FrameworkElement sender, object args)
    {
      await ViewModel.TransactionsViewModel.Load();
      await ViewModel.ExpensesViewModel.Load();
      await ViewModel.Load();
    }
  }
}