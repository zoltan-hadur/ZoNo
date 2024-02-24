using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml.Media.Animation;
using Splitwise.Contracts;
using Tracer;
using Tracer.Contracts;
using ZoNo.Contracts.Services;
using ZoNo.Messages;
using ZoNo.Models;

namespace ZoNo.ViewModels
{
  public partial class AccountPageViewModel(
    ITopLevelNavigationService topLevelNavigationService,
    ISplitwiseService splitwiseService,
    ITraceFactory traceFactory,
    IMessenger messenger) : ObservableRecipient(messenger)
  {
    private readonly ITopLevelNavigationService _topLevelNavigationService = topLevelNavigationService;
    private readonly ISplitwiseService _splitwiseService = splitwiseService;
    private readonly ITraceFactory _traceFactory = traceFactory;

    private bool _isLoaded = false;

    [ObservableProperty]
    private string _profilePicture = "invalid";

    [ObservableProperty]
    private string _firstName;

    [ObservableProperty]
    private string _lastName;

    [ObservableProperty]
    private string _email;

    [ObservableProperty]
    private Currency _defaultCurrency;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private Group[] _groups = [];

    public async Task LoadAsync()
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([_isLoaded]));
      if (_isLoaded) return;
      IsLoading = true;

      var taskUser = TraceFactory.HandleAsAsyncVoid(_splitwiseService.GetCurrentUserAsync);
      var taskGroups = TraceFactory.HandleAsAsyncVoid(_splitwiseService.GetGroupsAsync);
      await Task.WhenAll([taskUser, taskGroups]);
      var user = taskUser.Result;
      var groups = taskGroups.Result;

      ProfilePicture = user.Picture.Medium;
      FirstName = user.FirstName;
      LastName = user.LastName;
      Email = user.Email;
      DefaultCurrency = Enum.Parse<Currency>(user.DefaultCurrency.ToString());

      Groups = groups.Select(group => new Group()
      {
        Picture = group.Avatar.Medium,
        Name = group.Name,
        Members = group.Members.Select(user => new User()
        {
          Picture = user.Picture.Medium,
          FirstName = user.FirstName,
          LastName = user.LastName,
          Email = user.Email
        }).ToArray()
      }).ToArray();
      _isLoaded = true;

      IsLoading = false;
    }

    [RelayCommand]
    private void Logout()
    {
      using var trace = _traceFactory.CreateNew();
      Messenger.Send(new UserLoggedOutMessage());
      _topLevelNavigationService.NavigateTo(typeof(LoginPageViewModel).FullName, infoOverride: new DrillInNavigationTransitionInfo());
    }
  }
}
