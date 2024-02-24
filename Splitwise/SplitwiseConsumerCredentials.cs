using Splitwise.Contracts;

namespace Splitwise
{
  public record SplitwiseConsumerCredentials() : ISplitwiseConsumerCredentials
  {
    public required string ConsumerKey { get; init; }
    public required string ConsumerSecret { get; init; }
  }
}
