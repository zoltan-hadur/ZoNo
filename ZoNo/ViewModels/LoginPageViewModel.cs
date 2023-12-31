using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.Web.WebView2.Core;
using Splitwise.Contracts;
using System.ComponentModel;
using System.Drawing;
using System.Text.Json.Nodes;
using ZoNo.Contracts.Services;

namespace ZoNo.ViewModels
{
  public partial class LoginPageViewModel(
    ITopLevelNavigationService topLevelNavigationService,
    ILocalSettingsService localSettingsService,
    IThemeSelectorService themeSelectorService,
    ISplitwiseAuthorizationService splitwiseAuthorizationService,
    ISplitwiseService splitwiseService,
    ITokenService tokenService) : ObservableRecipient
  {
    private readonly ITopLevelNavigationService _topLevelNavigationService = topLevelNavigationService;
    private readonly ILocalSettingsService _localSettingsService = localSettingsService;
    private readonly IThemeSelectorService _themeSelectorService = themeSelectorService;
    private readonly ISplitwiseAuthorizationService _splitwiseAuthorizationService = splitwiseAuthorizationService;
    private readonly ISplitwiseService _splitwiseService = splitwiseService;
    private readonly ITokenService _tokenService = tokenService;

    private enum State
    {
      LoggedOut,
      LoggingIn,
      LoggedIn,
      Authorizing,
      Authorized
    }

    private const string SettingEmail = "Login_Email";
    private const string SettingRememberMe = "Login_RememberMe";

    /// <summary>
    /// To determine the theme of the captcha.
    /// </summary>
    private ApplicationTheme Theme => _themeSelectorService.Theme == ElementTheme.Default ? Application.Current.RequestedTheme : Enum.Parse<ApplicationTheme>(_themeSelectorService.Theme.ToString());

    /// <summary>
    /// The current url during logging in / authorizing scenario.
    /// </summary>
    private string CurrentURL => WebView.Source.ToString();

    private State _state = State.LoggingIn;

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
    private double _captchaOpacity = 0.0;

    public WebView2 WebView { get; set; }

    public async Task LoadAsync()
    {
      IsRememberMe = await _localSettingsService.ReadSettingAsync<bool>(SettingRememberMe);
      if (IsRememberMe && _tokenService.Token != null)
      {
        Email = await _localSettingsService.ReadSettingAsync<string>(SettingEmail) ?? string.Empty;
        Password = "0123456789";
      }

      await WebView.EnsureCoreWebView2Async();
      WebView.CoreWebView2.DOMContentLoaded += CoreWebView2_DOMContentLoaded;
      WebView.NavigationCompleted += WebView_NavigationCompleted;

      PropertyChanged += LoginViewModel_PropertyChanged;
    }

    private async void LoginViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      switch (e.PropertyName)
      {
        case nameof(Email):
          await _localSettingsService.SaveSettingAsync(SettingEmail, Email);
          break;
        case nameof(IsRememberMe):
          await _localSettingsService.SaveSettingAsync(SettingRememberMe, IsRememberMe);
          break;
      }

      // If any of the following changes, forget the token as it invalidates it
      switch (e.PropertyName)
      {
        case nameof(Email):
        case nameof(Password):
        case nameof(IsRememberMe):
          _tokenService.Token = null;
          await _tokenService.SaveAsync();
          break;
      }
    }

    [RelayCommand]
    private void Login()
    {
      IsWrongCredentials = false;

      if (IsRememberMe && _tokenService.Token != null)
      {
        _splitwiseService.Token = _tokenService.Token;
        _topLevelNavigationService.NavigateTo(typeof(ShellPageViewModel).FullName, infoOverride: new DrillInNavigationTransitionInfo());
      }
      else
      {
        IsLoggingIn = true;
        WebView.Source = new Uri(_splitwiseAuthorizationService.LoginURL);
      }
    }

    private async void CoreWebView2_DOMContentLoaded(CoreWebView2 sender, CoreWebView2DOMContentLoadedEventArgs args)
    {
      await sender.ExecuteScriptAsync($"document.querySelector('div.g-recaptcha').setAttribute('data-theme', '{Theme.ToString().ToLowerInvariant()}')");
    }

