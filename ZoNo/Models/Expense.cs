namespace ZoNo.Models
{
  public class Expense
  {
    public Guid Id { get; set; }
    public List<(string User, double Percentage)> With { get; set; }
    public Category Category { get; set; }
    public string Description { get; set; }
    public Currency Currency { get; set; }
    public double Cost { get; set; }
    public DateTimeOffset Date { get; set; }
    public string Group { get; set; }
  }
}
