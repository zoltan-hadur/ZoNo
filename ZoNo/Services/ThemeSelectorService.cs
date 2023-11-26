﻿using Microsoft.UI.Xaml;
using ZoNo.Contracts.Services;

namespace ZoNo.Services
{
  public class ThemeSelectorService(ILocalSettingsService _localSettingsService) : IThemeSelectorService
  {
    private const string SettingsKey = "AppBackgroundRequestedTheme";

    public ElementTheme Theme { get; set; } = ElementTheme.Default;

    public async Task InitializeAsync()
    {
      Theme = await LoadThemeFromSettingsAsync();
      await Task.CompletedTask;
    }

    public async Task SetThemeAsync(ElementTheme theme)
    {
      Theme = theme;

      await SetRequestedThemeAsync();
      await SaveThemeInSettingsAsync(Theme);
    }

    public async Task SetRequestedThemeAsync()
    {
      if (App.MainWindow.Content is FrameworkElement rootElement)
      {
        rootElement.RequestedTheme = Theme;
      }

      await Task.CompletedTask;
    }

    private async Task<ElementTheme> LoadThemeFromSettingsAsync()
    {
      var themeName = await _localSettingsService.ReadSettingAsync<string>(SettingsKey);

      if (Enum.TryParse(themeName, out ElementTheme cacheTheme))
      {
        return cacheTheme;
      }

      return ElementTheme.Default;
    }

    private async Task SaveThemeInSettingsAsync(ElementTheme theme)
    {
      await _localSettingsService.SaveSettingAsync(SettingsKey, theme.ToString());
    }
  }
}