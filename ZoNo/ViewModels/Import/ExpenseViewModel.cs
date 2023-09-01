using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ZoNo.Models;

namespace ZoNo.ViewModels.Import
{
  public partial class ExpenseViewModel : ObservableObject
  {
    [ObservableProperty]
    public ObservableCollection<(User User, double Percentage)> _with;

    [ObservableProperty]
    public Category _category;

    [ObservableProperty]
    public string _description;

    [ObservableProperty]
    public Currency _currency;

    [ObservableProperty]
    public double _cost;

    [ObservableProperty]
    public DateTime _date;

    [ObservableProperty]
    public Group _group;

    public static ExpenseViewModel FromModel(Expense model)
    {
      return new ExpenseViewModel()
      {
        With = new ObservableCollection<(User User, double Percentage)>(
          model.With.Select(with => (new User()
          {
            Email = with.User
          }, with.Percentage))),
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
      return new ExpenseViewModel()
      {
        With = new ObservableCollection<(User User, double Percentage)>(
          With.Select(with => (new User()
          {
            Picture = with.User.Picture,
            FirstName = with.User.FirstName,
            LastName = with.User.LastName,
            Email = with.User.Email
          }, with.Percentage))),
        Category = new Category()
        {
          Picture = Category.Picture,
          Name = Category.Name
        },
        Description = Description,
        Currency = Currency,
        Cost = Cost,
        Date = Date,
        Group = new Group()
        {
          Picture = Group.Picture,
          Name = Group.Name,
          Members = Group.Members.Select(user => new User()
          {
            Picture = user.Picture,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email
          }).ToArray()
        }
      };
    }

    [RelayCommand]
    private void SwitchCategory(Category category)
    {
      Category = category;
    }
  }
}
