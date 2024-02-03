using Microsoft.UI.Xaml;
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
    private readonly SettingsPageViewModel _settingsPageViewModel = settingsPageViewModel;

    public async Task ActivateAsync(object activationArgs)
    {
      // Execute tasks before activation.
      await InitializeAsync();

      // Handle activation via ActivationHandlers.
      await HandleActivationAsync(activationArgs);

      // Activate the MainWindow.
      App.MainWindow.Activate();

      // Execute tasks after activation.
      await StartupAsync();
    }

    private async Task HandleActivationAsync(object activationArgs)
    {
      var activationHandler = _activationHandlers.FirstOrDefault(h => h.CanHandle(activationArgs));

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