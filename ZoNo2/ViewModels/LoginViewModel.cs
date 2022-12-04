using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Media.Animation;
using ZoNo2.Contracts.Services;

namespace ZoNo2.ViewModels
{
  public partial class LoginViewModel : ObservableRecipient
  {
    private readonly ITopLevelNavigationService _topLevelNavigationService;

    private static int _id = 0;
    public int Id { get; } = _id++;

    public LoginViewModel(ITopLevelNavigationService topLevelNavigationService)
    {
      _topLevelNavigationService = topLevelNavigationService;
    }

    [RelayCommand]
    private void Login()
    {
      _topLevelNavigationService.NavigateTo(typeof(ShellViewModel).FullName!, infoOverride: new DrillInNavigationTransitionInfo());
    }
  }
}