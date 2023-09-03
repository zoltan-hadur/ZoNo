using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.Json;
using ZoNo.Helpers;
using ZoNo.Models;

namespace ZoNo.ViewModels.Import
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

    public static ExpenseViewModel FromModel(Expense model)
    {
      return new ExpenseViewModel()
      {
        Id = model.Id,
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
