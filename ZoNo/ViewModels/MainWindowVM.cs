using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using ZoNo.Messages;

namespace ZoNo.ViewModels
{
  public partial class MainWindowVM : ObservableRecipient
  {
    [ObservableProperty]
    private LoginVM _loginVM;

    [ObservableProperty]
    private HomeVM _homeVM;

    [ObservableProperty]
    private bool _userLoggedIn;

    public MainWindowVM(LoginVM loginVM, HomeVM homeVM, IMessenger messenger) : base(messenger)
    {
      LoginVM = loginVM;
      HomeVM = homeVM;

      Messenger.Register<UserLoggedInMessage>(this, OnUserLoggedIn);
      Messenger.Register<UserLoggedOutMessage>(this, OnUserLoggedOut);
    }

    private void OnUserLoggedIn(object recipient, UserLoggedInMessage message)
    {
      UserLoggedIn = true;
    }

    private void OnUserLoggedOut(object recipient, UserLoggedOutMessage message)
    {
      UserLoggedIn = false;
    }
  }
}
