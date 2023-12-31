using ZoNo.Helpers;

namespace ZoNo.Models
{
  public class Category
  {
    public string Picture { get; set; } = "invalid";
    public string Name { get; set; }
    public string ParentCategoryName { get; set; }
    private Category[] _subCategories = [];
    public Category[] SubCategories
    {
      get
      {
        return _subCategories;
      }
      set
      {
        if (value != null)
        {
          _subCategories = value;
          foreach (var category in _subCategories)
          {
            category.ParentCategoryName = Name;
          }
        }
      }
    }

    public static Category FromSplitwiseModel(Splitwise.Models.Category category)
    {
      return new Category()
      {
        Name = category.Name,
        Picture = category.IconTypes.Square.Large,
        SubCategories = category.Subcategories.OrEmpty().Select(FromSplitwiseModel).ToArray()
      };
    }
  }
}
