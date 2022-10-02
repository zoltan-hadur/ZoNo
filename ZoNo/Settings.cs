using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ZoNo
{
    /// <inheritdoc cref="ISettings"/>
    public class Settings : ISettings
  {
    private readonly IFileSystem _fileSystem;
    private readonly string _path;
    private const string _entropy = "Nora is the love of my life";
    private Dictionary<string, string> _settings;
    private readonly object _lock = new object();

    private bool _autoSave = true;
    /// <inheritdoc/>
    /// <remarks>True by default.</remarks>
    public bool AutoSave
    {
      get
      {
        lock (_lock)
        {
          return _autoSave;
        }
      }
      set
      {
        lock (_lock)
        {
          _autoSave = value;
        }
      }
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="fileSystem">Abstraction for ability to unit test.</param>
    /// <param name="path">Path where the encrypted settings will be stored.</param>
    public Settings(IFileSystem fileSystem, string path)
    {
      _fileSystem = fileSystem;
      _path = path;
      Load();
    }

    /// <inheritdoc/>
    public bool Get<T>(string key, out T setting)
    {
      lock (_lock)
      {
        if (_settings.TryGetValue(key, out var value))
        {
          setting = JsonSerializer.Deserialize<T>(value);
          return true;
        }
        setting = default(T);
        return false;
      }
    }

    /// <inheritdoc/>
    public void Set<T>(string key, T setting)
    {
      lock (_lock)
      {
        _settings[key] = JsonSerializer.Serialize(setting);
        if (AutoSave)
        {
          Save();
        }
      }
    }

    /// <inheritdoc/>
    public bool Remove(string key)
    {
      lock (_lock)
      {
        var wRemoved = _settings.Remove(key);
        if (AutoSave)
        {
          Save();
        }
        return wRemoved;
      }
    }

    /// <inheritdoc/>
    public void Load()
    {
      lock (_lock)
      {
        _settings = _fileSystem.File.Exists(_path) ?
          JsonSerializer.Deserialize<Dictionary<string, string>>(Unprotect(_fileSystem.File.ReadAllText(_path))) :
          new Dictionary<string, string>();
      }
    }

    /// <inheritdoc/>
    public void Save()
    {
      lock (_lock)
      {
        _fileSystem.File.WriteAllText(_path, Protect(JsonSerializer.Serialize(_settings)));
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
  }
}
