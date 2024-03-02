using CommunityToolkit.Mvvm.Messaging;
using Tracer;
using Tracer.Contracts;
using ZoNo.Contracts.Services;
using ZoNo.Messages;
using ZoNo.ViewModels;

namespace ZoNo.Services
{
  public class ActivationService(
    ITokenService _tokenService,
    IThemeSelectorService _themeSelectorService,
    IRulesService _rulesService,
    IRuleEvaluatorServiceBuilder _ruleEvaluatorServiceBuilder,
    ITransactionProcessorService _transactionProcessorService,
    IUpdateService _updateService,
    ITraceFactory _traceFactory,
    IMessenger _messenger,
    SettingsPageViewModel _settingsPageViewModel) : IActivationService
  {
    public async Task ActivateAsync(object activationArgs)
    {
      using var trace = _traceFactory.CreateNew();

      // Execute tasks before activation.
      await InitializeAsync();

      // Activate the MainWindow.
      _messenger.Send<ActivateMainWindowMessage>();

      // Execute tasks after activation.
      await StartupAsync();
    }

    private async Task InitializeAsync()
    {
      using var trace = _traceFactory.CreateNew();
      await _themeSelectorService.InitializeAsync();
      await Task.WhenAll(
      [
        TraceFactory.HandleAsAsyncVoid(_tokenService.InitializeAsync),
        TraceFactory.HandleAsAsyncVoid(_themeSelectorService.SetRequestedThemeAsync),
        TraceFactory.HandleAsAsyncVoid(_settingsPageViewModel.LoadAsync)
      ]);
    }

    private async Task StartupAsync()
    {
      using var trace = _traceFactory.CreateNew();
      await Task.WhenAll(
      [
        TraceFactory.HandleAsAsyncVoid(_rulesService.InitializeAsync),
        TraceFactory.HandleAsAsyncVoid(_ruleEvaluatorServiceBuilder.InitializeAsync),
        TraceFactory.HandleAsAsyncVoid(_updateService.CheckForUpdateAsync)
      ]);
      await _transactionProcessorService.InitializeAsync();
    }
  }
}