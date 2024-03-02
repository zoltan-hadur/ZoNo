using CommunityToolkit.Mvvm.ComponentModel;
using ZoNo.Models;

namespace ZoNo.ViewModels
{
  public class RulesPageViewModel(
    RulesViewModel _transactionRulesViewModel,
    RulesViewModel _expenseRulesViewModel) : ObservableObject
  {
    public RulesViewModel TransactionRulesViewModel => _transactionRulesViewModel;
    public RulesViewModel ExpenseRulesViewModel => _expenseRulesViewModel;

    public void Load()
    {
      TransactionRulesViewModel.Load(RuleType.Transaction);
      ExpenseRulesViewModel.Load(RuleType.Expense);
    }
  }
}
