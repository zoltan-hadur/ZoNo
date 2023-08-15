using Microsoft.UI.Xaml;
using ZoNo.Activation;
using ZoNo.Contracts.Services;
using ZoNo.Models;

namespace ZoNo.Services
{
  public class ActivationService : IActivationService
  {
    private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler;
    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly IRulesService _rulesService;
    private readonly IRuleEvaluatorServiceBuilder _ruleEvaluatorServiceBuilder;

    public ActivationService(
      ActivationHandler<LaunchActivatedEventArgs> defaultHandler,
      IEnumerable<IActivationHandler> activationHandlers,
      IThemeSelectorService themeSelectorService,
      IRulesService rulesService,
      IRuleEvaluatorServiceBuilder ruleEvaluatorServiceBuilder)
    {
      _defaultHandler = defaultHandler;
      _activationHandlers = activationHandlers;
      _themeSelectorService = themeSelectorService;
      _rulesService = rulesService;
      _ruleEvaluatorServiceBuilder = ruleEvaluatorServiceBuilder;
    }

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
      await _themeSelectorService.InitializeAsync().ConfigureAwait(false);
      _ = _rulesService.LoadRulesAsync();
      _ = _ruleEvaluatorServiceBuilder.BuildAsync<Transaction, Transaction>(Array.Empty<Rule>());
      await Task.CompletedTask;
    }

    private async Task StartupAsync()
    {
      await _themeSelectorService.SetRequestedThemeAsync();
      await Task.CompletedTask;
    }
  }
}