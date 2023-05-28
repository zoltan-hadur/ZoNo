using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Media.Animation;
using Newtonsoft.Json.Linq;
using Splitwise;
using Splitwise.Contracts;
using Splitwise.Models;
using ZoNo.Contracts.Services;
using ZoNo.Messages;
using ZoNo.Services;

namespace ZoNo.ViewModels
{
  public partial class AccountPageViewModel : ObservableRecipient
  {
    private readonly ITopLevelNavigationService _topLevelNavigationService;
    private readonly ISplitwiseService _splitwiseService;
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
    private CurrencyCode? _defaultCurrency;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private Group[] _groups = null;

    public AccountPageViewModel(ITopLevelNavigationService topLevelNavigationService, ISplitwiseService splitwiseService, IMessenger messenger) : base(messenger)
    {
      _topLevelNavigationService = topLevelNavigationService;
      _splitwiseService = splitwiseService;
    }

    public async Task Load()
    {
      if (_isLoaded) return;
      IsLoading = true;

      Groups = await _splitwiseService.GetGroupsAsync();
      var user = await _splitwiseService.GetCurrentUserAsync();
      ProfilePicture = user.Picture.Medium;
      FirstName = user.FirstName;
      LastName = user.LastName;
      Email = user.Email;
      DefaultCurrency = user.DefaultCurrency;
      _isLoaded = true;

      IsLoading = false;
    }

    [RelayCommand]
    private void Logout()
    {
      Messenger.Send(new UserLoggedOutMessage());
      _topLevelNavigationService.NavigateTo(typeof(LoginPageViewModel).FullName!, infoOverride: new DrillInNavigationTransitionInfo());
    }
  }
}
