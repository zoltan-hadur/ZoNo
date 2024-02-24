using Splitwise.Models;
using Tracer.Contracts;
using ZoNo.Contracts.Services;

namespace ZoNo.Services
{
  public class TokenService(
    ILocalSettingsService localSettingsService,
    ITraceFactory traceFactory) : ITokenService
  {
    private readonly ILocalSettingsService _localSettingsService = localSettingsService;
    private readonly ITraceFactory _traceFactory = traceFactory;

    private const string SettingToken = "Protected_Token";

    public Token Token { get; set; } = null;

    public async Task InitializeAsync()
    {
      using var trace = _traceFactory.CreateNew();
      Token = await _localSettingsService.ReadSettingAsync<Token>(SettingToken, encrypted: true);
    }

    public async Task SaveAsync()
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([Token is null]));
      if (Token is null)
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
