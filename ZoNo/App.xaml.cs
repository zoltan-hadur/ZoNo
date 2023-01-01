using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Splitwise;
using ZoNo.Activation;
using ZoNo.Contracts.Services;
using ZoNo.Messages;
using ZoNo.Models;
using ZoNo.Services;
using ZoNo.ViewModels;
using ZoNo.Views;

namespace ZoNo
{
  // To learn more about WinUI 3, see https://docs.microsoft.com/windows/apps/winui/winui3/.
  public partial class App : Application
  {
    private new static App Current => (App)Application.Current;
    private IServiceScope ServiceScope { get; set; }

    public static WindowEx MainWindow { get; } = new MainWindow();

    public static T GetService<T>() where T : class
    {
      if (Current.ServiceScope.ServiceProvider.GetService<T>() is not T service)
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
      services.AddSingleton<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

      // Other Activation Handlers

      // Services
      services.AddSingleton(new Authorization(consumerKey: Environment.GetEnvironmentVariable("ZoNo_ConsumerKey"), consumerSecret: Environment.GetEnvironmentVariable("ZoNo_ConsumerSecret")));
      services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
      services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
      services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
      services.AddScoped<INavigationViewService, NavigationViewService>();

      services.AddSingleton<IActivationService, ActivationService>();
      services.AddSingleton<ITopLevelPageService, PageService>(provider =>
      {
        return new PageService.Builder()
          .Configure<LoginViewModel, LoginPage>()
          .Configure<ShellViewModel, ShellPage>()
          .Build();
      });
      services.AddSingleton<IPageService, PageService>(provider =>
      {
        return new PageService.Builder()
          .Configure<ImportViewModel, ImportPage>()
          .Configure<QueryViewModel, QueryPage>()
          .Configure<AccountViewModel, AccountPage>()
          .Configure<SettingsViewModel, SettingsPage>()
          .Build();
      });
      services.AddSingleton<ITopLevelNavigationService, NavigationService>(provider =>
      {
        return new NavigationService(provider.GetService<ITopLevelPageService>()!)
        {
          Frame = MainWindow.Content as Frame
        };
      });
      services.AddScoped<INavigationService, NavigationService>();

      // Core Services
      services.AddSingleton<IFileService, FileService>();

      // Views and ViewModels
      services.AddScoped<LoginViewModel>();
      services.AddScoped<ImportViewModel>();
      services.AddScoped<QueryViewModel>();
      services.AddScoped<AccountViewModel>();
      services.AddScoped<SettingsViewModel>();
      services.AddScoped<ShellViewModel>();

      // Configuration
      services.Configure<LocalSettingsOptions>(configuration.GetSection(nameof(LocalSettingsOptions)));

      ServiceScope = services.BuildServiceProvider().CreateScope();

      UnhandledException += App_UnhandledException;

      ServiceScope.ServiceProvider.GetService<IMessenger>()!.Register<App, UserLoggedOutMessage>(this, OnUserLoggedOut);
    }

    private void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
      // TODO: Log and handle exceptions as appropriate.
      // https://docs.microsoft.com/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.application.unhandledexception.
    }

    private void OnUserLoggedOut(App recipient, UserLoggedOutMessage message)
    {
      ReplaceServiceScope();
    }

    private void ReplaceServiceScope()
    {
      var newScope = ServiceScope.ServiceProvider.CreateScope();
      ServiceScope?.Dispose();
      ServiceScope = newScope;
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
      base.OnLaunched(args);

      await App.GetService<IActivationService>().ActivateAsync(args);
    }
  }
}