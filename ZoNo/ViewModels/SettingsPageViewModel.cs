using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Reflection;
using System.Text.Json.Nodes;
using Tracer;
using Tracer.Contracts;
using Tracer.Sinks;
using Tracer.Utilities;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using WinRT.Interop;
using ZoNo.Contracts.Services;
using ZoNo.Helpers;
using ZoNo.Models;
using ZoNo.Views;

namespace ZoNo.ViewModels
{
  public partial class SettingsPageViewModel(
    IRulesService rulesService,
    IDialogService dialogService,
    IThemeSelectorService themeSelectorService,
    ILocalSettingsService localSettingsService,
    ITraceFactory traceFactory,
    ITraceDetailProcessor traceDetailProcessor,
    IEnumerable<ITraceSink> traceSinks,
    IMessenger messenger) : ObservableRecipient(messenger)
  {
    private const string SettingInMemoryTraceSink = "Settings_InMemoryTraceSink";
    private const string SettingFileTraceSink = "Settings_FileTraceSink";

    private readonly static TraceDomain _traceDomain = new(typeof(SettingsPageViewModel));
    private readonly IRulesService _rulesService = rulesService;
    private readonly IDialogService _dialogService = dialogService;
    private readonly IThemeSelectorService _themeSelectorService = themeSelectorService;
    private readonly ILocalSettingsService _localSettingsService = localSettingsService;
    private readonly ITraceFactory _traceFactory = traceFactory;
    private readonly ITraceDetailProcessor _traceDetailProcessor = traceDetailProcessor;
    private readonly IEnumerable<ITraceSink> _traceSinks = traceSinks;

    private InMemoryTraceSink InMemoryTraceSink => _traceSinks.Single(traceSink => traceSink is InMemoryTraceSink) as InMemoryTraceSink;
    private FileTraceSink FileTraceSink => _traceSinks.Single(traceSink => traceSink is FileTraceSink) as FileTraceSink;

    private bool _isLoaded = false;

    public TraceLevel[] TraceLevels { get; } = Enum.GetValues<TraceLevel>();

    [ObservableProperty]
    private ElementTheme _elementTheme = themeSelectorService.Theme;

    [ObservableProperty]
    private InMemoryTraceSinkSettings _inMemoryTraceSinkSettings;

    [ObservableProperty]
    private FileTraceSinkSettings _fileTraceSinkSettings;

    [ObservableProperty]
    private string _versionDescription = GetVersionDescription();

    public async Task LoadAsync()
    {
      using var trace = _traceFactory.CreateNew(_traceDomain);
      if (_isLoaded) return;

      InMemoryTraceSinkSettings = await _localSettingsService.ReadSettingAsync<InMemoryTraceSinkSettings>(SettingInMemoryTraceSink) ??
        new InMemoryTraceSinkSettings()
        {
          IsEnabled = true,
          Level = TraceLevel.Debug,
          Size = 50_000
        };
      FileTraceSinkSettings = await _localSettingsService.ReadSettingAsync<FileTraceSinkSettings>(SettingFileTraceSink) ??
        new FileTraceSinkSettings()
        {
          IsEnabled = true,
          Level = TraceLevel.Warning,
          Path = ApplicationData.Current.LocalFolder.Path
        };

      trace.Info($"Setting trace sink properties");
      InMemoryTraceSink.IsEnabled = InMemoryTraceSinkSettings.IsEnabled;
      InMemoryTraceSink.Level = InMemoryTraceSinkSettings.Level;
      InMemoryTraceSink.Size = InMemoryTraceSinkSettings.Size;
      FileTraceSink.IsEnabled = FileTraceSinkSettings.IsEnabled;
      FileTraceSink.Level = FileTraceSinkSettings.Level;
      FileTraceSink.Path = Path.Combine(FileTraceSinkSettings.Path, GetTraceFileName());

      if (!File.Exists(FileTraceSink.Path))
      {
        trace.Info($"Creating {FileTraceSink.Path}");
        using var stream = File.Create(FileTraceSink.Path);
      }
      foreach (var path in Directory.GetFiles(FileTraceSinkSettings.Path, "ZoNo__*.trace"))
      {
        var time = Path.GetFileNameWithoutExtension(path).Replace("ZoNo__", string.Empty);
        if ((DateTime.Now - DateTime.ParseExact(time, "yyyy-MM-dd__HH.mm.ss", null)).TotalDays > 10)
        {
          trace.Info($"Deleting {FileTraceSink.Path}");
          File.Delete(path);
        }
      }

      InMemoryTraceSinkSettings.PropertyChanged += async (s, e) =>
      {
        using var trace = _traceFactory.CreateNew(_traceDomain);
        trace.Info(
          $"{TraceUtility.Format(InMemoryTraceSink.IsEnabled)}, {TraceUtility.Format(InMemoryTraceSinkSettings.IsEnabled)}," +
          $"{TraceUtility.Format(InMemoryTraceSink.Level)}, {TraceUtility.Format(InMemoryTraceSinkSettings.Level)}," +
          $"{TraceUtility.Format(InMemoryTraceSink.Size)}, {TraceUtility.Format(InMemoryTraceSinkSettings.Size)}"
        );
        InMemoryTraceSink.IsEnabled = InMemoryTraceSinkSettings.IsEnabled;
        InMemoryTraceSink.Level = InMemoryTraceSinkSettings.Level;
        InMemoryTraceSink.Size = InMemoryTraceSinkSettings.Size;
        await _localSettingsService.SaveSettingAsync(SettingInMemoryTraceSink, InMemoryTraceSinkSettings);
      };
      FileTraceSinkSettings.PropertyChanged += async (s, e) =>
      {
        using var trace = _traceFactory.CreateNew(_traceDomain);
        trace.Info(
          $"{TraceUtility.Format(FileTraceSink.IsEnabled)}, {TraceUtility.Format(FileTraceSinkSettings.IsEnabled)}," +
          $"{TraceUtility.Format(FileTraceSink.Level)}, {TraceUtility.Format(FileTraceSinkSettings.Level)}," +
          $"{TraceUtility.Format(FileTraceSink.Path)}, {TraceUtility.Format(Path.Combine(FileTraceSinkSettings.Path, GetTraceFileName()))}"
        );
        FileTraceSink.IsEnabled = FileTraceSinkSettings.IsEnabled;
        FileTraceSink.Level = FileTraceSinkSettings.Level;
        FileTraceSink.Path = Path.Combine(FileTraceSinkSettings.Path, GetTraceFileName());
        await _localSettingsService.SaveSettingAsync(SettingFileTraceSink, FileTraceSinkSettings);
      };

      trace.Info($"Starting {nameof(_traceDetailProcessor)}");
      _traceDetailProcessor.Start();

      _isLoaded = true;
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
        version = Assembly.GetExecutingAssembly().GetName().Version;
      }

      return $"ZoNo - {version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
    }

