using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO.Abstractions;
using System.Windows;
using ZoNo.ViewModels;
using ZoNo.Views;

namespace ZoNo
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    /// <summary>
    /// Gets the current <see cref="App"/> instance in use.
    /// </summary>
    public new static App Current => (App)Application.Current;

    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider Services { get; }

    public App()
    {
      Services = new ServiceCollection()
        .AddSingleton<IMessenger>(WeakReferenceMessenger.Default)
        .AddSingleton<IFileSystem, FileSystem>()
        .AddSingleton<ISettings, Settings>(factory => new Settings(factory.GetService<IFileSystem>(), "settings"))
        .AddSingleton<MainWindowVM>()
        .AddSingleton<LoginVM>()
        .AddSingleton<HomeVM>()
        .AddSingleton<MainWindow>()
        .BuildServiceProvider();

      InitializeComponent();
    }

    private void Application_Startup(object sender, StartupEventArgs e)
    {
      Services.GetService<MainWindow>().Show();
    }
  }
}
