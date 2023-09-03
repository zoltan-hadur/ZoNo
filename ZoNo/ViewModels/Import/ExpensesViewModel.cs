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
using ZoNo.ViewModels.Rules;
using ZoNo.Views.Import;

namespace ZoNo.ViewModels.Import
{
  public partial class ExpensesViewModel : ObservableObject
  {
    private readonly ISplitwiseService _splitwiseService;
    private readonly IDialogService _dialogService;
    private Splitwise.Models.Group[] _groups;
    private Splitwise.Models.Category[] _categories;
    private bool _isLoaded = false;
    private SemaphoreSlim _guard = new SemaphoreSlim(initialCount: 1, maxCount: 1);

    public ObservableCollection<ExpenseViewModel> Expenses { get; } = new ObservableCollection<ExpenseViewModel>();

    public Category[] Categories { get; private set; }
    public Group[] Groups { get; private set; }

    [ObservableProperty]
    private bool _isUploadingToSplitwise = false;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UploadExpensesToSplitwiseCommand))]
    private bool _isExpensesNotEmpty = false;

    public ExpensesViewModel(ISplitwiseService splitwiseService, IDialogService dialogService)
    {
      _splitwiseService = splitwiseService;
      _dialogService = dialogService;

      Expenses.CollectionChanged += Expenses_CollectionChanged;
    }

    public async Task Load()
    {
      using var guard = await LockGuard.CreateAsync(_guard, TimeSpan.Zero);
      if (_isLoaded) return;

      _groups = await _splitwiseService.GetGroupsAsync();
      _categories = (await _splitwiseService.GetCategoriesAsync()).OrderBy(category => category.Name).ToArray();
      Categories = _categories.Select(category => new Category()
      {
        Name = category.Name,
        SubCategories = category.Subcategories.Select(subCategory => new Category()
        {
          Name = subCategory.Name,
          Picture = subCategory.IconTypes.Square.Large
        }).ToArray()
      }).ToArray();
      Groups = _groups.Select(group => new Group()
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
        var group = _groups.Single(group => group.Name == expense.Group.Name);

        var category = _categories.Single(category => category.Name == expense.Category.ParentCategoryName)
          .Subcategories.Single(subcategory => subcategory.Name == expense.Category.Name);

        var users = expense.Shares.Select(with => new Splitwise.Models.Share()
        {
          UserId = group.Members.Single(user => user.Email == with.User.Email).Id
        }).ToArray();

        for (int i = 0; i < users.Length; ++i)
        {
          if (i == 0)
          {
            users[i].PaidShare = expense.Cost.ToString("0.00");
          }
          else
          {
            users[i].PaidShare = "0";
          }
          if (i < users.Length - 1)
          {
            users[i].OwedShare = (expense.Cost * expense.Shares[i].Percentage / 100).ToString("0.00");
          }
          else
          {
            var total = Convert.ToDouble(expense.Cost.ToString("0.00"));
            var sofar = users.SkipLast(1).Sum(user => Convert.ToDouble(user.OwedShare));
            var rest = total - sofar;
            users[i].OwedShare = rest.ToString("0.00");
          }
        }

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