    [RelayCommand]
    private async Task SwitchTheme(ElementTheme elementTheme)
    {
      using var trace = _traceFactory.CreateNew(_traceDomain);

      if (ElementTheme != elementTheme)
      {
        ElementTheme = elementTheme;
        await _themeSelectorService.SetThemeAsync(elementTheme);
      }
    }

    [RelayCommand]
    private async Task ImportRules()
    {
      using var trace = _traceFactory.CreateNew(_traceDomain);

      var openPicker = new FileOpenPicker();
      var hWnd = WindowNative.GetWindowHandle(App.MainWindow);
      InitializeWithWindow.Initialize(openPicker, hWnd);
      openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
      openPicker.FileTypeFilter.Add(".json");

      if (await openPicker.PickSingleFileAsync() is var file && file != null)
      {
        var success = false;
        if (await file.OpenStreamForReadAsync() is var stream &&
            await JsonNode.ParseAsync(stream) is var jsonNode &&
            jsonNode.AsObject() is var jsonObject)
        {
          // Handle previous type of jsons (Import and Splitwise)
          if (jsonObject.ContainsKey("Import"))
          {
            var transactionRules = jsonObject["Import"];
            jsonObject.Remove("Import");
            jsonObject[RuleType.Transaction.ToString()] = transactionRules;
          }
          if (jsonObject.ContainsKey("Splitwise"))
          {
            var expenseRules = jsonObject["Splitwise"];
            jsonObject.Remove("Splitwise");
            jsonObject[RuleType.Expense.ToString()] = expenseRules;
          }
          if (await Json.StringifyAsync(jsonObject) is var json &&
              await Json.ToObjectAsync<Dictionary<RuleType, IList<Rule>>>(json) is var rules &&
              rules != null)
          {
            foreach (var rule in rules)
            {
              await _rulesService.SaveRulesAsync(rule.Key, rule.Value);
            }
            await ShowMessage($"Import from {file.Path} was successful!");
            success = true;
          }
        }
        if (!success)
        {
          await ShowMessage($"Import from {file.Path} was not successful!");
        }
      }
    }

