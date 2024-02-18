using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Specialized;
using Windows.Foundation.Collections;
using ZoNo.Contracts.Services;
using ZoNo.Helpers;
using ZoNo.Models;

namespace ZoNo.ViewModels
{
  public partial class ImportPageViewModel(
    ITransactionProcessorService transactionProcessorService,
    TransactionsViewModel transactionsViewModel,
    ExpensesViewModel expensesViewModel) : ObservableObject
  {
    private readonly ITransactionProcessorService _transactionProcessorService = transactionProcessorService;

    private bool _isLoaded = false;
    private readonly SemaphoreSlim _guard = new(initialCount: 1, maxCount: 1);

    public TransactionsViewModel TransactionsViewModel { get; } = transactionsViewModel;
    public ExpensesViewModel ExpensesViewModel { get; } = expensesViewModel;

    [ObservableProperty]
    private Transaction _selectedTransaction;

    [ObservableProperty]
    private ExpenseViewModel _selectedExpense;

    partial void OnSelectedTransactionChanged(Transaction value)
    {
      SelectedExpense = value is null ? null : ExpensesViewModel.Expenses.Single(expense => expense.Id == value.Id);
    }

    partial void OnSelectedExpenseChanged(ExpenseViewModel value)
    {
      SelectedTransaction = value is null ? null : TransactionsViewModel.TransactionsView.Cast<Transaction>().Single(transaction => transaction.Id == value.Id);
    }

    public async Task LoadAsync()
    {
      using var guard = await LockGuard.CreateAsync(_guard, TimeSpan.Zero);
      if (_isLoaded) return;

      await Task.WhenAll([TransactionsViewModel.Load(), ExpensesViewModel.LoadAsync()]);

      TransactionsViewModel.TransactionsView.VectorChanged += TransactionsView_VectorChanged;
      ExpensesViewModel.Expenses.CollectionChanged += Expenses_CollectionChangedAsync;
      _transactionProcessorService.TransactionProcessed += TransactionProcessorService_TransactionProcessed;

      _isLoaded = true;
    }

    private void TransactionProcessorService_TransactionProcessed(object sender, (Transaction Transaction, Expense Expense) e)
    {
      TransactionsViewModel.TransactionsView.Add(e.Transaction);
      var index = TransactionsViewModel.TransactionsView.IndexOf(e.Transaction);
      ExpensesViewModel.Expenses.Insert(index, ExpenseViewModel.FromModel(e.Expense));
    }

    private async void TransactionsView_VectorChanged(IObservableVector<object> sender, IVectorChangedEventArgs e)
    {
      using var guard = await LockGuard.CreateAsync(_guard, TimeSpan.FromSeconds(5));
      switch (e.CollectionChange)
      {
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
                //await AddExpenseFromTransaction(newTransaction);
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