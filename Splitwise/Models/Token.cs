namespace Splitwise.Models
{
  public class Token
  {
    public string AccessToken { get; set; }
    public TokenType TokenType { get; set; }
  };

  public enum TokenType
  {
    Bearer
  }
}
