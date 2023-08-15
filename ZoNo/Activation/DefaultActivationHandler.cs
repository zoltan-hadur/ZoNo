using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using ZoNo.Contracts.Services;
using ZoNo.ViewModels;

namespace ZoNo.Activation
{
  public class DefaultActivationHandler : ActivationHandler<LaunchActivatedEventArgs>
  {
    private readonly ITopLevelNavigationService _topLevelNavigationService;

    public DefaultActivationHandler(ITopLevelNavigationService topLevelNavigationService)
    {
      _topLevelNavigationService = topLevelNavigationService;
    }

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
      // None of the ActivationHandlers has handled the activation.
      return _topLevelNavigationService.Frame?.Content == null;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
      _topLevelNavigationService.NavigateTo(typeof(LoginPageViewModel).FullName!, infoOverride: new DrillInNavigationTransitionInfo());

      await Task.CompletedTask;
    }
  }
}