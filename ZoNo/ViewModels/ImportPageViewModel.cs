using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using Windows.Foundation.Collections;
using ZoNo.Contracts.Services;
using ZoNo.Helpers;
using ZoNo.Models;

namespace ZoNo.ViewModels
{
  public partial class ImportPageViewModel : ObservableObject
  {
    private readonly IRulesService _rulesService;
    private readonly IRuleEvaluatorServiceBuilder _ruleEvaluatorServiceBuilder;
    private IRuleEvaluatorService<Transaction, Expense> _ruleEvaluatorService;
    private readonly BlockingCollection<(int Index, Transaction Transaction)> _newTransactions = [];
    private readonly BlockingCollection<Transaction> _transactionsToRemove = [];
    private readonly SemaphoreSlim _guard = new(initialCount: 1, maxCount: 1);

    public TransactionsViewModel TransactionsViewModel { get; }
    public ExpensesViewModel ExpensesViewModel { get; }

    [ObservableProperty]
    private Transaction _selectedTransaction;

    partial void OnSelectedTransactionChanged(Transaction value)
    {
      SelectedExpense = value == null ? null : ExpensesViewModel.Expenses.Single(expense => expense.Id == value.Id);
    }

    [ObservableProperty]
    private ExpenseViewModel _selectedExpense;

    partial void OnSelectedExpenseChanged(ExpenseViewModel value)
    {
      SelectedTransaction = value == null ? null : TransactionsViewModel.TransactionsView.Cast<Transaction>().Single(transaction => transaction.Id == value.Id);
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

      TransactionsViewModel.LoadExcelDocumentsStarted += TransactionsViewModel_LoadExcelDocumentsStarted;
      TransactionsViewModel.LoadExcelDocumentsFinished += TransactionsViewModel_LoadExcelDocumentsFinished;
      TransactionsViewModel.TransactionsView.VectorChanged += (s, e) =>
      {
        if (e.CollectionChange == CollectionChange.ItemInserted)
        {
          // To track order of insertions when less than 30 are added at once
          _newTransactions.Add(((int)e.Index, TransactionsViewModel.TransactionsView[(int)e.Index] as Transaction));
        }
      };
      TransactionsViewModel.TransactionsView.VectorChanged += TransactionsView_VectorChanged;
      ExpensesViewModel.Expenses.CollectionChanged += Expenses_CollectionChangedAsync;
    }

    private async void TransactionsViewModel_LoadExcelDocumentsStarted(object sender, EventArgs e)
    {
      using var guard = await LockGuard.CreateAsync(_guard, TimeSpan.Zero);
      var rules = await _rulesService.GetRulesAsync(RuleType.Splitwise);
      _ruleEvaluatorService = await _ruleEvaluatorServiceBuilder.BuildAsync<Transaction, Expense>(rules);
    }

    private async void TransactionsViewModel_LoadExcelDocumentsFinished(object sender, EventArgs e)
    {
      using var guard = await LockGuard.CreateAsync(_guard, TimeSpan.FromSeconds(5));
      while (_transactionsToRemove.TryTake(out var transaction))
      {
        try
        {
          TransactionsViewModel.TransactionsView.Remove(transaction);
        }
        catch (ArgumentOutOfRangeException)
        {
          // When deleting last item, there is an exception
          TransactionsViewModel.TransactionsView.Refresh();
        }
      }
    }

    private async Task AddExpenseFromTransaction(Transaction transaction, int? index = null)
    {
      var evaluatedExpense = new Expense()
      {
        Id = transaction.Id,
        With = [],
        Category = Categories.Uncategorized.General,
        Description = transaction.PartnerName,
        Currency = transaction.Currency,
        Cost = -transaction.Amount,
        Date = transaction.TransactionTime,
        Group = "Non-group expenses"
      };
      var result = await _ruleEvaluatorService!.EvaluateRulesAsync(input: transaction, output: evaluatedExpense);
      var expense = ExpenseViewModel.FromModel(evaluatedExpense);
      if (index.HasValue)
      {
        ExpensesViewModel.Expenses.Insert(index.Value, expense);
      }
      else
      {
        ExpensesViewModel.Expenses.Add(expense);
      }
      if (result.RemoveThisElementFromList)
      {
        _transactionsToRemove.Add(transaction);
      }
    }

