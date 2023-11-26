using Splitwise.Models;
using ZoNo.Contracts.Services;

namespace ZoNo.Services
{
  public class TokenService(ILocalSettingsService _localSettingsService) : ITokenService
  {
    private const string SettingToken = "Protected_Token";
    private Token _token = null;

    public async Task<Token> GetTokenAsync()
    {
      return _token ??= await _localSettingsService.ReadProtectedSettingAsync<Token>(SettingToken);
    }

    public async Task SetTokenAsync(Token token)
    {
      _token = token;
      await Task.CompletedTask;
    }

    public async Task SaveTokenAsync()
    {
      if (_token == null)
      {
        await _localSettingsService.RemoveSettingAsync(SettingToken);
      }
      else
      {
        await _localSettingsService.SaveProtectedSettingAsync(SettingToken, _token);
      }
    }

    public async Task<bool> DeleteSavedTokenAsync()
    {
      _token = null;
      return await _localSettingsService.RemoveSettingAsync(SettingToken);
    }
  }
}
