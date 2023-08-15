namespace ZoNo.Contracts.Services
{
  public interface ILocalSettingsService
  {
    Task<T?> ReadSettingAsync<T>(string key);
    Task<T?> ReadProtectedSettingAsync<T>(string key);
    Task SaveSettingAsync<T>(string key, T value);
    Task SaveProtectedSettingAsync<T>(string key, T value);
    Task<bool> RemoveSettingAsync(string key);
  }
}