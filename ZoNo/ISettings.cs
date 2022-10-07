namespace ZoNo
{
  /// <summary>
  /// Class used to save and load application settings in a safe and protected way.
  /// </summary>
  public interface ISettings
  {
    /// <summary>
    /// Indicates wether a save will happen after every call to <see cref="Set{T}(string, T)"/> or <see cref="Remove(string)"/>.
    /// </summary>
    public bool AutoSave { get; set; }

    /// <summary>
    /// Gets an application setting by key.
    /// </summary>
    /// <typeparam name="T">The type to convert the setting.</typeparam>
    /// <param name="key">The key of the setting.</param>
    /// <param name="setting">The actual setting.</param>
    /// <returns>True if the setting exists; otherwsie, false.</returns>
    public bool Get<T>(string key, out T setting);

    /// <summary>
    /// Sets an application setting by key.
    /// </summary>
    /// <typeparam name="T">The type of the setting.</typeparam>
    /// <param name="key">The key of the setting.</param>
    /// <param name="setting">The actual setting.</param>
    public void Set<T>(string key, T setting);

    /// <summary>
    /// Removes an application setting by key.
    /// </summary>
    /// <param name="key">The key of the setting to be removed.</param>
    /// <returns>True if the setting is removed; otherwise, false.</returns>
    public bool Remove(string key);

    /// <summary>
    /// Loads the encrypted settings.
    /// </summary>
    public void Load();

    /// <summary>
    /// Saves the encrypted settings.
    /// </summary>
    public void Save();
  }
}
