using Splitwise.Models;
using ZoNo.Contracts.Services;

namespace ZoNo.Services
{
  public class TokenService(ILocalSettingsService _localSettingsService) : ITokenService
  {
    private const string SettingToken = "Protected_Token";

    public Token Token { get; set; } = null;

    public async Task InitializeAsync()
    {
      Token = await _localSettingsService.ReadProtectedSettingAsync<Token>(SettingToken);
    }

    public async Task SaveAsync()
    {
      if (Token == null)
      {
        await _localSettingsService.RemoveSettingAsync(SettingToken);
      }
      else
      {
        await _localSettingsService.SaveProtectedSettingAsync(SettingToken, Token);
      }
    }
  }
}
