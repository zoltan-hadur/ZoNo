namespace Splitwise.Models
{
  public record class Token(
    string AccessToken,
    TokenType TokenType
  );
}
