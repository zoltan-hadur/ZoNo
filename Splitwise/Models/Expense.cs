using System;

namespace Splitwise.Models
{
  public class Expense
  {
    public string Cost { get; set; }
    public string Description { get; set; }
    public DateTime Date { get; set; }
    public CurrencyCode CurrencyCode { get; set; }
    public int CategoryId { get; set; }
    public int? GroupId { get; set; }
    public Share[] Users { get; set; }
  }

  public class Share
  {
    public int UserId { get; set; }
    public string PaidShare { get; set; }
    public string OwedShare { get; set; }
  }
}
