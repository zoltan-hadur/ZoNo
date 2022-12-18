using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Media.Animation;
using Newtonsoft.Json.Linq;
using Splitwise;
using Splitwise.Models;
using ZoNo2.Contracts.Services;
using ZoNo2.Messages;
using ZoNo2.Services;

namespace ZoNo2.ViewModels
{
  public partial class AccountViewModel : ObservableRecipient
  {
    private readonly ITopLevelNavigationService _topLevelNavigationService;
    private readonly ILocalSettingsService _localSettingsService;
    private bool _isLoaded = false;

    [ObservableProperty]
    private string _profilePicture = " ";

    [ObservableProperty]
    private string? _firstName;

    [ObservableProperty]
    private string? _lastName;

    [ObservableProperty]
    private string? _email;

    [ObservableProperty]
    private CountryCode? _countryCode;

    [ObservableProperty]
    private CurrencyCode? _defaultCurrency;

    [ObservableProperty]
    private bool _isLoading = false;

    public AccountViewModel(ITopLevelNavigationService topLevelNavigationService, ILocalSettingsService localSettingsService, IMessenger messenger) : base(messenger)
    {
      _topLevelNavigationService = topLevelNavigationService;
      _localSettingsService = localSettingsService;
    }

    public async Task Load()
    {
      if (_isLoaded) return;
      IsLoading = true;

      var token = await _localSettingsService.ReadProtectedSettingAsync<Token>("Protected_Token");
      using var client = new Client(token);
      var user = await client.GetCurrentUserAsync();
      ProfilePicture = user.Picture.Medium;
      FirstName = user.FirstName;
      LastName = user.LastName;
      Email = user.Email;
      CountryCode = user.CountryCode;
      DefaultCurrency = user.DefaultCurrency;
      _isLoaded = true;

      IsLoading = false;
    }

    [RelayCommand]
    private void Logout()
    {
      Messenger.Send(new UserLoggedOutMessage());
      _topLevelNavigationService.NavigateTo(typeof(LoginViewModel).FullName!, infoOverride: new DrillInNavigationTransitionInfo());
    }
  }
}
