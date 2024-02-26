using CommunityToolkit.Mvvm.ComponentModel;
using ZoNo.Models;

namespace ZoNo.ViewModels
{
  public class RulesPageViewModel(
    RulesViewModel transactionRulesViewModel,
    RulesViewModel expenseRulesViewModel) : ObservableObject
  {
    public RulesViewModel TransactionRulesViewModel { get; } = transactionRulesViewModel;
    public RulesViewModel ExpenseRulesViewModel { get; } = expenseRulesViewModel;

    public void Load()
    {
      TransactionRulesViewModel.Load(RuleType.Transaction);
      ExpenseRulesViewModel.Load(RuleType.Expense);
    }
  }
}
