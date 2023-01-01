﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using System.Reflection;
using System.Windows.Input;
using Windows.ApplicationModel;
using ZoNo.Contracts.Services;
using ZoNo.Helpers;

namespace ZoNo.ViewModels
{
  public partial class SettingsViewModel : ObservableRecipient
  {
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

    public SettingsViewModel(IThemeSelectorService themeSelectorService, IMessenger messenger) : base(messenger)
    {
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
  }
}