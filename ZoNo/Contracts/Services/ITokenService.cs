using Splitwise.Models;

namespace ZoNo.Contracts.Services
{
  public interface ITokenService
  {
    Token Token { get; set; }
    Task InitializeAsync();
    Task SaveAsync();
  }
}
