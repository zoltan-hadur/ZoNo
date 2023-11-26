using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using ZoNo.ViewModels;

namespace ZoNo.Views
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
    }

    private void Expenses_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (e.AddedItems.Count == 0) return;
      Expenses.ScrollIntoView(e.AddedItems[0]);
    }

    private void Expenses_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
      if (e.OriginalSource is FrameworkElement element)
      {
        ViewModel.ExpensesViewModel.EditExpenseCommand?.Execute(element.DataContext);
      }
    }
  }
}