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
  }
}
