using Microsoft.UI.Xaml;
using Tracer.Contracts;
using ZoNo.Activation;
using ZoNo.Contracts.Services;
using ZoNo.ViewModels;

namespace ZoNo.Services
{
  public class ActivationService(
    ActivationHandler<LaunchActivatedEventArgs> defaultHandler,
    IEnumerable<IActivationHandler> activationHandlers,
    ITokenService tokenService,
    IThemeSelectorService themeSelectorService,
    IRulesService rulesService,
    IRuleEvaluatorServiceBuilder ruleEvaluatorServiceBuilder,
    ITransactionProcessorService transactionProcessorService,
    IUpdateService updateService,
    ITraceFactory traceFactory,
    SettingsPageViewModel settingsPageViewModel) : IActivationService
  {
    private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler = defaultHandler;
    private readonly IEnumerable<IActivationHandler> _activationHandlers = activationHandlers;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IThemeSelectorService _themeSelectorService = themeSelectorService;
    private readonly IRulesService _rulesService = rulesService;
    private readonly IRuleEvaluatorServiceBuilder _ruleEvaluatorServiceBuilder = ruleEvaluatorServiceBuilder;
    private readonly ITransactionProcessorService _transactionProcessorService = transactionProcessorService;
    private readonly IUpdateService _updateService = updateService;
    private readonly ITraceFactory _traceFactory = traceFactory;
    private readonly SettingsPageViewModel _settingsPageViewModel = settingsPageViewModel;

    public async Task ActivateAsync(object activationArgs)
    {
      using var trace = _traceFactory.CreateNew();

      // Execute tasks before activation.
      await InitializeAsync();

      // Handle activation via ActivationHandlers.
      await HandleActivationAsync(activationArgs);

      // Activate the MainWindow.
      trace.Info("Activate MainWindow");
      App.MainWindow.Activate();

      // Execute tasks after activation.
      await StartupAsync();
    }

    private async Task HandleActivationAsync(object activationArgs)
    {
      using var trace = _traceFactory.CreateNew();
      var activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle(activationArgs));

      trace.Debug(Format([activationHandler]));
      if (activationHandler != null)
      {
        await activationHandler.HandleAsync(activationArgs);
      }

      if (_defaultHandler.CanHandle(activationArgs))
      {
        await _defaultHandler.HandleAsync(activationArgs);
      }
    }

    private async Task InitializeAsync()
    {
      using var trace = _traceFactory.CreateNew();
      await _themeSelectorService.InitializeAsync();
      await Task.WhenAll(
      [
        _tokenService.InitializeAsync(),
        _themeSelectorService.SetRequestedThemeAsync(),
        _settingsPageViewModel.LoadAsync()
      ]);
    }

    private async Task StartupAsync()
    {
      using var trace = _traceFactory.CreateNew();
      await Task.WhenAll(
      [
        _rulesService.InitializeAsync(),
        _ruleEvaluatorServiceBuilder.InitializeAsync(),
        _updateService.CheckForUpdateAsync()
      ]);
      await _transactionProcessorService.InitializeAsync();
    }
  }
}