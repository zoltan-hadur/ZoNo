using CefSharp.Wpf;
using Prism.Commands;
using Prism.Events;
using Splitwise;
using Splitwise.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using ZoNo.Contracts;
using ZoNo.Contracts.ViewModels;
using ZoNo.Events;

namespace ZoNo.ViewModels
{
  public class LoginVM : VMBase, ILoginVM
  {
    private Authorization mAuthorization;
    private User mUser;
    private Token mToken;

    private bool mIsUserAuthenticated;
    public bool IsUserAuthenticated
    {
      get => mIsUserAuthenticated;
      protected set => Set(ref mIsUserAuthenticated, value);
    }

    private string mUserFirstName;
    public string UserFirstName
    {
      get => mUserFirstName;
      protected set => Set(ref mUserFirstName, value);
    }

    private string mUserProfilePicture;
    public string UserProfilePicture
    {
      get => mUserProfilePicture;
      protected set => Set(ref mUserProfilePicture, value);
    }

    private bool mIsUserLoggedIn;
    public bool IsUserLoggedIn
    {
      get => mIsUserLoggedIn;
      set => Set(ref mIsUserLoggedIn, value);
    }

    private ICommand mLoginUserCommand;
    public ICommand LoginUserCommand
    {
      get => mLoginUserCommand;
      protected set => Set(ref mLoginUserCommand, value);
    }

    public LoginVM(IEventAggregator eventAggregator, ISettings settings)
    {
      mAuthorization = new Authorization(consumerKey   : Environment.GetEnvironmentVariable("ZoNo_ConsumerKey"   ),
                                         consumerSecret: Environment.GetEnvironmentVariable("ZoNo_ConsumerSecret"));

      EventAggregator = eventAggregator;
      Settings = settings;

      LoginUserCommand = new DelegateCommand(OnLoginUser);

      EventAggregator.GetEvent<UserLoggedInEvent >().Subscribe(OnUserLoggedIn );
      EventAggregator.GetEvent<UserLoggedOutEvent>().Subscribe(OnUserLoggedOut);

      if (Settings.Get<Token>("Token", out var wToken))
      {
        mToken = wToken;
        IsUserAuthenticated = true;
      }
      if (Settings.Get<Dictionary<string, string>>("UserData", out var wUserData))
      {
        UserFirstName = wUserData["FirstName"];
        UserProfilePicture = wUserData["ProfilePicture"];
      }
    }

    ~LoginVM()
    {
      EventAggregator.GetEvent<UserLoggedInEvent >().Unsubscribe(OnUserLoggedIn );
      EventAggregator.GetEvent<UserLoggedOutEvent>().Unsubscribe(OnUserLoggedOut);
    }

    private void OnUserLoggedIn(UserLoggedInEventArgs e)
    {
      IsUserLoggedIn = true;
    }

    private void OnUserLoggedOut(UserLoggedOutEventArgs e)
    {
      IsUserLoggedIn = false;
      mUser = null;
      mToken = null;
      IsUserAuthenticated = false;
      UserFirstName = null;
      UserProfilePicture = null;
      Settings.Remove("Token");
      Settings.Remove("UserData");
    }

    public bool AuthorizeUser(string url)
    {
      var wAuthorizationCode = mAuthorization.ExtractAuthorizationCode(url);
      if (!string.IsNullOrEmpty(wAuthorizationCode))
      {
        mToken = mAuthorization.GetToken(wAuthorizationCode);
        Settings.Set("Token", mToken);
        mUser = new Client(mToken).GetCurrentUser();
        UserFirstName = mUser.FirstName;
        UserProfilePicture = mUser.Picture.Large;
        Settings.Set("UserData", new Dictionary<string, string>()
        {
          { "FirstName"     , UserFirstName      },
          { "ProfilePicture", UserProfilePicture }
        });
        IsUserAuthenticated = true;
        return true;
      }
      return false;
    }

    private void OnLoginUser()
    {
      // If user is already logged in (i.e. have access token from previous session), navigate to the home page
      if (IsUserAuthenticated)
      {
        EventAggregator.GetEvent<UserLoggedInEvent>().Publish(new UserLoggedInEventArgs() { User = mUser, Token = mToken });
        return;
      }

      // Use CefSharp webbrowser
      using (var wWebBrowser = new ChromiumWebBrowser(mAuthorization.LoginURL))
      {
        var wSplitwiseLoginWindow = new Window()
        {
          Owner = Application.Current.MainWindow,
          WindowStartupLocation = WindowStartupLocation.CenterOwner,
          ResizeMode = ResizeMode.NoResize,
          Title = "Splitwise Login",
          Width = 1000,
          Height = 600,
          Content = wWebBrowser
        };

        wWebBrowser.AddressChanged += (s, e) =>
        {
          if (e.NewValue is string wAddress)
          {
            System.Diagnostics.Debug.WriteLine(wAddress);
            // If the user clicked the cancel button or successfully authorized access to the application, close the window
            if (wAddress == "https://secure.splitwise.com/?error=access_denied&state=" ||
                AuthorizeUser(wAddress))
            {
              wSplitwiseLoginWindow.Close();
            }
          }
        };

        wSplitwiseLoginWindow.ShowDialog();
      }

      if (IsUserAuthenticated)
      {
        EventAggregator.GetEvent<UserLoggedInEvent>().Publish(new UserLoggedInEventArgs() { User = mUser, Token = mToken });
      }
    }
  }
}
