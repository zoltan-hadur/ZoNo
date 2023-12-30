namespace ZoNo.Contracts.Services
{
  public interface ILocalSettingsService
  {
    Task<T> ReadSettingAsync<T>(string key, bool encrypted = false);
    Task SaveSettingAsync<T>(string key, T value, bool encrypt = false);
    bool RemoveSetting(string key);
  }
}