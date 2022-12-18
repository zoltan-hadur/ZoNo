using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Newtonsoft.Json.Linq;
using Splitwise;
using Splitwise.Models;
using System.ComponentModel;
using System.Drawing;
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

    [ObservableProperty]
    private WebView2? _webView;

    [ObservableProperty]
    private double _captchaOpacity = 0.0;

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
      _token = await _localSettingsService.ReadProtectedSettingAsync<Token>("Protected_Token");
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
          await _localSettingsService.RemoveSettingAsync("Protected_Token");
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

      await WebView!.EnsureCoreWebView2Async();
      WebView.CoreWebView2.CookieManager.DeleteAllCookies();
      WebView.Source = new Uri(authorization.AuthorizationURL);

      WebView.NavigationStarting += (s, e) =>
      {
        uris[e.NavigationId] = e.Uri;
      };

      WebView.NavigationCompleted += async (s, e) =>
      {
        var uri = uris[e.NavigationId];

        // Fill login details then click on log in button
        if (uri == authorization.LoginURL)
        {
          await WebView.ExecuteScriptAsync($"document.querySelector('#credentials_identity').value = '{Email}'");
          await WebView.ExecuteScriptAsync($"document.querySelector('#credentials_password').value = '{Password}'");

          var isCaptchaExists = Convert.ToBoolean(await WebView.ExecuteScriptAsync("document.querySelector('iframe[title=\\'reCAPTCHA\\']') != null"));
          if (isCaptchaExists)
          {
            // Hide scrollbars
            await WebView.ExecuteScriptAsync("document.querySelector('body').style.overflow='hidden'");
            // Display only the captcha
            await WebView.ExecuteScriptAsync("document.querySelector('iframe[title=\\'reCAPTCHA\\']').scrollIntoView()");

            CaptchaOpacity = 1.0;
            var solved = false;
            while (!solved)
            {
              await Task.Delay(300);
              var json = await WebView.CoreWebView2.CallDevToolsProtocolMethodAsync("Page.captureScreenshot", "{}");
              var dataStr = JObject.Parse(json)["data"].Value<string>();
              byte[] data = Convert.FromBase64String(dataStr);
              using var ms = new MemoryStream(data);
              using var bm = new Bitmap(ms);
              var color = bm.GetPixel(27, 43);
              // Green checkmark
              solved = color == Color.FromArgb(255, 0, 158, 85);
            }
            CaptchaOpacity = 0.0;
          }

          // Click on login button
          await WebView.ExecuteScriptAsync("document.querySelector('input[type=\\'submit\\']').click()");
        }

        // Click on authorize button
        if (uri == authorization.AuthorizationURL)
        {
          await WebView.ExecuteScriptAsync($"document.querySelector('input[type=\\'submit\\']').click()");
        }

        if (authorization.IsAccessDenied(uri) || authorization.IsWrongCredentials(uri))
        {
          IsLoggingIn = false;
          IsWrongCredentials = true;
        }

        if (authorization.IsAccessGranted(uri, out var wAuthorizationCode))
        {
          _token = await authorization.GetTokenAsync(wAuthorizationCode);
          await _localSettingsService.SaveProtectedSettingAsync("Protected_Token", _token);
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
    }
  }
}