using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.UI;
using System.ComponentModel;
using Tracer.Contracts;
using Tracer.Sinks;
using ZoNo.Contracts.Services;

namespace ZoNo.ViewModels
{
  public partial class TracesViewModel : ObservableObject
  {
    private const string SettingsTraceLevels = "Settings_TraceLevels";

    private readonly ILocalSettingsService _localSettingsService;

    private bool _isLoaded = false;

    public AdvancedCollectionView TraceDetails { get; }

    [ObservableProperty]
    private bool _isDebugLevelFilterEnabled;

    [ObservableProperty]
    private bool _isInformationLevelFilterEnabled;

    [ObservableProperty]
    private bool _isWarningLevelFilterEnabled;

    [ObservableProperty]
    private bool _isErrorLevelFilterEnabled;

    [ObservableProperty]
    private bool _isFatalLevelFilterEnabled;

    public TracesViewModel(
      ILocalSettingsService localSettingsService,
      IEnumerable<ITraceSink> traceSinks)
    {
      _localSettingsService = localSettingsService;

      var inMemoryTraceSink = traceSinks.Single(traceSink => traceSink is InMemoryTraceSink) as InMemoryTraceSink;
      var traceDetails = inMemoryTraceSink.TraceDetails;
      TraceDetails = new AdvancedCollectionView(traceDetails)
      {
        Filter = TraceLevelFilter,
        SortDescriptions = { new SortDescription(nameof(ITraceDetail.Id), SortDirection.Ascending) }
      };
    }

    public async Task LoadAsync()
    {
      if (_isLoaded) return;

      (IsDebugLevelFilterEnabled, IsWarningLevelFilterEnabled, IsInformationLevelFilterEnabled, IsErrorLevelFilterEnabled, IsFatalLevelFilterEnabled) =
        await _localSettingsService.ReadSettingAsync<(bool, bool, bool, bool, bool)?>(SettingsTraceLevels) ??
        (true, true, true, true, true);

      TraceDetails.RefreshFilter();

      PropertyChanged += TracesViewModel_PropertyChanged;

      _isLoaded = true;
    }

    private async void TracesViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      TraceDetails.RefreshFilter();
      await _localSettingsService.SaveSettingAsync(SettingsTraceLevels, (IsDebugLevelFilterEnabled, IsWarningLevelFilterEnabled, IsInformationLevelFilterEnabled, IsErrorLevelFilterEnabled, IsFatalLevelFilterEnabled));
    }

    private bool TraceLevelFilter(object obj)
    {
      if (obj is ITraceDetail traceDetail)
      {
        return traceDetail.Level == Tracer.TraceLevel.Debug && IsDebugLevelFilterEnabled ||
               traceDetail.Level == Tracer.TraceLevel.Information && IsInformationLevelFilterEnabled ||
               traceDetail.Level == Tracer.TraceLevel.Warning && IsWarningLevelFilterEnabled ||
               traceDetail.Level == Tracer.TraceLevel.Error && IsErrorLevelFilterEnabled ||
               traceDetail.Level == Tracer.TraceLevel.Fatal && IsFatalLevelFilterEnabled;
      }
      return false;
    }
  }
}
