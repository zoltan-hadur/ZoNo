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
    HUF,
    EUR,
    CAD
  }

  public class Transaction
  {
    public DateTime TransactionTime { get; set; } = DateTime.MinValue;
    public DateOnly? AccountingDate { get; set; } = null;
    public string Type { get; set; } = string.Empty;
    public IncomeOutcome IncomeOutcome { get; set; } = IncomeOutcome.Outcome;
    public string PartnerName { get; set; } = string.Empty;
    public string PartnerAccountId { get; set; } = string.Empty;
    public string SpendingCategory { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public double Amount { get; set; } = 0;
    public Currency Currency { get; set; } = Currency.HUF;

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
