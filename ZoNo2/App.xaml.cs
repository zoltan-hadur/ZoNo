using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;

using ZoNo2.Activation;
using ZoNo2.Contracts.Services;
using ZoNo2.Models;
using ZoNo2.Services;
using ZoNo2.ViewModels;
using ZoNo2.Views;

namespace ZoNo2;

// To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
public partial class App : Application
{
  private new static App Current => (App)Application.Current;
  private IServiceProvider Services { get; set; }

  public static WindowEx MainWindow { get; } = new MainWindow();

  public static T GetService<T>() where T : class
  {
    // TODO create new scope upon logging off
    if (typeof(T) == typeof(SettingsViewModel))
    {
      Current.Services = Current.Services.CreateScope().ServiceProvider;
    }
    if (Current.Services.GetService<T>() is not T service)
    {
      throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
    }
    return service;
  }

  public App()
  {
    InitializeComponent();

    var configuration = new ConfigurationBuilder()
      .SetBasePath(AppContext.BaseDirectory)
      .AddJsonFile("appsettings.json")
      .Build();

    var services = new ServiceCollection();

    // Default Activation Handler
    services.AddTransient<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

    // Other Activation Handlers

    // Services
    services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
    services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
    services.AddSingleton<INavigationViewService, NavigationViewService>();

    services.AddSingleton<IActivationService, ActivationService>();
    services.AddSingleton<IPageService, PageService>();
    services.AddSingleton<INavigationService, NavigationService>();

    // Core Services
    services.AddSingleton<IFileService, FileService>();

    // Views and ViewModels
    services.AddScoped<ImportViewModel>();
    //services.AddScoped<ImportPage>();
    services.AddScoped<QueryViewModel>();
    //services.AddScoped<QueryPage>();
    services.AddScoped<SettingsViewModel>();
    //services.AddTransient<SettingsPage>();
    services.AddTransient<ShellViewModel>();
    services.AddTransient<ShellPage>();

    // Configuration
    services.Configure<LocalSettingsOptions>(configuration.GetSection(nameof(LocalSettingsOptions)));

    Services = services.BuildServiceProvider();

    UnhandledException += App_UnhandledException;
  }

  private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
  {
    // TODO: Log and handle exceptions as appropriate.
    // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
  }

  protected async override void OnLaunched(LaunchActivatedEventArgs args)
  {
    base.OnLaunched(args);

    await App.GetService<IActivationService>().ActivateAsync(args);
  }
}
