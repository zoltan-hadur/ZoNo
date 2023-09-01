using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Splitwise.Contracts;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using ZoNo.Contracts.Services;
using ZoNo.Helpers;
using ZoNo.Models;
using ZoNo.Views.Import;

namespace ZoNo.ViewModels.Import
{
  public partial class ExpensesViewModel : ObservableObject
  {
    private readonly ISplitwiseService _splitwiseService;
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

    public ExpensesViewModel(ISplitwiseService splitwiseService)
    {
      _splitwiseService = splitwiseService;

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
          newItem.Category = Categories.Single(category => category.Name == newItem.Category.ParentCategory.Name)
            .SubCategories.Single(subCategory => subCategory.Name == newItem.Category.Name);

          newItem.Group = Groups.Single(group => group.Name == newItem.Group.Name);

          for (int i = 0; i < newItem.With.Count; i++)
          {
            var user = newItem.Group.Name == "Non-group expenses" ?
              newItem.With[i].User :
              newItem.Group.Members.Single(user => user.Email == newItem.With[i].User.Email);
            newItem.With[i] = (user, newItem.With[i].Percentage);
          }

          if (newItem.Group.Name == "Non-group expenses" && newItem.With.Count == 0)
          {
            newItem.With.Add((newItem.Group.Members.First(), 100));
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

        var category = _categories.Single(category => category.Name == expense.Category.ParentCategory.Name)
          .Subcategories.Single(subcategory => subcategory.Name == expense.Category.Name);

        var users = expense.With.Select(with => new Splitwise.Models.Share()
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
            users[i].OwedShare = (expense.Cost * expense.With[i].Percentage / 100).ToString("0.00");
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
  }
}
