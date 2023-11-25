namespace Splitwise.Models
{
  public class User
  {
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public Picture Picture { get; set; }
    public bool CustomPicture { get; set; }
    public CurrencyCode DefaultCurrency { get; set; }
    public int DefaultGroupId { get; set; }
  }
}
