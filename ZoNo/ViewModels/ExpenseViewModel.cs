using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.Json;
using ZoNo.Helpers;
using ZoNo.Models;

namespace ZoNo.ViewModels
{
  public partial class ExpenseViewModel : ObservableObject
  {
    public Guid Id { get; set; }

    [ObservableProperty]
    private ObservableCollection<ShareViewModel> _shares;

    [ObservableProperty]
    private Category _category;

    [ObservableProperty]
    private string _description;

    [ObservableProperty]
    private Currency _currency;

    [ObservableProperty]
    private double _cost;

    [ObservableProperty]
    private DateTimeOffset _date;

    [ObservableProperty]
    private Group _group;

    public bool ArePercentagesAddUp => Shares?.Sum(share => share.Percentage) == 100;

    partial void OnSharesChanged(ObservableCollection<ShareViewModel> oldValue, ObservableCollection<ShareViewModel> newValue)
    {
      if (oldValue != null)
      {
        oldValue.CollectionChanged -= Shares_CollectionChanged;
        foreach (var share in Shares)
        {
          share.PropertyChanged -= Share_PropertyChanged;
        }
      }
      if (newValue != null)
      {
        newValue.CollectionChanged += Shares_CollectionChanged;
        foreach (var share in Shares)
        {
          share.PropertyChanged += Share_PropertyChanged;
        }
      }
      OnPropertyChanged(nameof(ArePercentagesAddUp));
    }

    private void Shares_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
        case NotifyCollectionChangedAction.Remove:
        case NotifyCollectionChangedAction.Replace:
          foreach (ShareViewModel share in e.OldItems.OrEmpty())
          {
            share.PropertyChanged -= Share_PropertyChanged;
          }
          foreach (ShareViewModel share in e.NewItems.OrEmpty())
          {
            share.PropertyChanged += Share_PropertyChanged;
          }
          OnPropertyChanged(nameof(ArePercentagesAddUp));
          break;
      }
    }

    private void Share_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(ShareViewModel.Percentage))
      {
        OnPropertyChanged(nameof(ArePercentagesAddUp));
      }
    }

    public static ExpenseViewModel FromModel(Expense expense)
    {
      return new ExpenseViewModel()
      {
        Id = expense.Id,
        Shares = new ObservableCollection<ShareViewModel>(
          expense.With.Select(with => new ShareViewModel
          {
            User = new User()
            {
              Email = with.User
            },
            Percentage = with.Percentage
          })),
        Category = expense.Category,
        Description = expense.Description,
        Currency = expense.Currency,
        Cost = expense.Cost,
        Date = expense.Date,
        Group = new Group() { Name = expense.Group }
      };
    }

    public static Splitwise.Models.Expense ToSplitwiseModel(ExpenseViewModel vm, Splitwise.Models.Group[] groups, Splitwise.Models.Category[] categories)
    {
      var group = groups.Single(group => group.Name == vm.Group.Name);
      var category = categories.Single(category => category.Name == vm.Category.ParentCategoryName)
        .Subcategories.Single(subcategory => subcategory.Name == vm.Category.Name);

      var sofar = 0.0;
      var users = vm.Shares.Select((with, index) =>
      {
        var paidShare = string.Empty;
        var owedShare = string.Empty;
        if (index == 0)
        {
          paidShare = vm.Cost.ToString("0.00");
        }
        else
        {
          paidShare = "0";
        }
        if (index < vm.Shares.Count - 1)
        {
          owedShare = (vm.Cost * vm.Shares[index].Percentage / 100).ToString("0.00");
          sofar = sofar + Convert.ToDouble(owedShare);
        }
        else
        {
          var total = Convert.ToDouble(vm.Cost.ToString("0.00"));
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

      return new Splitwise.Models.Expense()
      {
        Cost = vm.Cost.ToString("0.00"),
        Description = vm.Description,
        Date = vm.Date,
        CurrencyCode = Enum.Parse<Splitwise.Models.CurrencyCode>(vm.Currency.ToString()),
        CategoryId = category.Id,
        GroupId = group.Id,
        Users = users
      };
    }

    public ExpenseViewModel Clone()
    {
      return JsonSerializer.Deserialize<ExpenseViewModel>(JsonSerializer.Serialize(this));
    }

    [RelayCommand]
    private void SwitchCategory(Category category)
    {
      Category = category;
    }

    [RelayCommand]
    private void AddUser(User user)
    {
      Shares.Add(new ShareViewModel()
      {
        User = new User()
        {
          Picture = user.Picture,
          FirstName = user.FirstName,
          LastName = user.LastName,
          Email = user.Email
        },
        Percentage = 0
      });
    }

    [RelayCommand]
    private void DeleteShare(ShareViewModel share)
    {
      Shares.Remove(share);
      if (Shares.Count == 1)
      {
        Shares[0].Percentage = 100;
      }
    }
  }
}
