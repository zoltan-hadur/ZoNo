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

    public static Expense ToModel(ExpenseViewModel vm)
    {
      var users = vm.With.Select(x => new Share()
      {
        UserId = x.User.Id
      }).ToArray();

      for (int i = 0; i < users.Length; ++i)
      {
        if (i == 0)
        {
          users[i].PaidShare = vm.Cost.ToString("0.00");
        }
        else
        {
          users[i].PaidShare = "0";
        }
        if (i < users.Length - 1)
        {
          users[i].OwedShare = (vm.Cost * vm.With[i].Percentage / 100).ToString("0.00");
        }
        else
        {
          var total = Convert.ToDouble(vm.Cost.ToString("0.00"));
          var sofar = users.SkipLast(1).Sum(user => Convert.ToDouble(user.OwedShare));
          var rest = total - sofar;
          users[i].OwedShare = rest.ToString("0.00");
        }
      }

      return new Expense()
      {
        Cost = vm.Cost.ToString("0.00"),
        Description = vm.Description,
        Date = vm.Date,
        CurrencyCode = vm.CurrencyCode,
        CategoryId = vm.Category.Id,
        GroupId = vm.Group.Id,
        Users = users
      };
    }

    [RelayCommand]
    private void SwitchCategory(Category category)
    {
      Category = category;
    }
  }
}
