﻿using ZoNo.ViewModels;

namespace ZoNo.Models
{
  public class Transaction
  {
    public Guid Id { get; set; }
    public DateTimeOffset TransactionTime { get; set; } = DateTimeOffset.MinValue;
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

    public override string ToString() =>
      $"{nameof(Id)}: {Id}, {nameof(TransactionTime)}: {TransactionTime}, {nameof(AccountingDate)}: {AccountingDate}, " +
      $"{nameof(Type)}: {Type}, {nameof(IncomeOutcome)}: {IncomeOutcome}, {nameof(PartnerName)}: {PartnerName}, " +
      $"{nameof(PartnerAccountId)}: {PartnerAccountId}, {nameof(SpendingCategory)}: {SpendingCategory}, " +
      $"{nameof(Description)}: {Description}, {nameof(AccountName)}: {AccountName}, {nameof(AccountId)}: {AccountId}, " +
      $"{nameof(Amount)}: {Amount}, {nameof(Currency)}: {Currency}";

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
