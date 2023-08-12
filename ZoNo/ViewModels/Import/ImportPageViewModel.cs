using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using Windows.Foundation.Collections;
using ZoNo.Contracts.Services;
using ZoNo.Helpers;
using ZoNo.Models;

namespace ZoNo.ViewModels.Import
{
  public partial class ImportPageViewModel : ObservableObject
  {
    private readonly IRulesService _rulesService;
    private readonly IRuleEvaluatorServiceBuilder _ruleEvaluatorServiceBuilder;
    private IRuleEvaluatorService<Transaction, Expense>? _ruleEvaluatorService;
    private Dictionary<Transaction, ExpenseViewModel> _transactionToExpenseViewModel = new Dictionary<Transaction, ExpenseViewModel>();
    private BlockingCollection<(int Index, Transaction? Transaction)> _newTransactions = new BlockingCollection<(int Index, Transaction? Transaction)>();
    private BlockingCollection<Transaction> _transactionsToRemove = new BlockingCollection<Transaction>();
    private SemaphoreSlim _guard = new SemaphoreSlim(initialCount: 1, maxCount: 1);

    public TransactionsViewModel TransactionsViewModel { get; }
    public ExpensesViewModel ExpensesViewModel { get; }

    [ObservableProperty]
    private Transaction? _selectedTransaction;

    partial void OnSelectedTransactionChanged(Transaction? value)
    {
      SelectedExpense = value == null ? null : _transactionToExpenseViewModel[value];
    }

    [ObservableProperty]
    private ExpenseViewModel? _selectedExpense;

    partial void OnSelectedExpenseChanged(ExpenseViewModel? value)
    {
      SelectedTransaction = value == null ? null : _transactionToExpenseViewModel.Single(x => x.Value == value).Key;
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
          // To track order of insertions
          _newTransactions.Add(((int)e.Index, TransactionsViewModel.TransactionsView[(int)e.Index] as Transaction));
        }
      };
      TransactionsViewModel.TransactionsView.VectorChanged += TransactionsView_VectorChanged;
      ExpensesViewModel.Expenses.CollectionChanged += Expenses_CollectionChangedAsync;
    }

    private async void TransactionsViewModel_LoadExcelDocumentsStarted(object? sender, EventArgs e)
    {
      using var guard = await LockGuard.CreateAsync(_guard, TimeSpan.Zero);
      var rules = await _rulesService.GetRulesAsync(RuleType.Splitwise);
      _ruleEvaluatorService = await _ruleEvaluatorServiceBuilder.BuildAsync<Transaction, Expense>(rules);
    }

    private async void TransactionsViewModel_LoadExcelDocumentsFinished(object? sender, EventArgs e)
    {
      using var guard = await LockGuard.CreateAsync(_guard, TimeSpan.Zero);
      while (_transactionsToRemove.TryTake(out var transaction))
      {
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

    private async void TransactionsView_VectorChanged(IObservableVector<object> sender, IVectorChangedEventArgs e)
    {
      using var guard = await LockGuard.CreateAsync(_guard, TimeSpan.FromSeconds(5));
      switch (e.CollectionChange)
      {
        case CollectionChange.ItemInserted:
          {
            var (index, newTransaction) = _newTransactions.Take();
            if (newTransaction == null)
            {
              throw new Exception($"Transaction for index \"{index}\" is null!");
            }
            var evaluatedExpense = new Expense();
            var result = await _ruleEvaluatorService!.EvaluateRulesAsync(input: newTransaction, output: evaluatedExpense);
            var newExpense = new ExpenseViewModel(evaluatedExpense);
            _transactionToExpenseViewModel[newTransaction] = newExpense;
            ExpensesViewModel.Expenses.Insert(index, newExpense);
            if (result.RemoveThisElementFromList)
            {
              _transactionsToRemove.Add(newTransaction);
            }
          }
          break;
        case CollectionChange.ItemRemoved:
          {
            // Transaction is already removed, need to remove expense too
            if (TransactionsViewModel.TransactionsView.Count < ExpensesViewModel.Expenses.Count)
            {
              var oldIndex = (int)e.Index;
              var expense = ExpensesViewModel.Expenses[oldIndex];
              var transaction = _transactionToExpenseViewModel.Single(x => x.Value == expense).Key;
              _transactionToExpenseViewModel.Remove(transaction);
              ExpensesViewModel.Expenses.Remove(expense);
            }
          }
          break;
        case CollectionChange.Reset:
          {
            if (TransactionsViewModel.TransactionsView.Count > 0)
            {
              // If transactions were reordered
              if (TransactionsViewModel.TransactionsView.Count == ExpensesViewModel.Expenses.Count)
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
          }
          break;
      }
    }

    private async void Expenses_CollectionChangedAsync(object? sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Remove)
      {
        using var guard = await LockGuard.CreateAsync(_guard, TimeSpan.FromSeconds(5));
        foreach (ExpenseViewModel expense in e.OldItems.OrEmpty())
        {
          // Expense is already removed, need to remove transaction too
          if (ExpensesViewModel.Expenses.Count < TransactionsViewModel.TransactionsView.Count)
          {
            var transaction = _transactionToExpenseViewModel.Single(x => x.Value == expense).Key;
            _transactionToExpenseViewModel.Remove(transaction);
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