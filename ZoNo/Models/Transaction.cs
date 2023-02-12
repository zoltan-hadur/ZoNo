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
    public required DateTime TransactionTime { get; set; }
    public required DateOnly AccountingDate { get; set; }
    public required string Type { get; set; }
    public required IncomeOutcome IncomeOutcome { get; set; }
    public required string PartnerName { get; set; }
    public required string PartnerAccountId { get; set; }
    public required string SpendingCategory { get; set; }
    public required string Description { get; set; }
    public required string AccountName { get; set; }
    public required string AccountId { get; set; }
    public required double Amount { get; set; }
    public required Currency Currency { get; set; }

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
