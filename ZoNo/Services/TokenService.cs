using Splitwise.Models;
using ZoNo.Contracts.Services;

namespace ZoNo.Services
{
  public class TokenService(
    ILocalSettingsService localSettingsService) : ITokenService
  {
    private readonly ILocalSettingsService _localSettingsService = localSettingsService;

    private const string SettingToken = "Protected_Token";

    public Token Token { get; set; } = null;

    public async Task InitializeAsync()
    {
      Token = await _localSettingsService.ReadSettingAsync<Token>(SettingToken, encrypted: true);
    }

    public async Task SaveAsync()
    {
      if (Token == null)
      {
        _localSettingsService.RemoveSetting(SettingToken);
      }
      else
      {
        await _localSettingsService.SaveSettingAsync(SettingToken, Token, encrypt: true);
      }
    }
  }
}
