using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Animation;
using Tracer.Contracts;
using ZoNo.Contracts.Services;
using ZoNo.ViewModels;

namespace ZoNo.Activation
{
  public class DefaultActivationHandler(
    ITraceFactory traceFactory,
    ITopLevelNavigationService topLevelNavigationService) : ActivationHandler<LaunchActivatedEventArgs>(traceFactory)
  {
    private readonly ITraceFactory _traceFactory = traceFactory;
    private readonly ITopLevelNavigationService _topLevelNavigationService = topLevelNavigationService;

    protected override bool CanHandleInternal(LaunchActivatedEventArgs args)
    {
      using var trace = _traceFactory.CreateNew();
      // None of the ActivationHandlers has handled the activation.
      return _topLevelNavigationService.Frame?.Content is null;
    }

    protected async override Task HandleInternalAsync(LaunchActivatedEventArgs args)
    {
      using var trace = _traceFactory.CreateNew();
      _topLevelNavigationService.NavigateTo(typeof(LoginPageViewModel).FullName, infoOverride: new DrillInNavigationTransitionInfo());
      await Task.CompletedTask;
    }
  }
}