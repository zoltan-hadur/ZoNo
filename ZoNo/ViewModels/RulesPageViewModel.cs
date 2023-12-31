using CommunityToolkit.Mvvm.ComponentModel;

namespace ZoNo.ViewModels
{
  public class RulesPageViewModel(
    RulesViewModel transactionRulesViewModel,
    RulesViewModel expenseRulesViewModel) : ObservableObject
  {
    public RulesViewModel TransactionRulesViewModel { get; } = transactionRulesViewModel;
    public RulesViewModel ExpenseRulesViewModel { get; } = expenseRulesViewModel;
  }
}
