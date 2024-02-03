using System.Collections.ObjectModel;
using ZoNo.Contracts.Services;
using ZoNo.ViewModels;

namespace ZoNo.Services
{
  public class NotificationService : INotificationService
  {
    private ObservableCollection<NotificationViewModel> _notifications = [];
    public ReadOnlyObservableCollection<NotificationViewModel> Notifications => new(_notifications);

    public void AddNotification(NotificationViewModel notificationViewModel)
    {
      _notifications.Add(notificationViewModel);
    }

    public bool RemoveNotification(NotificationViewModel notificationViewModel)
    {
      return _notifications.Remove(notificationViewModel);
    }
  }
}
