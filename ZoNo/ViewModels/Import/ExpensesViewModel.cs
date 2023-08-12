using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Splitwise.Contracts;
using Splitwise.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ZoNo.ViewModels.Import
{
  public partial class ExpensesViewModel : ObservableObject
  {
    private readonly ISplitwiseService _splitwiseService;
    private Group[] _groups;
    private Category[] _categories;
    private bool _isLoaded = false;

    public ObservableCollection<ExpenseViewModel> Expenses { get; } = new ObservableCollection<ExpenseViewModel>();

    public Category[] Categories => _categories;

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
      if (_isLoaded) return;

      _groups = await _splitwiseService.GetGroupsAsync();
      _categories = (await _splitwiseService.GetCategoriesAsync()).OrderBy(category => category.Name).ToArray();

      _isLoaded = true;
    }

    private async void Expenses_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Add && e.NewItems != null)
      {
        await Load();

        foreach (ExpenseViewModel newItem in e.NewItems)
        {
          newItem.Group = _groups.Single(group => group.Name == newItem.Group!.Name);
          for (int i = 0; i < newItem.With.Count; i++)
          {
            var user = newItem.Group.Members.Single(user => user.Email == newItem.With[i].User.Email);
            newItem.With[i] = (user, newItem.With[i].Percentage);
          }
          newItem.Category = _categories.Single(category => category.Name == newItem.Category!.Name)
            .Subcategories.Single(subcategory => subcategory.Name == newItem.Category!.Subcategories[0].Name);
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
        // TODO real upload
        await Task.Delay(100);
        Expenses.Remove(expense);
      }

      IsUploadingToSplitwise = false;
    }
  }
}
