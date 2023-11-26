using Microsoft.UI.Xaml;
using ZoNo.Activation;
using ZoNo.Contracts.Services;
using ZoNo.Models;

namespace ZoNo.Services
{
  public class ActivationService(
    ActivationHandler<LaunchActivatedEventArgs> _defaultHandler,
    IEnumerable<IActivationHandler> _activationHandlers,
    IThemeSelectorService _themeSelectorService,
    IRulesService _rulesService,
    IRuleEvaluatorServiceBuilder _ruleEvaluatorServiceBuilder) : IActivationService
  {
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