    [RelayCommand]
    private async Task ExportRules()
    {
      using var trace = _traceFactory.CreateNew(_traceDomain);

      var savePicker = new FileSavePicker();
      var hWnd = WindowNative.GetWindowHandle(App.MainWindow);
      InitializeWithWindow.Initialize(savePicker, hWnd);
      savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
      savePicker.FileTypeChoices.Add("JSON", new List<string>() { ".json" });
      savePicker.SuggestedFileName = "Rules.json";

      if (await savePicker.PickSaveFileAsync() is var file && file != null)
      {
        var rules = new Dictionary<RuleType, IReadOnlyCollection<Rule>>();
        foreach (var ruleType in Enum.GetValues<RuleType>())
        {
          rules[ruleType] = _rulesService.GetRules(ruleType);
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

    [RelayCommand]
    private async Task OpenInMemoryTrace()
    {
      using var trace = _traceFactory.CreateNew(_traceDomain);

      var richEditBox = new RichEditBox()
      {
        FontFamily = new FontFamily("Courier New"),
        TextWrapping = TextWrapping.NoWrap,
        Padding = new Thickness(0, 0, 12, 12)
      };
      richEditBox.Document.SetText(TextSetOptions.None, string.Join(Environment.NewLine, InMemoryTraceSink.Traces));
      richEditBox.IsReadOnly = true;
      await _dialogService.ShowDialogAsync(DialogType.Ok, "In Memory Trace", richEditBox);
    }

    [RelayCommand]
    private async Task SelectFileTraceOutputFolder()
    {
      using var trace = _traceFactory.CreateNew(_traceDomain);

      var folderPicker = new FolderPicker();
      var hWnd = WindowNative.GetWindowHandle(App.MainWindow);
      InitializeWithWindow.Initialize(folderPicker, hWnd);
      folderPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
      folderPicker.ViewMode = PickerViewMode.List;

      if (await folderPicker.PickSingleFolderAsync() is var folder && folder != null)
      {
        FileTraceSinkSettings.Path = folder.Path;
      }
    }

    [RelayCommand]
    private void OpenFileTrace()
    {
      using var trace = _traceFactory.CreateNew(_traceDomain);
      System.Diagnostics.Process.Start("explorer", "\"" + FileTraceSinkSettings.Path + "\"");
    }

    [RelayCommand]
    private void ShowReleaseNotes()
    {
      using var trace = _traceFactory.CreateNew(_traceDomain);
      _dialogService.ShowDialogAsync(DialogType.Ok, "Release Notes", new ReleaseNotesView());
    }

    private async Task ShowMessage(string message)
    {
      using var trace = _traceFactory.CreateNew(_traceDomain);
      await _dialogService.ShowDialogAsync(DialogType.Ok, "Message", message);
    }

    private string GetTraceFileName() => $"ZoNo__{System.Diagnostics.Process.GetCurrentProcess().StartTime:yyyy-MM-dd__HH.mm.ss}.trace";
  }
}