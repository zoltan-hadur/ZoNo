namespace Splitwise.Models
{
  public record Token
  {
    public string AccessToken { get; init; }
    public TokenType TokenType { get; init; }
  };
}
