using System.Collections.ObjectModel;
using Tracer.Contracts;
using ZoNo.Contracts.Services;

namespace ZoNo.Services
{
  internal class ConverterService(
    ISplitwiseCacheService _splitwiseCacheService) : IConverterService
  {
    public ZoNo.ViewModels.ExpenseViewModel ModelExpenseToViewModel(ZoNo.Models.Expense expense)
    {
      return new ZoNo.ViewModels.ExpenseViewModel()
      {
        Id = expense.Id,
        Shares = new ObservableCollection<ZoNo.ViewModels.ShareViewModel>(
          expense.With.Select(with => new ZoNo.ViewModels.ShareViewModel
          {
            User = new ZoNo.Models.User()
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
        Group = new ZoNo.Models.Group() { Name = expense.Group }
      };
    }

    public Splitwise.Models.Expense ViewModelExpenseToSplitwise(ZoNo.ViewModels.ExpenseViewModel expense)
    {
      var group = _splitwiseCacheService.SplitwiseGroups.Single(group => group.Name == expense.Group.Name);
      var category = _splitwiseCacheService.SplitwiseCategories.Single(category => category.Name == expense.Category.ParentCategoryName)
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

      return new Splitwise.Models.Expense()
      {
        Cost = expense.Cost.ToString("0.00"),
        Description = expense.Description,
        Date = expense.Date,
        CurrencyCode = Enum.Parse<Splitwise.Models.CurrencyCode>(expense.Currency.ToString()),
        Category = category,
        GroupId = group.Id,
        Users = users
      };
    }
  }
}
