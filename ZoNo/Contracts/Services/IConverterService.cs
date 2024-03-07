namespace ZoNo.Contracts.Services
{
  public interface IConverterService
  {
    ZoNo.ViewModels.ExpenseViewModel ModelExpenseToViewModel(ZoNo.Models.Expense expense);
    Splitwise.Models.Expense ViewModelExpenseToSplitwise(ZoNo.ViewModels.ExpenseViewModel expense);
    ZoNo.ViewModels.ExpenseViewModel SplitwiseExpenseToViewModel(Splitwise.Models.Expense expense);
  }
}
