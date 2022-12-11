using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Web.WebView2.Wpf;
using Splitwise;
using System;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using ZoNo.Messages;
using ZoNo.Models;

namespace ZoNo.ViewModels
{
  public partial class LoginVM : ObservableRecipient
  {
    private Authorization _authorization;

    [ObservableProperty]
    private User _user;

    private ISettings _settings;

    public LoginVM(ISettings settings, IMessenger messenger) : base(messenger)
    {
      _authorization = new Authorization(consumerKey: Environment.GetEnvironmentVariable("ZoNo_ConsumerKey"),
                                         consumerSecret: Environment.GetEnvironmentVariable("ZoNo_ConsumerSecret"));

      _settings = settings;
      if (_settings.Get<User>(nameof(User), out var user))
      {
        User = user;
      }

      Messenger.Register<UserLoggedOutMessage>(this, OnUserLoggedOut);
    }

    private void OnUserLoggedOut(object recipient, UserLoggedOutMessage message)
    {
      User = null;
      _settings.Remove(nameof(User));
    }

    [RelayCommand]
    private void LoginUser()
    {
      if (User == null)
      {
        using (var webView = new WebView2() { Source = new Uri(_authorization.AuthorizationURL) })
        {
          var window = new Window()
          {
            Owner = Application.Current.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize,
            Title = "Splitwise Login",
            Width = 600,
            Height = 670,
            Content = webView
          };

          webView.NavigationStarting += async (s, e) =>
          {
            if (_authorization.IsAccessDenied(e.Uri))
            {
              window.Close();
            }
            else if (_authorization.IsAccessGranted(e.Uri, out var wAuthorizationCode))
            {
              var token = await _authorization.GetTokenAsync(wAuthorizationCode);
              using (var client = new Client(token))
              {
                var user = client.GetCurrentUser();
                User = new User(token, user.FirstName, user.Picture.Large);
                _settings.Set(nameof(User), User);
              }
              window.Close();
            }
          };

          window.ShowDialog();
        }
      }

      if (User != null)
      {
        Messenger.Send(new UserLoggedInMessage(User));
        return;
      }
    }
  }
}
