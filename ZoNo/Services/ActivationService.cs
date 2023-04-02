using Microsoft.UI.Xaml;
using ZoNo.Activation;
using ZoNo.Contracts.Services;

namespace ZoNo.Services
{
  public class ActivationService : IActivationService
  {
    private readonly ActivationHandler<LaunchActivatedEventArgs> _defaultHandler;
    private readonly IEnumerable<IActivationHandler> _activationHandlers;
    private readonly IThemeSelectorService _themeSelectorService;
    private readonly IImportRulesService _importRulesService;
    private readonly ISplitwiseRulesService _splitwiseRulesService;

    public ActivationService(
      ActivationHandler<LaunchActivatedEventArgs> defaultHandler,
      IEnumerable<IActivationHandler> activationHandlers,
      IThemeSelectorService themeSelectorService,
      IImportRulesService importRulesService,
      ISplitwiseRulesService splitwiseRulesService)
    {
      _defaultHandler = defaultHandler;
      _activationHandlers = activationHandlers;
      _themeSelectorService = themeSelectorService;
      _importRulesService = importRulesService;
      _splitwiseRulesService = splitwiseRulesService;
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
      _ = _importRulesService.LoadRulesAsync();
      _ = _splitwiseRulesService.LoadRulesAsync();
      await Task.CompletedTask;
    }

    private async Task StartupAsync()
    {
      await _themeSelectorService.SetRequestedThemeAsync();
      await Task.CompletedTask;
    }
  }
}