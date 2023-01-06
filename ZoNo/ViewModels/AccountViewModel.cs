using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Media.Animation;
using Newtonsoft.Json.Linq;
using Splitwise;
using Splitwise.Models;
using ZoNo.Contracts.Services;
using ZoNo.Messages;
using ZoNo.Services;

namespace ZoNo.ViewModels
{
  public partial class AccountViewModel : ObservableRecipient
  {
    private readonly ITopLevelNavigationService _topLevelNavigationService;
    private readonly ITokenService _tokenService;
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

    public AccountViewModel(ITopLevelNavigationService topLevelNavigationService, ITokenService tokenService, IMessenger messenger) : base(messenger)
    {
      _topLevelNavigationService = topLevelNavigationService;
      _tokenService = tokenService;
    }

    public async Task Load()
    {
      if (_isLoaded) return;
      IsLoading = true;

      var token = await _tokenService.GetTokenAsync();
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
