using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using ZoNo2.Contracts.Services;
using ZoNo2.ViewModels;

namespace ZoNo2.Activation
{
  public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
  {
    private readonly ITopLevelNavigationService _topLevelNavigationService;

    // TODO: inject settings to determine if user is already logged
    public DefaultActivationHandler(ITopLevelNavigationService topLevelNavigationService)
    {
      _topLevelNavigationService = topLevelNavigationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
      // None of the ActivationHandlers has handled the activation.
      return (App.MainWindow.Content as Frame)!.Content == null;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
      // TODO: navigate to ShellPage when logged in, otherwise navigate to LoginPage
      _topLevelNavigationService.NavigateTo(typeof(LoginViewModel).FullName!, infoOverride: new DrillInNavigationTransitionInfo());

      await Task.CompletedTask;
    }
  }
}