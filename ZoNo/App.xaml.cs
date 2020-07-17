using CefSharp;
using CefSharp.Wpf;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Unity;
using ZoNo.Contracts;
using ZoNo.Contracts.ViewModels;
using ZoNo.Contracts.Views;
using ZoNo.ViewModels;
using ZoNo.Views;

namespace ZoNo
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    public App()
    {
      // Setup cache (cookies and etc) of the browser next to the exe
      Cef.Initialize(new CefSettings()
      {
        CachePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Cache")
      });
    }

    protected override void OnStartup(StartupEventArgs e)
    {
      base.OnStartup(e);

      var wContainer = new UnityContainer();

      // Register views
      wContainer.RegisterType     <IMainWindowV    , MainWindowV    >();

      // Register view models
      wContainer.RegisterType     <IMainWindowVM   , MainWindowVM   >();
      wContainer.RegisterType     <ILoginVM        , LoginVM        >();
      wContainer.RegisterType     <IHomeVM         , HomeVM         >();

      // Register singleton resources
      wContainer.RegisterSingleton<IFileSystem     , FileSystem     >();
      wContainer.RegisterSingleton<IEventAggregator, EventAggregator>();
      wContainer.RegisterSingleton<ISettings       , Settings       >();

      // Load settings and set a timer to save it every 5 seconds
      var wSettings = wContainer.Resolve<ISettings>();
      wSettings.Load("settings");
      var wSettingsSaver = new DispatcherTimer()
      {
        Interval = TimeSpan.FromSeconds(5),
        IsEnabled = true
      };
      wSettingsSaver.Tick += OnTick;
      void OnTick(object sender, EventArgs e)
      {
        wSettings.Save("settings");
      }
      wSettingsSaver.Start();

      // Unsubscribe from the events when the application exits
      Exit += OnExit;
      void OnExit(object sender, ExitEventArgs e)
      {
        wSettingsSaver.Stop();
        wSettingsSaver.Tick -= OnTick;
        wSettings.Save("settings");
        Exit -= OnExit;
      }

      // Display the main window
      wContainer.Resolve<MainWindowV>().Show();
    }
  }
}
