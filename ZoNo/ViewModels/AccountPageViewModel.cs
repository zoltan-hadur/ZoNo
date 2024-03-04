using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Tracer.Contracts;
using ZoNo.Contracts.Services;
using ZoNo.Messages;
using ZoNo.Models;

namespace ZoNo.ViewModels
{
  public partial class AccountPageViewModel(
    ISplitwiseCacheService _splitwiseCacheService,
    ITraceFactory _traceFactory,
    IMessenger _messenger) : ObservableObject
  {
    public User User => _splitwiseCacheService.ZoNoUser;
    public IReadOnlyCollection<Group> Groups => _splitwiseCacheService.ZoNoGroups;

    [RelayCommand]
    private void Logout()
    {
      using var trace = _traceFactory.CreateNew();
      _messenger.Send<UserLoggedOutMessage>();
    }
  }
}
