using CommunityToolkit.Mvvm.Messaging;
using Tracer;
using Tracer.Contracts;
using ZoNo.Contracts.Services;
using ZoNo.Messages;
using ZoNo.ViewModels;

namespace ZoNo.Services
{
  public class ActivationService(
    ITokenService tokenService,
    IThemeSelectorService themeSelectorService,
    IRulesService rulesService,
    IRuleEvaluatorServiceBuilder ruleEvaluatorServiceBuilder,
    ITransactionProcessorService transactionProcessorService,
    IUpdateService updateService,
    ITraceFactory traceFactory,
    IMessenger messenger,
    SettingsPageViewModel settingsPageViewModel) : IActivationService
  {
    private readonly ITokenService _tokenService = tokenService;
    private readonly IThemeSelectorService _themeSelectorService = themeSelectorService;
    private readonly IRulesService _rulesService = rulesService;
    private readonly IRuleEvaluatorServiceBuilder _ruleEvaluatorServiceBuilder = ruleEvaluatorServiceBuilder;
    private readonly ITransactionProcessorService _transactionProcessorService = transactionProcessorService;
    private readonly IUpdateService _updateService = updateService;
    private readonly ITraceFactory _traceFactory = traceFactory;
    private readonly IMessenger _messenger = messenger;
    private readonly SettingsPageViewModel _settingsPageViewModel = settingsPageViewModel;

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