    private async void WebView_NavigationCompleted(WebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
    {
      if (IsWrongCredentials) return;

      switch (_state)
      {
        case State.LoggedOut:
          {
            sender.Source = new Uri(_splitwiseAuthorizationService.LoginURL);
            _state = State.LoggingIn;
            break;
          }
        case State.LoggingIn:
          {
            if (CurrentURL == _splitwiseAuthorizationService.LoginURL)
            {
              // Fill login details
              await sender.ExecuteScriptAsync($"document.querySelector('#credentials_identity').value = '{Email}'");
              await sender.ExecuteScriptAsync($"document.querySelector('#credentials_password').value = '{Password}'");

              var captchaPath = "body>main iframe[title=\\'reCAPTCHA\\']";
              var isCaptchaExists = Convert.ToBoolean(await sender.ExecuteScriptAsync($"document.querySelector('{captchaPath}') != null"));
              if (isCaptchaExists)
              {
                // Hide scrollbars and display only the captcha
                await sender.ExecuteScriptAsync("document.querySelector('body').style.overflow='hidden'");
                await sender.ExecuteScriptAsync($"document.querySelector('{captchaPath}').scrollIntoView()");

                // Some additional styling when needed
                if (Theme == ApplicationTheme.Dark)
                {
                  await sender.ExecuteScriptAsync("document.body.style.backgroundColor = 'black'");
                  await sender.ExecuteScriptAsync($"document.querySelector('{captchaPath}').parentElement.parentElement.parentElement.parentElement.parentElement.style.backgroundColor = 'black'");
                }

                // Make captcha visible
                CaptchaOpacity = 1.0;

                // Wait until captcha is not solved
                var solved = false;
                while (!solved)
                {
                  await Task.Delay(10);
                  var json = await sender.CoreWebView2.CallDevToolsProtocolMethodAsync("Page.captureScreenshot", "{}");
                  var dataStr = (JsonNode.Parse(json)?["data"]?.GetValue<string>()) ?? throw new Exception($"Following json does not have 'data' key: {json}");
                  byte[] data = Convert.FromBase64String(dataStr);
                  using (var ms = new MemoryStream(data))
                  using (var bm = new Bitmap(ms))
                  {
                    // Check for green checkmark
                    var color = bm.GetPixel(27, 43);
                    solved = color == Color.FromArgb(255, 0, 158, 85);
                  }

                  var harderCaptchaPath = "body>div[style*=\\'visibility: visible\\'] iframe[title=\\'recaptcha challenge expires in two minutes\\']";
                  var isHarderCaptchaExists = Convert.ToBoolean(await sender.ExecuteScriptAsync($"document.querySelector('{harderCaptchaPath}') != null"));
                  if (isHarderCaptchaExists)
                  {
                    var originalWidth = sender.Width;
                    var originalHeight = sender.Height;
                    var originalRow = sender.GetValue(Grid.RowProperty);
                    var originalRowSpan = sender.GetValue(Grid.RowSpanProperty);
                    var originalVerticalAlignment = sender.VerticalAlignment;

                    // Adapt size and position for image captcha
                    sender.Width = 300;
                    sender.Height = 480;
                    sender.SetValue(Grid.RowProperty, 0);
                    sender.SetValue(Grid.RowSpanProperty, 3);
                    sender.VerticalAlignment = VerticalAlignment.Center;

                    // Hide scrollbars
                    await sender.ExecuteScriptAsync($"document.querySelector('{harderCaptchaPath}').parentNode.style.overflow = 'hidden'");

                    // Display only the captcha
                    await sender.ExecuteScriptAsync($"document.querySelector('{harderCaptchaPath}').parentNode.style.width = '300px'");
                    await sender.ExecuteScriptAsync($"document.querySelector('{harderCaptchaPath}').parentNode.style.height = '480px'");
                    await sender.ExecuteScriptAsync($"document.querySelector('{harderCaptchaPath}').scrollIntoView()");
                    await sender.ExecuteScriptAsync("window.scrollBy(1, 0)");

                    // Captcha is solved when it disappears
                    while (isHarderCaptchaExists)
                    {
                      isHarderCaptchaExists = Convert.ToBoolean(await sender.ExecuteScriptAsync($"document.querySelector('{harderCaptchaPath}') != null"));
                      await Task.Delay(10);
                    }
                    solved = true;

                    // Restore original values
                    sender.Width = originalWidth;
                    sender.Height = originalHeight;
                    sender.SetValue(Grid.RowProperty, originalRow);
                    sender.SetValue(Grid.RowSpanProperty, originalRowSpan);
                    sender.VerticalAlignment = originalVerticalAlignment;
                  }
                }

                // After captcha is solved, hide it
                CaptchaOpacity = 0.0;
              }

              // Click on login button
              await sender.ExecuteScriptAsync("document.querySelector('input[type=\\'submit\\']').click()");
              _state = State.LoggedIn;
            }
            else
            {
              // If drop down of user exists in top right corner
              var userLoggedIn = Convert.ToBoolean(await sender.ExecuteScriptAsync("document.querySelector('a.dropdown-toggle') != null"));
              if (userLoggedIn)
              {
                // Expand drop down then click on logout
                await sender.ExecuteScriptAsync("document.querySelector('a.dropdown-toggle').click()");
                await sender.ExecuteScriptAsync("document.querySelector('a[href=\\'/logout\\']').click()");
                _state = State.LoggedOut;
              }
              else
              {
                throw new Exception("URL is not LoginURL and there is no user logged in!");
              }
            }
            break;
          }
        case State.LoggedIn:
          {
            if (_splitwiseAuthorizationService.IsWrongCredentials(CurrentURL))
            {
              IsLoggingIn = false;
              IsWrongCredentials = true;
              sender.NavigateToString(string.Empty);
              _state = State.LoggingIn;
            }
            else
            {
              sender.Source = new Uri(_splitwiseAuthorizationService.AuthorizationURL);
              _state = State.Authorizing;
            }
            break;
          }
        case State.Authorizing:
          {
            if (CurrentURL == _splitwiseAuthorizationService.AuthorizationURL)
            {
              // Click on authorize button
              await sender.ExecuteScriptAsync($"document.querySelector('input[type=\\'submit\\']').click()");
              _state = State.Authorized;
            }
            else
            {
              throw new Exception("Should be clicking on authorize button but URL is not AuthorizationURL!");
            }
            break;
          }
        case State.Authorized:
          {
            if (_splitwiseAuthorizationService.ExtractAuthorizationCode(CurrentURL, out var wAuthorizationCode))
            {
              var token = await _splitwiseAuthorizationService.GetTokenAsync(wAuthorizationCode);
              _tokenService.Token = token;
              _splitwiseService.Token = token;
              if (IsRememberMe)
              {
                await _tokenService.SaveAsync();
              }
              _topLevelNavigationService.NavigateTo(typeof(ShellPageViewModel).FullName, infoOverride: new DrillInNavigationTransitionInfo());
            }
            else
            {
              throw new Exception("Access not granted!");
            }
            break;
          }
      }
    }
  }
}