using Tracer.Contracts;
using Windows.Storage;
using ZoNo.Contracts.Services;
using ZoNo.Helpers;

namespace ZoNo.Services
{
  public class LocalSettingsService(
    IEncryptionService encryptionService,
    ITraceFactory traceFactory) : ILocalSettingsService
  {
    private readonly IEncryptionService _encryptionService = encryptionService;
    private readonly ITraceFactory _traceFactory = traceFactory;

    public async Task<T> ReadSettingAsync<T>(string key, bool encrypted = false)
    {
      if (ReadStringifiedSetting(key) is string setting)
      {
        return await Json.ToObjectAsync<T>(encrypted ? _encryptionService.Decrypt(setting) : setting);
      }

      return default;
    }

    public async Task SaveSettingAsync<T>(string key, T value, bool encrypt = false)
    {
      ArgumentNullException.ThrowIfNull(value);

      var setting = await Json.StringifyAsync(value);
      SaveStringifiedSetting(key, encrypt ? _encryptionService.Encrypt(setting) : setting);
    }

    public bool RemoveSetting(string key)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([key]));
      return ApplicationData.Current.LocalSettings.Values.Remove(key);
    }

    private void SaveStringifiedSetting(string key, string value)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([key, value]));
      ApplicationData.Current.LocalSettings.Values[key] = value;
    }

    private string ReadStringifiedSetting(string key)
    {
      using var trace = _traceFactory.CreateNew();
      string value = null;
      if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var obj))
      {

        value = (string)obj;
      }
      trace.Debug(Format([key, value]));
      return value;
    }
  }
}