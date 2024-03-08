namespace ZoNo.Models
{
  public class Category : IComparable
  {
    public int Id { get; set; }
    public string Picture { get; set; } = "invalid";
    public string Name { get; set; }
    public Category ParentCategory { get; set; }
    private Category[] _subCategories = [];
    public Category[] SubCategories
    {
      get
      {
        return _subCategories;
      }
      set
      {
        if (value is not null)
        {
          _subCategories = value;
          foreach (var category in _subCategories)
          {
            category.ParentCategory = this;
          }
        }
      }
    }

    public int CompareTo(object obj)
    {
      if (obj is Category category)
      {
        return $"{ParentCategory?.Name} - {Name}".CompareTo($"{category.ParentCategory?.Name} - {category.Name}");
      }
      else
      {
        throw new ArgumentException($"Parameter type is not {nameof(Category)}", nameof(obj));
      }
    }
  }
}
