using System.Diagnostics;
using System.Xml.Linq;
using Tracer.Contracts;
using Windows.ApplicationModel;
using ZoNo.Contracts.Services;
using ZoNo.ViewModels;

namespace ZoNo.Services
{
  public class UpdateService(
    INotificationService notificationService,
    ITraceFactory traceFactory
    ) : IUpdateService
  {
    private readonly INotificationService _notificationService = notificationService;
    private readonly ITraceFactory _traceFactory = traceFactory;

    public async Task CheckForUpdateAsync()
    {
      using var trace = _traceFactory.CreateNew();
      PackageUpdateAvailabilityResult result = await Package.Current.CheckUpdateAvailabilityAsync();
      trace.Debug(Format([result.Availability]));
      switch (result.Availability)
      {
        case PackageUpdateAvailability.Available:
        case PackageUpdateAvailability.Required:
          {
            var uri = Package.Current.GetAppInstallerInfo().Uri;
            trace.Debug(Format([uri.LocalPath]));
            var version = "UNKNOWN";
            try
            {
              using var stream = File.OpenRead(uri.LocalPath);
              var appinstaller = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None);
              version = appinstaller.Root.Attribute("Version").Value;
            }
            catch (Exception e)
            {
              trace.Warn(e.ToString());
            }
            trace.Debug(Format([version]));
            _notificationService.AddNotification(new NotificationViewModel()
            {
              Title = "New version available.",
              Description = $"Click to update ZoNo to version {version}.",
              Action = async () =>
              {
                await UpdateAsync();
              }
            });
            break;
          }
        default:
          break;
      }
    }

    public Task UpdateAsync()
    {
      using var trace = _traceFactory.CreateNew();
      try
      {
        var uri = Package.Current.GetAppInstallerInfo().Uri;
        var ps = new ProcessStartInfo(uri.ToString())
        {
          UseShellExecute = true
        };
        Process.Start(ps);
      }
      catch (Exception e)
      {
        trace.Error(e.ToString());
      }

      return Task.CompletedTask;
    }
  }
}
