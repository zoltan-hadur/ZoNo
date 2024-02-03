using System.Collections.ObjectModel;
using ZoNo.ViewModels;

namespace ZoNo.Contracts.Services
{
  public interface INotificationService
  {
    ReadOnlyObservableCollection<NotificationViewModel> Notifications { get; }

    void AddNotification(NotificationViewModel notificationViewModel);
    bool RemoveNotification(NotificationViewModel notificationViewModel);
  }
}
