using System.Collections.ObjectModel;
using Tracer.Contracts;
using ZoNo.Contracts.Services;

namespace ZoNo.Services
{
  internal class ConverterService(
    ISplitwiseCacheService _splitwiseCacheService,
    ITraceFactory _traceFactory) : IConverterService
  {
    public ZoNo.ViewModels.ExpenseViewModel ModelExpenseToViewModel(ZoNo.Models.Expense expense)
    {
      using var trace = _traceFactory.CreateNew();
      var group = _splitwiseCacheService.ZoNoGroups.Single(group => group.Name == expense.Group);
      return new ZoNo.ViewModels.ExpenseViewModel()
      {
        Id = expense.Id,
        Shares = new ObservableCollection<ZoNo.ViewModels.ShareViewModel>(
          expense.With.Select(with => new ZoNo.ViewModels.ShareViewModel
          {
            User = group.Members.Single(member => member.Email == with.User),
            Percentage = with.Percentage
          })),
        Category = _splitwiseCacheService.ZoNoCategories
          .SelectMany(category => category.SubCategories)
          .Single(category => category.ParentCategory?.Name == expense.Category.ParentCategory?.Name &&
                              category.Name == expense.Category.Name),
        Description = expense.Description,
        Currency = expense.Currency,
        Cost = expense.Cost,
        Date = expense.Date,
        Group = group
      };
    }

    public Splitwise.Models.Expense ViewModelExpenseToSplitwise(ZoNo.ViewModels.ExpenseViewModel expense)
    {
      using var trace = _traceFactory.CreateNew();
      var group = _splitwiseCacheService.SplitwiseGroups.Single(group => group.Id == expense.Group.Id);
      var category = _splitwiseCacheService.SplitwiseCategories
        .SelectMany(category => category.Subcategories)
        .Single(category => category.Id == expense.Category.Id);

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

    public ZoNo.ViewModels.ExpenseViewModel SplitwiseExpenseToViewModel(Splitwise.Models.Expense expense)
    {
      using var trace = _traceFactory.CreateNew();
      var cost = Convert.ToDouble(expense.Cost);
      var group = _splitwiseCacheService.ZoNoGroups.Single(group => group.Id == expense.GroupId);

      var result = new ZoNo.ViewModels.ExpenseViewModel()
      {
        Category = _splitwiseCacheService.ZoNoCategories
          .SelectMany(category => category.SubCategories)
          .Single(category => category.Id == expense.Category.Id),
        Description = expense.Description,
        Currency = Enum.Parse<ZoNo.Models.Currency>(expense.CurrencyCode.ToString()),
        Cost = cost,
        Date = expense.Date,
        Group = group,
        Shares = new(expense.Users.OrderByDescending(user => Convert.ToDouble(user.PaidShare))
          .Select(user => new ZoNo.ViewModels.ShareViewModel()
          {
            User = group.Members.Single(member => member.Id == user.UserId),
            Percentage = Convert.ToDouble(user.OwedShare) / cost * 100
          }))
      };
      return result;
    }
  }
}
