using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Splitwise.Contracts;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using Tracer;
using Tracer.Contracts;
using ZoNo.Contracts.Services;
using ZoNo.Helpers;
using ZoNo.Models;
using ZoNo.Views;

namespace ZoNo.ViewModels
{
  public partial class ExpensesViewModel(
    ISplitwiseService _splitwiseService,
    ISplitwiseCacheService _splitwiseCacheService,
    IConverterService _converterService,
    IDialogService _dialogService,
    ITraceFactory _traceFactory) : ObservableObject
  {
    private bool _isLoaded = false;

    public ObservableCollection<ExpenseViewModel> Expenses { get; } = [];
    public IReadOnlyCollection<Category> Categories => _splitwiseCacheService.ZoNoCategories;
    public IReadOnlyCollection<Group> Groups => _splitwiseCacheService.ZoNoGroups;

    [ObservableProperty]
    private bool _isUploadingToSplitwise = false;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UploadExpensesToSplitwiseCommand))]
    private bool _isExpensesNotEmpty = false;

    public void Load()
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([_isLoaded]));
      if (_isLoaded) return;

      Expenses.CollectionChanged += Expenses_CollectionChanged;

      _isLoaded = true;
    }

    private void Expenses_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([e.Action]));
      if (e.Action == NotifyCollectionChangedAction.Add)
      {
        foreach (ExpenseViewModel newItem in e.NewItems.OrEmpty())
        {
          newItem.Category = Categories.Single(category => category.Name == newItem.Category.ParentCategory?.Name)
            .SubCategories.Single(subCategory => subCategory.Name == newItem.Category.Name);

          newItem.Group = Groups.Single(group => group.Name == newItem.Group.Name);

          for (int i = 0; i < newItem.Shares.Count; i++)
          {
            var user = newItem.Group.Name == "Non-group expenses" ?
              newItem.Shares[i].User :
              newItem.Group.Members.Single(user => user.Email == newItem.Shares[i].User.Email);
            newItem.Shares[i].User = user;
          }

          if (newItem.Group.Name == "Non-group expenses" && newItem.Shares.Count == 0)
          {
            newItem.Shares.Add(new ShareViewModel()
            {
              User = newItem.Group.Members.First(),
              Percentage = 100
            });
          }
        }
      }
      IsExpensesNotEmpty = Expenses.Count != 0;
      trace.Debug(Format([IsExpensesNotEmpty]));
    }

    [RelayCommand(CanExecute = nameof(IsExpensesNotEmpty))]
    private async Task UploadExpensesToSplitwise()
    {
      using var trace = _traceFactory.CreateNew();
      IsUploadingToSplitwise = true;

      await Task.WhenAll(Expenses
        .Select(expense => TraceFactory.HandleAsAsyncVoid(() => _splitwiseService
          .CreateExpenseAsync(_converterService.ViewModelExpenseToSplitwise(expense))
          .ContinueWith(task => Expenses.Remove(expense), TaskContinuationOptions.ExecuteSynchronously))).ToArray());

      IsUploadingToSplitwise = false;
    }

    [RelayCommand]
    private async Task EditExpense(ExpenseViewModel expense)
    {
      using var trace = _traceFactory.CreateNew();
      var index = Expenses.IndexOf(expense);
      var copiedExpense = expense.Clone();
      copiedExpense.Group = Groups.Single(group => group.Name == copiedExpense.Group.Name);
      var isPrimaryButtonEnabledBinding = new Binding()
      {
        Path = new PropertyPath(nameof(ExpenseViewModel.ArePercentagesAddUp)),
        Mode = BindingMode.OneWay,
        Source = copiedExpense
      };
      var result = await _dialogService.ShowDialogAsync(DialogType.SaveCancel, $"Edit Expense", new ExpenseEditor(copiedExpense)
      {
        Categories = Categories,
        Groups = Groups
      }, isPrimaryButtonEnabledBinding);
      trace.Debug(Format([result]));
      if (result)
      {
        Expenses[index] = copiedExpense;
      }
    }

    [RelayCommand]
    private void DeleteExpense(ExpenseViewModel expense)
    {
      using var trace = _traceFactory.CreateNew();
      var success = Expenses.Remove(expense);
      trace.Debug(Format([success]));
    }
  }
}
