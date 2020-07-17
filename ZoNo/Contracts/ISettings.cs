using System;
using System.Collections.Generic;
using System.Text;

namespace ZoNo.Contracts
{
  /// <summary>
  /// Class used to save and load application settings in a safe and protected way.
  /// </summary>
  public interface ISettings
  {
    /// <summary>
    /// Gets an application setting by key.
    /// </summary>
    /// <typeparam name="T">The type to convert the setting.</typeparam>
    /// <param name="key">The key of the setting.</param>
    /// <param name="value">The actual setting. It will be defaulted if no setting exists for the given key.</param>
    /// <returns>true if the setting exists; otherwsie, false.</returns>
    public bool Get<T>(string key, out T value);

    /// <summary>
    /// Sets an application setting by key.
    /// </summary>
    /// <typeparam name="T">The type of the setting.</typeparam>
    /// <param name="key">The key of the setting.</param>
    /// <param name="value">The actual setting.</param>
    public void Set<T>(string key, T value);

    /// <summary>
    /// Removes an application setting by key.
    /// </summary>
    /// <param name="key">The key of the setting to be removed.</param>
    /// <returns>true if the setting is removed; otherwise, false.</returns>
    public bool Remove(string key);

    /// <summary>
    /// Loads the encrypted settings file.
    /// </summary>
    /// <param name="path">Path of the encrypted settings file, which will be loaded.</param>
    public void Load(string path);

    /// <summary>
    /// Saves the encrypted settings file.
    /// </summary>
    /// <param name="path">Path of the encrypted settings file, where it will be saved</param>
    public void Save(string path);
  }
}
