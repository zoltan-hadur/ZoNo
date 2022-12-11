using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Media.Animation;
using ZoNo2.Contracts.Services;
using ZoNo2.Messages;

namespace ZoNo2.ViewModels
{
  public partial class AccountViewModel : ObservableRecipient
  {
    private readonly ITopLevelNavigationService _topLevelNavigationService;

    public AccountViewModel(ITopLevelNavigationService topLevelNavigationService, IMessenger messenger) : base(messenger)
    {
      _topLevelNavigationService = topLevelNavigationService;
    }

    [RelayCommand]
    private void Logout()
    {
      Messenger.Send(new UserLoggedOutMessage());
      _topLevelNavigationService.NavigateTo(typeof(LoginViewModel).FullName!, infoOverride: new DrillInNavigationTransitionInfo());
    }
  }
}
