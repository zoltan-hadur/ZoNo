namespace Splitwise.Models
{
  public record User
  {
    public int Id { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Email { get; init; }
    public Picture Picture { get; init; }
    public bool CustomPicture { get; init; }
    public CurrencyCode DefaultCurrency { get; init; }
    public int DefaultGroupId { get; init; }
  }
}
