namespace Splitwise.Models
{
  public record class User(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    Picture Picture,
    bool CustomPicture,
    CurrencyCode DefaultCurrency,
    int DefaultGroupId
  );
}
