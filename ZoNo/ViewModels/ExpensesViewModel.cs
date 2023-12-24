using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Splitwise.Contracts;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using ZoNo.Contracts.Services;
using ZoNo.Helpers;
using ZoNo.Models;
using ZoNo.Views;

namespace ZoNo.ViewModels
{
  public partial class ExpensesViewModel(
    ISplitwiseService _splitwiseService,
    IDialogService _dialogService) : ObservableObject
  {
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
      using var guard = await LockGuard.CreateAsync(_guard, TimeSpan.Zero);
      if (_isLoaded) return;

      var taskGroups = _splitwiseService.GetGroupsAsync();
      var taskCategories = _splitwiseService.GetCategoriesAsync();
      await Task.WhenAll([taskGroups, taskCategories]);

      _splitwiseGroups = taskGroups.Result;
      _splitwiseCategories = taskCategories.Result.OrderBy(category => category.Name).ToArray();

      Categories = _splitwiseCategories.Select(category => new Category()
      {
        Name = category.Name,
        SubCategories = category.Subcategories.Select(subCategory => new Category()
        {
          Name = subCategory.Name,
          Picture = subCategory.IconTypes.Square.Large
        }).ToArray()
      }).ToArray();
      Groups = _splitwiseGroups.Select(group => new Group()
      {
        Picture = group.Avatar.Medium,
        Name = group.Name,
        Members = group.Members.Select(user => new User()
        {
          Picture = user.Picture.Medium,
          FirstName = user.FirstName,
          LastName = user.LastName,
          Email = user.Email
        }).ToArray()
      }).ToArray();

      Expenses.CollectionChanged += Expenses_CollectionChanged;

      _isLoaded = true;
    }

    private async void Expenses_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
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
    }

    [RelayCommand(CanExecute = nameof(IsExpensesNotEmpty))]
    private async Task UploadExpensesToSplitwise()
    {
      IsUploadingToSplitwise = true;

      foreach (var expense in Expenses.ToArray())
      {
        var group = _splitwiseGroups.Single(group => group.Name == expense.Group.Name);

        var category = _splitwiseCategories.Single(category => category.Name == expense.Category.ParentCategoryName)
          .Subcategories.Single(subcategory => subcategory.Name == expense.Category.Name);

        var sofar = 0.0;
        var users = expense.Shares.Select((with, index) =>
        {
          var paidShare = string.Empty;
          var owedShare = string.Empty;
          if (index == 0)
          {
            paidShare = expense.Cost.ToString("0.00");
          }
          else
          {
            paidShare = "0";
          }
          if (index < expense.Shares.Count - 1)
          {
            owedShare = (expense.Cost * expense.Shares[index].Percentage / 100).ToString("0.00");
            sofar = sofar + Convert.ToDouble(owedShare);
          }
          else
          {
            var total = Convert.ToDouble(expense.Cost.ToString("0.00"));
            var rest = total - sofar;
            owedShare = rest.ToString("0.00");
          }
          return new Splitwise.Models.Share()
          {
            UserId = group.Members.Single(user => user.Email == with.User.Email).Id,
            PaidShare = paidShare,
            OwedShare = owedShare
          };
        }).ToArray();

        var splitwiseExpense = new Splitwise.Models.Expense()
        {
          Cost = expense.Cost.ToString("0.00"),
          Description = expense.Description,
          Date = expense.Date,
          CurrencyCode = Enum.Parse<Splitwise.Models.CurrencyCode>(expense.Currency.ToString()),
          CategoryId = category.Id,
          GroupId = group.Id,
          Users = users
        };

        await _splitwiseService.CreateExpense(splitwiseExpense);
        Expenses.Remove(expense);
      }

      IsUploadingToSplitwise = false;
    }

    [RelayCommand]
    private async Task EditExpense(ExpenseViewModel expense)
    {
      var index = Expenses.IndexOf(expense);
      var copiedExpense = expense.Clone();
      copiedExpense.Group = Groups.Single(group => group.Name == copiedExpense.Group.Name);
      var isPrimaryButtonEnabledBinding = new Binding()
      {
        Path = new PropertyPath(nameof(ExpenseViewModel.ArePercentagesAddUp)),
        Mode = BindingMode.OneWay,
        Source = copiedExpense
      };
      var result = await _dialogService.ShowDialogAsync(DialogType.OkCancel, $"Edit Expense", new ExpenseEditor(copiedExpense)
      {
        Categories = Categories,
        Groups = Groups
      }, isPrimaryButtonEnabledBinding);
      if (result == DialogResult.Ok)
      {
        Expenses[index] = copiedExpense;
      }
    }

    [RelayCommand]
    private void DeleteExpense(ExpenseViewModel expense)
    {
      Expenses.Remove(expense);
    }
  }
}
