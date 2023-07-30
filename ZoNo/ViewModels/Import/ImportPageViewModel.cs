using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.Specialized;
using Windows.Foundation.Collections;
using ZoNo.Contracts.Services;
using ZoNo.Models;
using ZoNo.Services;

namespace ZoNo.ViewModels.Import
{
  public partial class ImportPageViewModel : ObservableObject
  {
    private readonly IRulesService _rulesService;
    private readonly IRuleEvaluatorServiceBuilder _ruleEvaluatorServiceBuilder;
    private Dictionary<Transaction, ExpenseViewModel> _transactionToExpenseViewModel = new Dictionary<Transaction, ExpenseViewModel>();
    private IRuleEvaluatorService<Transaction, Expense>? _ruleEvaluatorService;
    private bool _isLoaded = false;

    public TransactionsViewModel TransactionsViewModel { get; }
    public ExpensesViewModel ExpensesViewModel { get; }

    [ObservableProperty]
    private Transaction _selectedTransaction;

    partial void OnSelectedTransactionChanged(Transaction value)
    {
      if (value == null) return;
      SelectedExpense = _transactionToExpenseViewModel[value];
    }

    [ObservableProperty]
    private ExpenseViewModel _selectedExpense;

    partial void OnSelectedExpenseChanged(ExpenseViewModel value)
    {
      if (value == null) return;
      SelectedTransaction = _transactionToExpenseViewModel.Single(x => x.Value == value).Key;
    }

    public ImportPageViewModel(
      IRulesService rulesService,
      IRuleEvaluatorServiceBuilder ruleEvaluatorServiceBuilder,
      TransactionsViewModel transactionsViewModel,
      ExpensesViewModel expensesViewModel)
    {
      _rulesService = rulesService;
      _ruleEvaluatorServiceBuilder = ruleEvaluatorServiceBuilder;
      TransactionsViewModel = transactionsViewModel;
      ExpensesViewModel = expensesViewModel;

      transactionsViewModel.TransactionsView.VectorChanged += async (s, e) =>
      {
        await Load();

        switch (e.CollectionChange)
        {
          case CollectionChange.ItemInserted:
            {
              var newIndex = (int)e.Index;
              var newTransaction = (TransactionsViewModel.TransactionsView[newIndex] as Transaction)!;

              var evaluatedExpense = new Expense();
              await _ruleEvaluatorService!.EvaluateRulesAsync(input: newTransaction, output: evaluatedExpense);
              var newExpense = new ExpenseViewModel(evaluatedExpense);

              _transactionToExpenseViewModel[newTransaction] = newExpense;
              ExpensesViewModel.Expenses.Insert(newIndex, newExpense);
            }
            break;
          case CollectionChange.ItemRemoved:
            {
              var expense = expensesViewModel.Expenses[(int)e.Index];
              var transaction = _transactionToExpenseViewModel.Single(x => x.Value == expense).Key;
              expensesViewModel.Expenses.Remove(expense);
              _transactionToExpenseViewModel.Remove(transaction);
            }
            break;
          case CollectionChange.Reset:
            {
              if (TransactionsViewModel.TransactionsView.Count > 0)
              {
                foreach (var (transaction, expense) in _transactionToExpenseViewModel.OrderBy(pair => TransactionsViewModel.TransactionsView.IndexOf(pair.Key)))
                {
                  var oldIndex = ExpensesViewModel.Expenses.IndexOf(expense);
                  var newIndex = TransactionsViewModel.TransactionsView.IndexOf(transaction);
                  if (newIndex != oldIndex)
                  {
                    ExpensesViewModel.Expenses.Move(oldIndex, newIndex);
                  }
                }
              }
            }
            break;
        }
      };
    }

    public async Task Load()
    {
      if (_isLoaded) return;

      var rules = await _rulesService.GetRulesAsync(RuleType.Splitwise);
      _ruleEvaluatorService = await _ruleEvaluatorServiceBuilder.BuildAsync<Transaction, Expense>(rules);

      _isLoaded = true;
    }
  }
}