    private async void TransactionsView_VectorChanged(IObservableVector<object> sender, IVectorChangedEventArgs e)
    {
      using var guard = await LockGuard.CreateAsync(_guard, TimeSpan.FromSeconds(5));
      switch (e.CollectionChange)
      {
        // When less than 30 items were added at once
        case CollectionChange.ItemInserted:
          {
            var (index, newTransaction) = _newTransactions.Take();
            if (newTransaction == null)
            {
              throw new Exception($"Transaction for index \"{index}\" is null!");
            }
            await AddExpenseFromTransaction(newTransaction, index);
          }
          break;
        // When less than 30 items were removed at once
        case CollectionChange.ItemRemoved:
          {
            // Transaction is already removed, need to remove expense too
            if (TransactionsViewModel.TransactionsView.Count < ExpensesViewModel.Expenses.Count)
            {
              var oldIndex = (int)e.Index;
              var oldExpense = ExpensesViewModel.Expenses[oldIndex];
              ExpensesViewModel.Expenses.Remove(oldExpense);
            }
          }
          break;
        case CollectionChange.Reset:
          {
            // When 30 or more items were added at once
            if (TransactionsViewModel.TransactionsView.Count > ExpensesViewModel.Expenses.Count)
            {
              var newTransactions = TransactionsViewModel.TransactionsView.Cast<Transaction>().Where(transaction => !ExpensesViewModel.Expenses.Any(expense => expense.Id == transaction.Id)).ToArray();
              foreach (var newTransaction in newTransactions)
              {
                await AddExpenseFromTransaction(newTransaction);
              }
            }
            // When 30 or more items were removed at once
            else if (TransactionsViewModel.TransactionsView.Count < ExpensesViewModel.Expenses.Count)
            {
              var oldExpenses = ExpensesViewModel.Expenses.Where(expense => !TransactionsViewModel.TransactionsView.Cast<Transaction>().Any(transaction => transaction.Id == expense.Id)).ToArray();
              foreach (var oldExpense in oldExpenses)
              {
                ExpensesViewModel.Expenses.Remove(oldExpense);
              }
            }

            // Transactions were reordered
            if (TransactionsViewModel.TransactionsView.Count == ExpensesViewModel.Expenses.Count)
            {
              for (int i = 0; i < TransactionsViewModel.TransactionsView.Count; ++i)
              {
                var oldIndex = ExpensesViewModel.Expenses.IndexOf(ExpensesViewModel.Expenses.Single(expense => expense.Id == (TransactionsViewModel.TransactionsView[i] as Transaction).Id));
                ExpensesViewModel.Expenses.Move(oldIndex, i);
              }
            }
          }
          break;
      }
    }

    private async void Expenses_CollectionChangedAsync(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Remove)
      {
        using var guard = await LockGuard.CreateAsync(_guard, TimeSpan.FromSeconds(5));
        foreach (ExpenseViewModel expense in e.OldItems.OrEmpty())
        {
          // Expense is already removed, need to remove transaction too
          if (ExpensesViewModel.Expenses.Count < TransactionsViewModel.TransactionsView.Count)
          {
            var transaction = TransactionsViewModel.TransactionsView.Cast<Transaction>().Single(transaction => transaction.Id == expense.Id);
            try
            {
              TransactionsViewModel.TransactionsView.Source.Remove(transaction);
            }
            catch (ArgumentOutOfRangeException)
            {
              // When deleting last item, there is an exception
              TransactionsViewModel.TransactionsView.Refresh();
            }
          }
        }
      }
    }
  }
}