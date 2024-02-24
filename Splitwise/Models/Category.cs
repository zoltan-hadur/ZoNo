namespace Splitwise.Models
{
  public record Category
  {
    public int Id { get; init; }
    public string Name { get; init; }
    public string Icon { get; init; }
    public IconTypes IconTypes { get; init; }
    public Category[] Subcategories { get; init; }
  }
}
