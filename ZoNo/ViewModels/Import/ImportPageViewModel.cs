using CommunityToolkit.Mvvm.ComponentModel;

namespace ZoNo.ViewModels.Import
{
  public partial class ImportPageViewModel : ObservableObject
  {
    public TransactionsViewModel TransactionsViewModel { get; }

    public ImportPageViewModel(TransactionsViewModel transactionsViewModel)
    {
      TransactionsViewModel = transactionsViewModel;
    }
  }
}