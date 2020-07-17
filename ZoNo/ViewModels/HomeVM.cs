using Prism.Commands;
using Prism.Events;
using Splitwise.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using ZoNo.Contracts;
using ZoNo.Contracts.ViewModels;
using ZoNo.Events;

namespace ZoNo.ViewModels
{
  public class HomeVM : VMBase, IHomeVM
  {
    private User mUser;
    private Token mToken;

    private bool mIsUserLoggedIn;
    public bool IsUserLoggedIn
    {
      get => mIsUserLoggedIn;
      set => Set(ref mIsUserLoggedIn, value);
    }

    private ICommand mLogoutUserCommand;
    public ICommand LogoutUserCommand
    {
      get => mLogoutUserCommand;
      protected set => Set(ref mLogoutUserCommand, value);
    }

    public HomeVM(IEventAggregator eventAggregator, ISettings settings)
    {
      EventAggregator = eventAggregator;
      Settings = settings;

      LogoutUserCommand = new DelegateCommand(() => EventAggregator.GetEvent<UserLoggedOutEvent>().Publish(new UserLoggedOutEventArgs() { User = mUser, Token = mToken }));

      EventAggregator.GetEvent<UserLoggedInEvent >().Subscribe(OnUserLoggedIn );
      EventAggregator.GetEvent<UserLoggedOutEvent>().Subscribe(OnUserLoggedOut);
    }

    ~HomeVM()
    {
      EventAggregator.GetEvent<UserLoggedInEvent >().Unsubscribe(OnUserLoggedIn );
      EventAggregator.GetEvent<UserLoggedOutEvent>().Unsubscribe(OnUserLoggedOut);
    }

    private void OnUserLoggedIn(UserLoggedInEventArgs e)
    {
      IsUserLoggedIn = true;
      mUser = e.User;
      mToken = e.Token;
    }

    private void OnUserLoggedOut(UserLoggedOutEventArgs e)
    {
      IsUserLoggedIn = false;
      mUser = null;
      mToken = null;
    }
  }
}
