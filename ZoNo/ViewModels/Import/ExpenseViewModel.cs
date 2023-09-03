using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Options;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZoNo.Models;

namespace ZoNo.ViewModels.Import
{
  public partial class ExpenseViewModel : ObservableObject
  {
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

    public static ExpenseViewModel FromModel(Expense model)
    {
      return new ExpenseViewModel()
      {
        Shares = new ObservableCollection<ShareViewModel>(
          model.With.Select(with => new ShareViewModel
          {
            User = new User()
            {
              Email = with.User
            },
            Percentage = with.Percentage
          })),
        Category = model.Category,
        Description = model.Description,
        Currency = model.Currency,
        Cost = model.Cost,
        Date = model.Date,
        Group = new Group() { Name = model.Group }
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
