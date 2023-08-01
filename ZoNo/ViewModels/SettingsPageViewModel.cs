﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Reflection;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using WinRT.Interop;
using ZoNo.Contracts.Services;
using ZoNo.Helpers;
using ZoNo.Models;

namespace ZoNo.ViewModels
{
  public partial class SettingsPageViewModel : ObservableRecipient
  {
    private readonly IRulesService _rulesService;
    private readonly IDialogService _dialogService;
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

    public SettingsPageViewModel(
      IRulesService rulesService,
      IDialogService dialogService,
      IThemeSelectorService themeSelectorService,
      IMessenger messenger) : base(messenger)
    {
      _rulesService = rulesService;
      _dialogService = dialogService;
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

      return $"ZoNo - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    [RelayCommand]
    private async Task ImportRules()
    {
      var openPicker = new FileOpenPicker();
      var hWnd = WindowNative.GetWindowHandle(App.MainWindow);
      InitializeWithWindow.Initialize(openPicker, hWnd);
      openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
      openPicker.FileTypeFilter.Add(".json");

      if (await openPicker.PickSingleFileAsync() is var file && file != null)
      {
        if (await FileIO.ReadTextAsync(file) is var json &&
            json != null &&
            await Json.ToObjectAsync<Dictionary<RuleType, IList<Rule>>>(json) is var rules &&
            rules != null)
        {
          foreach (var rule in rules)
          {
            await _rulesService.SaveRulesAsync(rule.Key, rule.Value);
          }
          await ShowMessage($"Import from {file.Path} was successful!");
        }
        else
        {
          await ShowMessage($"Import from {file.Path} was not successful!");
        }
      }
    }

    [RelayCommand]
    private async Task ExportRules()
    {
      var savePicker = new FileSavePicker();
      var hWnd = WindowNative.GetWindowHandle(App.MainWindow);
      InitializeWithWindow.Initialize(savePicker, hWnd);
      savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
      savePicker.FileTypeChoices.Add("JSON", new List<string>() { ".json" });
      savePicker.SuggestedFileName = "Rules.json";

      if (await savePicker.PickSaveFileAsync() is var file && file != null)
      {
        var rules = new Dictionary<RuleType, IList<Rule>>();
        foreach (var ruleType in Enum.GetValues<RuleType>())
        {
          rules[ruleType] = await _rulesService.GetRulesAsync(ruleType);
        }
        var json = await Json.StringifyAsync(rules);
        await FileIO.WriteTextAsync(file, json);
        var status = await CachedFileManager.CompleteUpdatesAsync(file);
        switch (status)
        {
          case FileUpdateStatus.Complete:
          case FileUpdateStatus.CompleteAndRenamed:
            await ShowMessage($"Export to {file.Path} was successful!");
            break;
          default:
            await ShowMessage($"Export to {file.Path} was not successful!");
            break;
        }
      }
    }

    private async Task ShowMessage(string message)
    {
      await _dialogService.ShowDialogAsync(DialogType.Ok, "Message", message);
    }
  }
}