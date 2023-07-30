using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Splitwise.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoNo.ViewModels.Import
{
  public partial class ExpenseViewModel : ObservableObject
  {
    public ObservableCollection<(User User, double Percentage)> With { get; } = new ObservableCollection<(User User, double Percentage)>();

    [ObservableProperty]
    public Category _category;

    [ObservableProperty]
    public string _description;

    [ObservableProperty]
    public CurrencyCode _currencyCode;

    [ObservableProperty]
    public double _cost;

    [ObservableProperty]
    public DateTime _date;

    [ObservableProperty]
    public Group _group;

    public ExpenseViewModel(Models.Expense expense)
    {
      foreach (var (user, percentage) in expense.With)
      {
        With.Add((new User() { Email = user }, percentage));
      }
      Category = new Category()
      {
        Name = expense.Category!.MainCategory,
        Subcategories = new Category[]
        {
          new Category()
          {
            Name = expense.Category.SubCategory
          }
        }
      };
      Description = expense.Description;
      CurrencyCode = expense.CurrencyCode;
      Cost = expense.Cost;
      Date = expense.Date;
      Group = new Group()
      {
        Name = expense.Group
      };
    }

    [RelayCommand]
    private void SwitchCategory(Category category)
    {
      Category = category;
    }
  }
}
