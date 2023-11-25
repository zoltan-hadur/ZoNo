namespace Splitwise.Models
{
  public class Category
  {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }
    public IconTypes IconTypes { get; set; }
    public Category[] Subcategories { get; set; }
  }
}
