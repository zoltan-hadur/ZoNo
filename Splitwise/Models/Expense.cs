using System;

namespace Splitwise.Models
{
  public record Expense
  {
    public Int64 Id { get; init; }
    public string Cost { get; init; }
    public string Description { get; init; }
    public DateTimeOffset Date { get; init; }
    public CurrencyCode CurrencyCode { get; init; }
    public int CategoryId { get; init; }
    public int? GroupId { get; init; }
    public Share[] Users { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
  }
}
