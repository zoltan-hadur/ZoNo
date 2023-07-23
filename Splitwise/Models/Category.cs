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

  public class IconTypes
  {
    public Slim Slim { get; set; }
    public Square Square { get; set; }
  }

  public class Slim
  {
    public string Small { get; set; }
    public string Large { get; set; }
  }

  public class Square
  {
    public string Large { get; set; }
    public string XLarge { get; set; }
  }
}
