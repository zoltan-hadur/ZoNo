using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoNo.ViewModels.Import;

namespace ZoNo.Models
{
  public enum IncomeOutcome
  {
    Income,
    Outcome
  }

  public enum Currency
  {
    HUF
  }

  public class Transaction
  {
    public DateTime TransactionTime { get; set; }
    public DateOnly AccountingDate { get; set; }
    public string Type { get; set; }
    public IncomeOutcome IncomeOutcome { get; set; }
    public string PartnerName { get; set; }
    public string PartnerAccountId { get; set; }
    public string SpendingCategory { get; set; }
    public string Description { get; set; }
    public string AccountName { get; set; }
    public string AccountId { get; set; }
    public double Amount { get; set; }
    public Currency Currency { get; set; }

    public static string GetProperty(ColumnHeader columnHeader) => columnHeader switch
    {
      ColumnHeader.TransactionTime => nameof(TransactionTime),
      ColumnHeader.AccountingDate => nameof(AccountingDate),
      ColumnHeader.Type => nameof(Type),
      ColumnHeader.IncomeOutcome => nameof(IncomeOutcome),
      ColumnHeader.PartnerName => nameof(PartnerName),
      ColumnHeader.PartnerAccountId => nameof(PartnerAccountId),
      ColumnHeader.SpendingCategory => nameof(SpendingCategory),
      ColumnHeader.Description => nameof(Description),
      ColumnHeader.AccountName => nameof(AccountName),
      ColumnHeader.AccountId => nameof(AccountId),
      ColumnHeader.Amount => nameof(Amount),
      ColumnHeader.Currency => nameof(Currency),
      _ => throw new Exception($"Could not map {columnHeader}!")
    };
  }
}
