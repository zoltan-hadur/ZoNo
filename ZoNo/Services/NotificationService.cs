using System.Collections.ObjectModel;
using Tracer.Contracts;
using ZoNo.Contracts.Services;
using ZoNo.ViewModels;

namespace ZoNo.Services
{
  public class NotificationService(
    ITraceFactory traceFactory) : INotificationService
  {
    private readonly ITraceFactory _traceFactory = traceFactory;

    private ObservableCollection<NotificationViewModel> _notifications = [];
    public ReadOnlyObservableCollection<NotificationViewModel> Notifications => new(_notifications);

    public void AddNotification(NotificationViewModel notificationViewModel)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([notificationViewModel.Title, notificationViewModel.Description]));
      _notifications.Add(notificationViewModel);
    }

    public bool RemoveNotification(NotificationViewModel notificationViewModel)
    {
      using var trace = _traceFactory.CreateNew();
      var result = _notifications.Remove(notificationViewModel);
      trace.Debug(Format([notificationViewModel.Title, notificationViewModel.Description, result]));
      return result;
    }
  }
}
