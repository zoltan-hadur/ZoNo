using Splitwise.Models;

namespace ZoNo.Contracts.Services
{
  public interface ITokenService
  {
    Task<Token?> GetTokenAsync();
    Task SetTokenAsync(Token? token);
    Task SaveTokenAsync();
    Task<bool> DeleteSavedTokenAsync();
  }
}
