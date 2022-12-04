using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using System.Reflection;
using System.Windows.Input;
using Windows.ApplicationModel;
using ZoNo2.Contracts.Services;
using ZoNo2.Helpers;
using ZoNo2.Messages;

namespace ZoNo2.ViewModels
{
  public partial class SettingsViewModel : ObservableRecipient
  {
    private readonly ITopLevelNavigationService _topLevelNavigationService;
    private readonly IThemeSelectorService _themeSelectorService;
    private ElementTheme _elementTheme;
    private string _versionDescription;

    public ElementTheme ElementTheme
    {
      get => _elementTheme;
      set => SetProperty(ref _elementTheme, value);
    }

    public string VersionDescription
    {
      get => _versionDescription;
      set => SetProperty(ref _versionDescription, value);
    }

    public ICommand SwitchThemeCommand { get; }

    public SettingsViewModel(ITopLevelNavigationService topLevelNavigationService, IThemeSelectorService themeSelectorService, IMessenger messenger) : base(messenger)
    {
      _topLevelNavigationService = topLevelNavigationService;
      _themeSelectorService = themeSelectorService;
      _elementTheme = _themeSelectorService.Theme;
      _versionDescription = GetVersionDescription();

      SwitchThemeCommand = new RelayCommand<ElementTheme>(async (param) =>
      {
        if (ElementTheme != param)
        {
          ElementTheme = param;
          await _themeSelectorService.SetThemeAsync(param);
        }
      });
    }

    private static string GetVersionDescription()
    {
      Version version;

      if (RuntimeHelper.IsMSIX)
      {
        var packageVersion = Package.Current.Id.Version;
        version = new Version(packageVersion.Major, packageVersion.Minor, packageVersion.Build, packageVersion.Revision);
      }
      else
      {
        version = Assembly.GetExecutingAssembly().GetName().Version!;
      }

      return $"{"AppDisplayName".GetLocalized()} - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    [RelayCommand]
    private void Logout()
    {
      Messenger.Send(new UserLoggedOutMessage());
      _topLevelNavigationService.NavigateTo(typeof(LoginViewModel).FullName!, infoOverride: new DrillInNavigationTransitionInfo());
    }
  }
}