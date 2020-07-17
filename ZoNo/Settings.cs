using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Security.Cryptography;
using System.Text;
using ZoNo.Contracts;

namespace ZoNo
{
  /// <summary>
  /// Class used to save and load application settings in a safe and protected way.
  /// </summary>
  public class Settings : ISettings
  {
    private IFileSystem mFileSystem;
    private const string mEntropy = "Nora is the love of my life";
    private bool mChanged = false;
    private Dictionary<string, string> mValues;

    public Settings(IFileSystem fileSystem)
    {
      mFileSystem = fileSystem;
    }

    /// <summary>
    /// Gets an application setting by key.
    /// </summary>
    /// <typeparam name="T">The type to convert the setting.</typeparam>
    /// <param name="key">The key of the setting.</param>
    /// <param name="value">The actual setting. It will be defaulted if no setting exists for the given key.</param>
    /// <returns>true if the setting exists; otherwsie, false.</returns>
    public bool Get<T>(string key, out T value)
    {
      var wSuccess = mValues.TryGetValue(key, out var _value);
      value = wSuccess ? JsonConvert.DeserializeObject<T>(_value) :
                         default(T);
      return wSuccess;
    }

    /// <summary>
    /// Sets an application setting by key.
    /// </summary>
    /// <typeparam name="T">The type of the setting.</typeparam>
    /// <param name="key">The key of the setting.</param>
    /// <param name="value">The actual setting.</param>
    public void Set<T>(string key, T value)
    {
      mChanged = true;
      mValues[key] = JsonConvert.SerializeObject(value, typeof(T), Formatting.None, null);
    }

    /// <summary>
    /// Removes an application setting by key.
    /// </summary>
    /// <param name="key">The key of the setting to be removed.</param>
    /// <returns>true if the setting is removed; otherwise, false.</returns>
    public bool Remove(string key)
    {
      mChanged = true;
      return mValues.Remove(key);
    }

    /// <summary>
    /// Loads the encrypted settings file.
    /// </summary>
    /// <param name="path">Path of the encrypted settings file, which will be loaded.</param>
    public void Load(string path)
    {
      mValues = mFileSystem.File.Exists(path) ? JsonConvert.DeserializeObject<Dictionary<string, string>>(Unprotect(mFileSystem.File.ReadAllText(path))) :
                                                new Dictionary<string, string>();
    }

    /// <summary>
    /// Saves the encrypted settings file.
    /// </summary>
    /// <param name="path">Path of the encrypted settings file, where it will be saved</param>
    public void Save(string path)
    {
      if (mChanged)
      {
        mFileSystem.File.WriteAllText(path, Protect(JsonConvert.SerializeObject(mValues, Formatting.Indented)));
        mChanged = false;
      }
    }

    /// <summary>
    /// Encrypts a string by using the <see cref="ProtectedData"/> class.
    /// </summary>
    /// <param name="stringToEncrypt">String to encrypt.</param>
    /// <returns>Encrypted string.</returns>
    private string Protect(string stringToEncrypt)
    {
      return Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(stringToEncrypt), Encoding.UTF8.GetBytes(mEntropy), DataProtectionScope.CurrentUser));
    }

    /// <summary>
    /// Decrypts a string by using the <see cref="ProtectedData"/> class.
    /// </summary>
    /// <param name="encryptedString">String to decrypt.</param>
    /// <returns>Decrypted string.</returns>
    private string Unprotect(string encryptedString)
    {
      return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(encryptedString), Encoding.UTF8.GetBytes(mEntropy), DataProtectionScope.CurrentUser));
    }
  }
}
