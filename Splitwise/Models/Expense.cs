using System;

namespace Splitwise.Models
{
  public class Expense
  {
    public string Cost { get; set; }
    public string Description { get; set; }
    public DateTimeOffset Date { get; set; }
    public CurrencyCode CurrencyCode { get; set; }
    public int CategoryId { get; set; }
    public int? GroupId { get; set; }
    public Share[] Users { get; set; }
  }
}
