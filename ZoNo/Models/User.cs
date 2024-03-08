namespace ZoNo.Models
{
  public class User
  {
    public int Id { get; set; }
    public string Picture { get; set; } = "invalid";
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public Currency DefaultCurrency { get; set; }
  }
}
