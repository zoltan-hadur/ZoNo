using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

  public record Transaction(
    DateTime TransactionTime,
    DateOnly AccountingDate,
    string Type,
    IncomeOutcome IncomeOutcome,
    string PartnerName,
    string PartnerAccountId,
    string SpendingCategory,
    string Description,
    string AccountName,
    string AccountId,
    double Amount,
    Currency Currency
  );
}
