using Splitwise.Models;
using System.Diagnostics;

namespace ZoNo.Models
{
  public class Expense
  {
    public List<(string User, double Percentage)> With { get; } = new List<(string User, double Percentage)>();
    public Category Category { get; set; } = Uncategorized.General;
    public string Description { get; set; } = string.Empty;
    public CurrencyCode CurrencyCode { get; set; } = CurrencyCode.HUF;
    public double Cost { get; set; } = 0;
    public DateTime Date { get; set; } = DateTime.MinValue;
    public string Group { get; set; } = string.Empty;
  }

  [DebuggerDisplay("{MainCategory,nq}/{SubCategory,nq}")]
  public abstract class Category
  {
    public string MainCategory { get; }
    public string SubCategory { get; }

    public Category(string mainCategory, string subCategory)
    {
      MainCategory = mainCategory;
      SubCategory = subCategory;
    }
  }

  public class Entertainment : Category
  {
    public static Category Games { get; } = new Entertainment("Games");
    public static Category Movies { get; } = new Entertainment("Movies");
    public static Category Music { get; } = new Entertainment("Music");
    public static Category Other { get; } = new Entertainment("Other");
    public static Category Sports { get; } = new Entertainment("Sports");

    private Entertainment(string subCategory) : base("Entertainment", subCategory) { }
  }

  public class FoodAndDrink : Category
  {
    public static Category DiningOut { get; } = new FoodAndDrink("Dining out");
    public static Category Groceries { get; } = new FoodAndDrink("Groceries");
    public static Category Liquor { get; } = new FoodAndDrink("Liquor");
    public static Category Other { get; } = new FoodAndDrink("Other");

    private FoodAndDrink(string subCategory) : base("Food and drink", subCategory) { }
  }

  public class Home : Category
  {
    public static Category Electronics { get; } = new Home("Electronics");
    public static Category Furniture { get; } = new Home("Furniture");
    public static Category HouseholdSupplies { get; } = new Home("Household supplies");
    public static Category Maintenance { get; } = new Home("Maintenance");
    public static Category Mortgage { get; } = new Home("Mortgage");
    public static Category Other { get; } = new Home("Other");
    public static Category Pets { get; } = new Home("Pets");
    public static Category Rent { get; } = new Home("Rent");
    public static Category Services { get; } = new Home("Services");

    private Home(string subCategory) : base("Home", subCategory) { }
  }

  public class Life : Category
  {
    public static Category Childcare { get; } = new Life("Childcare");
    public static Category Clothing { get; } = new Life("Clothing");
    public static Category Education { get; } = new Life("Education");
    public static Category Gifts { get; } = new Life("Gifts");
    public static Category Insurance { get; } = new Life("Insurance");
    public static Category MedicalExpenses { get; } = new Life("Medical expenses");
    public static Category Other { get; } = new Life("Other");
    public static Category Taxes { get; } = new Life("Taxes");

    private Life(string subCategory) : base("Life", subCategory) { }
  }

  public class Transportation : Category
  {
    public static Category Bicycle { get; } = new Transportation("Bicycle");
    public static Category BusTrain { get; } = new Transportation("Bus/train");
    public static Category Car { get; } = new Transportation("Car");
    public static Category GasFuels { get; } = new Transportation("Gas/fuel");
    public static Category Hotel { get; } = new Transportation("Hotel");
    public static Category Other { get; } = new Transportation("Other");
    public static Category Parking { get; } = new Transportation("Parking");
    public static Category Plane { get; } = new Transportation("Plane");
    public static Category Taxi { get; } = new Transportation("Taxi");

    private Transportation(string subCategory) : base("Transportation", subCategory) { }
  }

  public class Uncategorized : Category
  {
    public static Category General { get; } = new Uncategorized("General");

    private Uncategorized(string subCategory) : base("Uncategorized", subCategory) { }
  }

  public class Utilities : Category
  {
    public static Category Cleaning { get; } = new Utilities("Cleaning");
    public static Category Electricity { get; } = new Utilities("Electricity");
    public static Category HeatGas { get; } = new Utilities("Heat/gas");
    public static Category Other { get; } = new Utilities("Other");
    public static Category Trash { get; } = new Utilities("Trash");
    public static Category TVPhoneInternet { get; } = new Utilities("TV/Phone/Internet");
    public static Category Water { get; } = new Utilities("Water");

    private Utilities(string subCategory) : base("Utilities", subCategory) { }
  }
}
