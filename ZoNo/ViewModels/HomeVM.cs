using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System.Windows.Controls;
using System.Windows;
using ZoNo.Messages;
using ZoNo.Models;
using System.Runtime;

namespace ZoNo.ViewModels
{
  public partial class HomeVM : ObservableRecipient
  {
    [ObservableProperty]
    private User _user;

    public HomeVM(IMessenger messenger) : base(messenger)
    {
      Messenger.Register<UserLoggedInMessage>(this, OnUserLoggedIn);
    }

    private void OnUserLoggedIn(object recipient, UserLoggedInMessage message)
    {
      User = message.Value;
    }

    [RelayCommand]
    private void LogoutUser()
    {
      User = null;
      Messenger.Send(new UserLoggedOutMessage(User));
    }
  }
}
