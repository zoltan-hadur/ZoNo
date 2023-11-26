namespace ZoNo.Models
{
  public static class Categories
  {
    private static readonly Category[] _categories =
    [
      new()
      {
        Name = "Entertainment",
        SubCategories =
        [
          new() { Name = "Games" },
          new() { Name = "Movies" },
          new() { Name = "Music" },
          new() { Name = "Other" },
          new() { Name = "Sports" }
        ]
      },
      new()
      {
        Name = "Food and drink",
        SubCategories =
        [
          new() { Name = "Dining out" },
          new() { Name = "Groceries" },
          new() { Name = "Liquor" },
          new() { Name = "Other" }
        ]
      },
      new()
      {
        Name = "Home",
        SubCategories =
        [
          new() { Name = "Electronics" },
          new() { Name = "Furniture" },
          new() { Name = "Household supplies" },
          new() { Name = "Maintenance" },
          new() { Name = "Mortgage" },
          new() { Name = "Other" },
          new() { Name = "Pets" },
          new() { Name = "Rent" },
          new() { Name = "Services" }
        ]
      },
      new()
      {
        Name = "Life",
        SubCategories =
        [
          new() { Name = "Childcare" },
          new() { Name = "Clothing" },
          new() { Name = "Education" },
          new() { Name = "Gifts" },
          new() { Name = "Insurance" },
          new() { Name = "Medical expenses" },
          new() { Name = "Other" },
          new() { Name = "Taxes" }
        ]
      },
      new()
      {
        Name = "Transportation",
        SubCategories =
        [
          new() { Name = "Bicycle" },
          new() { Name = "Bus/train" },
          new() { Name = "Car" },
          new() { Name = "Gas/fuel" },
          new() { Name = "Hotel" },
          new() { Name = "Other" },
          new() { Name = "Parking" },
          new() { Name = "Plane" },
          new() { Name = "Taxi" }
        ]
      },
      new()
      {
        Name = "Uncategorized",
        SubCategories =
        [
          new() { Name = "General" }
        ]
      },
      new()
      {
        Name = "Utilities",
        SubCategories =
        [
          new() { Name = "Cleaning" },
          new() { Name = "Electricity" },
          new() { Name = "Heat/gas" },
          new() { Name = "Other" },
          new() { Name = "Trash" },
          new() { Name = "TV/Phone/Internet" },
          new() { Name = "Water" }
        ]
      }
    ];

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
