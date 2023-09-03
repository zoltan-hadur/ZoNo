namespace ZoNo.Models
{
  public class Expense
  {
    public List<(string User, double Percentage)> With { get; set; }
    public Category Category { get; set; }
    public string Description { get; set; }
    public Currency Currency { get; set; }
    public double Cost { get; set; }
    public DateTimeOffset Date { get; set; }
    public string Group { get; set; }
  }

  public class Category
  {
    public string Picture { get; set; } = "invalid";
    public string Name { get; set; }
    public string ParentCategoryName { get; set; }
    private Category[] _subCategories = Array.Empty<Category>();
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
  }

  public static class Categories
  {
    private static Category[] _categories = new Category[]
    {
      new Category()
      {
        Name = "Entertainment",
        SubCategories = new Category[]
        {
          new Category(){ Name = "Games" },
          new Category(){ Name = "Movies" },
          new Category(){ Name = "Music" },
          new Category(){ Name = "Other" },
          new Category(){ Name = "Sports" }
        }
      },
      new Category()
      {
        Name = "Food and drink",
        SubCategories = new Category[]
        {
          new Category(){ Name = "Dining out" },
          new Category(){ Name = "Groceries" },
          new Category(){ Name = "Liquor" },
          new Category(){ Name = "Other" }
        }
      },
      new Category()
      {
        Name = "Home",
        SubCategories = new Category[]
        {
          new Category(){ Name = "Electronics" },
          new Category(){ Name = "Furniture" },
          new Category(){ Name = "Household supplies" },
          new Category(){ Name = "Maintenance" },
          new Category(){ Name = "Mortgage" },
          new Category(){ Name = "Other" },
          new Category(){ Name = "Pets" },
          new Category(){ Name = "Rent" },
          new Category(){ Name = "Services" }
        }
      },
      new Category()
      {
        Name = "Life",
        SubCategories = new Category[]
        {
          new Category(){ Name = "Childcare" },
          new Category(){ Name = "Clothing" },
          new Category(){ Name = "Education" },
          new Category(){ Name = "Gifts" },
          new Category(){ Name = "Insurance" },
          new Category(){ Name = "Medical expenses" },
          new Category(){ Name = "Other" },
          new Category(){ Name = "Taxes" }
        }
      },
      new Category()
      {
        Name = "Transportation",
        SubCategories = new Category[]
        {
          new Category(){ Name = "Bicycle" },
          new Category(){ Name = "Bus/train" },
          new Category(){ Name = "Car" },
          new Category(){ Name = "Gas/fuel" },
          new Category(){ Name = "Hotel" },
          new Category(){ Name = "Other" },
          new Category(){ Name = "Parking" },
          new Category(){ Name = "Plane" },
          new Category(){ Name = "Taxi" }
        }
      },
      new Category()
      {
        Name = "Uncategorized",
        SubCategories = new Category[]
        {
          new Category(){ Name = "General" }
        }
      },
      new Category()
      {
        Name = "Utilities",
        SubCategories = new Category[]
        {
          new Category(){ Name = "Cleaning" },
          new Category(){ Name = "Electricity" },
          new Category(){ Name = "Heat/gas" },
          new Category(){ Name = "Other" },
          new Category(){ Name = "Trash" },
          new Category(){ Name = "TV/Phone/Internet" },
          new Category(){ Name = "Water" }
        }
      }
    };

    public static class Entertainment
    {
      public static Category Games => _categories[0].SubCategories[0];
      public static Category Movies => _categories[0].SubCategories[1];
      public static Category Music => _categories[0].SubCategories[2];
      public static Category Other => _categories[0].SubCategories[3];
      public static Category Sports => _categories[0].SubCategories[4];
    }

    public class FoodAndDrink : Category
    {
      public static Category DiningOut => _categories[1].SubCategories[0];
      public static Category Groceries => _categories[1].SubCategories[1];
      public static Category Liquor => _categories[1].SubCategories[2];
      public static Category Other => _categories[1].SubCategories[3];
    }

    public class Home : Category
    {
      public static Category Electronics => _categories[2].SubCategories[0];
      public static Category Furniture => _categories[2].SubCategories[1];
      public static Category HouseholdSupplies => _categories[2].SubCategories[2];
      public static Category Maintenance => _categories[2].SubCategories[3];
      public static Category Mortgage => _categories[2].SubCategories[4];
      public static Category Other => _categories[2].SubCategories[5];
      public static Category Pets => _categories[2].SubCategories[6];
      public static Category Rent => _categories[2].SubCategories[7];
      public static Category Services => _categories[2].SubCategories[7];
    }

    public class Life : Category
    {
      public static Category Childcare => _categories[3].SubCategories[0];
      public static Category Clothing => _categories[3].SubCategories[1];
      public static Category Education => _categories[3].SubCategories[2];
      public static Category Gifts => _categories[3].SubCategories[3];
      public static Category Insurance => _categories[3].SubCategories[4];
      public static Category MedicalExpenses => _categories[3].SubCategories[5];
      public static Category Other => _categories[3].SubCategories[6];
      public static Category Taxes => _categories[3].SubCategories[7];
    }

    public class Transportation : Category
    {
      public static Category Bicycle => _categories[4].SubCategories[0];
      public static Category BusTrain => _categories[4].SubCategories[1];
      public static Category Car => _categories[4].SubCategories[2];
      public static Category GasFuels => _categories[4].SubCategories[3];
      public static Category Hotel => _categories[4].SubCategories[4];
      public static Category Other => _categories[4].SubCategories[5];
      public static Category Parking => _categories[4].SubCategories[6];
      public static Category Plane => _categories[4].SubCategories[7];
      public static Category Taxi => _categories[4].SubCategories[8];
    }

    public class Uncategorized : Category
    {
      public static Category General => _categories[5].SubCategories[0];
    }

    public class Utilities : Category
    {
      public static Category Cleaning => _categories[6].SubCategories[0];
      public static Category Electricity => _categories[6].SubCategories[1];
      public static Category HeatGas => _categories[6].SubCategories[2];
      public static Category Other => _categories[6].SubCategories[3];
      public static Category Trash => _categories[6].SubCategories[4];
      public static Category TVPhoneInternet => _categories[6].SubCategories[5];
      public static Category Water => _categories[6].SubCategories[6];
    }
  }
}
