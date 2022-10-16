using Microsoft.UI.Xaml;

namespace ZoNo2.Contracts.Services;

public interface IThemeSelectorService
{
  ElementTheme Theme { get; }
  Task InitializeAsync();
  Task SetThemeAsync(ElementTheme theme);
  Task SetRequestedThemeAsync();
}
