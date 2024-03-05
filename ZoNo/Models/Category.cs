﻿namespace ZoNo.Models
{
  public class Category
  {
    public int Id { get; set; }
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
        if (value is not null)
        {
          _subCategories = value;
          foreach (var category in _subCategories)
          {
            category.ParentCategoryName = Name;
          }
        }
      }
    }
  }
}
