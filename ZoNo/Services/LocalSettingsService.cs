using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using Windows.Storage;
using ZoNo.Contracts.Services;
using ZoNo.Helpers;
using ZoNo.Models;

namespace ZoNo.Services
{
  public class LocalSettingsService : ILocalSettingsService
  {
    private const string _entropy = "Nora is the love of my life";
    private const string _defaultApplicationDataFolder = "ZoNo/ApplicationData";
    private const string _defaultLocalSettingsFile = "LocalSettings.json";

    private readonly IFileService _fileService;
    private readonly LocalSettingsOptions _options;

    private readonly string _localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    private readonly string _applicationDataFolder;
    private readonly string _localsettingsFile;

    private IDictionary<string, object> _settings;

    private bool _isInitialized;

    public LocalSettingsService(IFileService fileService, IOptions<LocalSettingsOptions> options)
    {
      _fileService = fileService;
      _options = options.Value;

      _applicationDataFolder = Path.Combine(_localApplicationData, _options.ApplicationDataFolder ?? _defaultApplicationDataFolder);
      _localsettingsFile = _options.LocalSettingsFile ?? _defaultLocalSettingsFile;

      _settings = new Dictionary<string, object>();
    }

    private async Task InitializeAsync()
    {
      if (!_isInitialized)
      {
        _settings = await Task.Run(() => _fileService.Read<IDictionary<string, object>>(_applicationDataFolder, _localsettingsFile)) ?? new Dictionary<string, object>();

        _isInitialized = true;
      }
    }

    private async Task<string?> ReadStringifiedSettingAsync(string key)
    {
      if (RuntimeHelper.IsMSIX)
      {
        if (ApplicationData.Current.LocalSettings.Values.TryGetValue(key, out var obj))
        {
          return (string)obj;
        }
      }
      else
      {
        await InitializeAsync();

        if (_settings != null && _settings.TryGetValue(key, out var obj))
        {
          return (string)obj;
        }
      }

      return null;
    }

    public async Task SaveStringifiedSettingAsync(string key, string value)
    {
      ArgumentNullException.ThrowIfNull(value);

      if (RuntimeHelper.IsMSIX)
      {
        ApplicationData.Current.LocalSettings.Values[key] = value;
      }
      else
      {
        await InitializeAsync();

        _settings[key] = value;

        await Task.Run(() => _fileService.Save(_applicationDataFolder, _localsettingsFile, _settings));
      }
    }

    /// <summary>
    /// Encrypts a string by using the <see cref="ProtectedData"/> class.
    /// </summary>
    /// <param name="stringToEncrypt">String to encrypt.</param>
    /// <returns>Encrypted string.</returns>
    private static string Protect(string stringToEncrypt)
    {
      return Convert.ToBase64String(
        ProtectedData.Protect(
          Encoding.UTF8.GetBytes(stringToEncrypt),
          Encoding.UTF8.GetBytes(_entropy),
          DataProtectionScope.CurrentUser));
    }

    /// <summary>
    /// Decrypts a string by using the <see cref="ProtectedData"/> class.
    /// </summary>
    /// <param name="encryptedString">String to decrypt.</param>
    /// <returns>Decrypted string.</returns>
    private static string Unprotect(string encryptedString)
    {
      return Encoding.UTF8.GetString(
        ProtectedData.Unprotect(
          Convert.FromBase64String(encryptedString),
          Encoding.UTF8.GetBytes(_entropy),
          DataProtectionScope.CurrentUser));
    }

    public async Task<T?> ReadSettingAsync<T>(string key)
    {
      var setting = await ReadStringifiedSettingAsync(key);
      if (setting != null)
      {
        return await Json.ToObjectAsync<T>(setting);
      }

      return default;
    }

    public async Task<T?> ReadProtectedSettingAsync<T>(string key)
    {
      var setting = await ReadStringifiedSettingAsync(key);
      if (setting != null)
      {
        return await Json.ToObjectAsync<T>(Unprotect(setting));
      }

      return default;
    }

    public async Task SaveSettingAsync<T>(string key, T value)
    {
      ArgumentNullException.ThrowIfNull(value);

      var setting = await Json.StringifyAsync(value);
      await SaveStringifiedSettingAsync(key, setting);
    }

    public async Task SaveProtectedSettingAsync<T>(string key, T value)
    {
      ArgumentNullException.ThrowIfNull(value);

      var setting = await Json.StringifyAsync(value);
      await SaveStringifiedSettingAsync(key, Protect(setting));
    }

    public async Task<bool> RemoveSettingAsync(string key)
    {
      if (RuntimeHelper.IsMSIX)
      {
        return ApplicationData.Current.LocalSettings.Values.Remove(key);
      }
      else
      {
        await InitializeAsync();
        var result = _settings.Remove(key);
        await Task.Run(() => _fileService.Save(_applicationDataFolder, _localsettingsFile, _settings));
        return result;
      }
    }
  }
}