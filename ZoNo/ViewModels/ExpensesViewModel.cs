﻿using CommunityToolkit.Mvvm.ComponentModel;
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
    ISplitwiseService splitwiseService,
    IDialogService dialogService,
    ITraceFactory traceFactory) : ObservableObject
  {
    private readonly ISplitwiseService _splitwiseService = splitwiseService;
    private readonly IDialogService _dialogService = dialogService;
    private readonly ITraceFactory _traceFactory = traceFactory;

    private bool _isLoaded = false;
    private readonly SemaphoreSlim _guard = new(initialCount: 1, maxCount: 1);

    private Splitwise.Models.Group[] _splitwiseGroups;
    private Splitwise.Models.Category[] _splitwiseCategories;

    public ObservableCollection<ExpenseViewModel> Expenses { get; } = [];

    [ObservableProperty]
    private Category[] _categories;

    [ObservableProperty]
    private Group[] _groups;

    [ObservableProperty]
    private bool _isUploadingToSplitwise = false;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UploadExpensesToSplitwiseCommand))]
    private bool _isExpensesNotEmpty = false;

    public async Task LoadAsync()
    {
      using var trace = _traceFactory.CreateNew();
      using var guard = await LockGuard.CreateAsync(_guard, TimeSpan.Zero);
      trace.Debug(Format([_isLoaded]));
      if (_isLoaded) return;

      var taskGroups = TraceFactory.HandleAsAsyncVoid(_splitwiseService.GetGroupsAsync);
      var taskCategories = TraceFactory.HandleAsAsyncVoid(_splitwiseService.GetCategoriesAsync);
      await Task.WhenAll([taskGroups, taskCategories]);

      _splitwiseGroups = taskGroups.Result;
      _splitwiseCategories = taskCategories.Result.OrderBy(category => category.Name).ToArray();

      Categories = _splitwiseCategories.Select(Category.FromSplitwiseModel).ToArray();
      Groups = _splitwiseGroups.Select(Group.FromSplitwiseModel).ToArray();

      Expenses.CollectionChanged += Expenses_CollectionChanged;

      _isLoaded = true;
    }

    private async void Expenses_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([e.Action]));
      if (e.Action == NotifyCollectionChangedAction.Add)
      {
        using var guard = await LockGuard.CreateAsync(_guard, TimeSpan.FromSeconds(5));
        foreach (ExpenseViewModel newItem in e.NewItems.OrEmpty())
        {
          newItem.Category = Categories.Single(category => category.Name == newItem.Category.ParentCategoryName)
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
          .CreateExpenseAsync(ExpenseViewModel.ToSplitwiseModel(expense, _splitwiseGroups, _splitwiseCategories))
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
