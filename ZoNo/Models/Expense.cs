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

    public override string ToString() =>
      $"{nameof(Id)}: {Id}, {nameof(With)}: [{string.Join(", ", With.Select(x => $"({x.User}, {x.Percentage})"))}], " +
      $"{nameof(Category)}: {Category}, {nameof(Description)}: {Description}, {nameof(Currency)}: {Currency}, " +
      $"{nameof(Cost)}: {Cost}, {nameof(Date)}: {Date}, {nameof(Group)}: {Group}";
  }
}
