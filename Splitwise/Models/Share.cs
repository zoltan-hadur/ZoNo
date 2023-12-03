namespace Splitwise.Models
{
  public record Share
  {
    public int UserId { get; init; }
    public string PaidShare { get; init; }
    public string OwedShare { get; init; }
  }
}
