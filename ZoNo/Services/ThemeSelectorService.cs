using Microsoft.UI.Xaml;
using Tracer.Contracts;
using ZoNo.Contracts.Services;

namespace ZoNo.Services
{
  public class ThemeSelectorService(
    ILocalSettingsService _localSettingsService,
    ITraceFactory _traceFactory,
    MainWindow _mainWindow) : IThemeSelectorService
  {
    private const string SettingsKey = "AppBackgroundRequestedTheme";

    public ElementTheme Theme { get; set; } = ElementTheme.Default;

    public async Task InitializeAsync()
    {
      using var trace = _traceFactory.CreateNew();
      Theme = await LoadThemeFromSettingsAsync();
      trace.Debug(Format([Theme]));
      await Task.CompletedTask;
    }

    public async Task SetThemeAsync(ElementTheme theme)
    {
      using var trace = _traceFactory.CreateNew();
      Theme = theme;
      trace.Debug(Format([Theme]));

      await SetRequestedThemeAsync();
      await SaveThemeInSettingsAsync(Theme);
    }

    public async Task SetRequestedThemeAsync()
    {
      using var trace = _traceFactory.CreateNew();
      if (_mainWindow.Content is FrameworkElement rootElement)
      {
        rootElement.RequestedTheme = Theme;
        trace.Debug(Format([rootElement.RequestedTheme]));
      }

      await Task.CompletedTask;
    }

    private async Task<ElementTheme> LoadThemeFromSettingsAsync()
    {
      using var trace = _traceFactory.CreateNew();
      var themeName = await _localSettingsService.ReadSettingAsync<string>(SettingsKey);
      trace.Debug(Format([themeName]));

      if (Enum.TryParse(themeName, out ElementTheme cacheTheme))
      {
        return cacheTheme;
      }

      return ElementTheme.Default;
    }

    private async Task SaveThemeInSettingsAsync(ElementTheme theme)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([theme]));
      await _localSettingsService.SaveSettingAsync(SettingsKey, theme.ToString());
    }
  }
}