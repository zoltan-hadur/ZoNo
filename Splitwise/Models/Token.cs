namespace Splitwise.Models
{
  public record Token
  {
    public required string AccessToken { get; init; }
    public required TokenType TokenType { get; init; }
  };
}
