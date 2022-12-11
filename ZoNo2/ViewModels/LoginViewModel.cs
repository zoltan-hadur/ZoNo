using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Splitwise;
using Splitwise.Models;
using System.ComponentModel;
using ZoNo2.Contracts.Services;

namespace ZoNo2.ViewModels
{
  public partial class LoginViewModel : ObservableRecipient
  {
    private readonly ITopLevelNavigationService _topLevelNavigationService;
    private readonly ILocalSettingsService _localSettingsService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private bool _isRememberMe = false;

    [ObservableProperty]
    private bool _isLoggingIn = false;

    [ObservableProperty]
    private bool _isWrongCredentials = false;

    private bool _isLoading = false;

    private Token? _token;

    public LoginViewModel(ITopLevelNavigationService topLevelNavigationService, ILocalSettingsService localSettingsService)
    {
      _topLevelNavigationService = topLevelNavigationService;
      _localSettingsService = localSettingsService;

      PropertyChanged += LoginViewModel_PropertyChanged;
    }

    public async Task Load()
    {
      _isLoading = true;

      IsRememberMe = await _localSettingsService.ReadSettingAsync<bool>("Login_RememberMe");
      _token = await _localSettingsService.ReadSettingAsync<Token>("Token");
      if (IsRememberMe && _token != null)
      {
        Email = await _localSettingsService.ReadSettingAsync<string>("Login_Email") ?? string.Empty;
        Password = "0123456789";
      }

      _isLoading = false;
    }

    private async void LoginViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
      if (_isLoading) return;

      switch (e.PropertyName)
      {
        case nameof(Email):
          await _localSettingsService.SaveSettingAsync("Login_Email", Email);
          break;
        case nameof(IsRememberMe):
          await _localSettingsService.SaveSettingAsync("Login_RememberMe", IsRememberMe);
          break;
      }

      // If the following changes, forget the token
      switch (e.PropertyName)
      {
        case nameof(Email):
        case nameof(Password):
        case nameof(IsRememberMe):
          _token = null;
          await _localSettingsService.RemoveSettingAsync("Token");
          break;
      }
    }

    [RelayCommand]
    private async void Login()
    {
      if (IsRememberMe && _token != null)
      {
        _topLevelNavigationService.NavigateTo(typeof(ShellViewModel).FullName!, infoOverride: new DrillInNavigationTransitionInfo());
        return;
      }

      IsLoggingIn = true;

      var authorization = new Authorization(consumerKey: Environment.GetEnvironmentVariable("ZoNo_ConsumerKey"),
                                            consumerSecret: Environment.GetEnvironmentVariable("ZoNo_ConsumerSecret"));

      var uris = new Dictionary<ulong, string>();

      var webView = new WebView2();
      await webView.EnsureCoreWebView2Async();
      webView.CoreWebView2.CookieManager.DeleteAllCookies();
      webView.Source = new Uri(authorization.AuthorizationURL);

      //var window = new WindowEx()
      //{
      //  Title = "Splitwise Login",
      //  Width = 600,
      //  Height = 670,
      //  Content = webView
      //};

      webView.NavigationStarting += (s, e) =>
      {
        uris[e.NavigationId] = e.Uri;
      };

      webView.NavigationCompleted += async (s, e) =>
      {
        var uri = uris[e.NavigationId];

        // Fill login details then click on log in button
        if (uri == authorization.LoginURL)
        {
          await webView.ExecuteScriptAsync($"document.querySelector('#credentials_identity').value = '{Email}'");
          await webView.ExecuteScriptAsync($"document.querySelector('#credentials_password').value = '{Password}'");
          await webView.ExecuteScriptAsync($"document.querySelector('input[type=\\'submit\\']').click()");
        }

        // Click on authorize button
        if (uri == authorization.AuthorizationURL)
        {
          await webView.ExecuteScriptAsync($"document.querySelector('input[type=\\'submit\\']').click()");
        }

        if (authorization.IsAccessDenied(uri) || authorization.IsWrongCredentials(uri))
        {
          IsLoggingIn = false;
          IsWrongCredentials = true;
        }

        if (authorization.IsAccessGranted(uri, out var wAuthorizationCode))
        {
          _token = await authorization.GetTokenAsync(wAuthorizationCode);
          await _localSettingsService.SaveSettingAsync("Token", _token);
          _topLevelNavigationService.NavigateTo(typeof(ShellViewModel).FullName!, infoOverride: new DrillInNavigationTransitionInfo());
          //using (var client = new Client(token))
          //{
          //  //var user = client.GetCurrentUser();
          //  //_topLevelNavigationService.NavigateTo(typeof(ShellViewModel).FullName!, infoOverride: new DrillInNavigationTransitionInfo());
          //  //User = new User(token, user.FirstName, user.Picture.Large);
          //  //_settings.Set(nameof(User), User);
          //}
          //window.Close();
        }
      };

      //window.Show();
    }
  }
}