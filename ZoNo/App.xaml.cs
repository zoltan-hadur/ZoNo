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
using ZoNo.ViewModels.Import;
using ZoNo.ViewModels.Rules;
using ZoNo.Views;
using ZoNo.Views.Import;
using ZoNo.Views.Rules;

namespace ZoNo
{
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
      // By default, ExcelDataReader throws a NotSupportedException "No data is available for encoding 1252." on .NET Core.
      // This fixes that.
      System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

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
      services.AddSingleton<ITokenService, TokenService>();
      services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
      services.AddScoped<INavigationViewService, NavigationViewService>();

      services.AddSingleton<IActivationService, ActivationService>();
      services.AddSingleton<ITopLevelPageService, PageService>(provider =>
      {
        return new PageService.Builder()
          .Configure<LoginPageViewModel, LoginPage>()
          .Configure<ShellPageViewModel, ShellPage>()
          .Build();
      });
      services.AddSingleton<IPageService, PageService>(provider =>
      {
        return new PageService.Builder()
          .Configure<ImportPageViewModel, ImportPage>()
          .Configure<RulesPageViewModel, RulesPage>()
          .Configure<QueryPageViewModel, QueryPage>()
          .Configure<AccountPageViewModel, AccountPage>()
          .Configure<SettingsPageViewModel, SettingsPage>()
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

      services.AddSingleton<IFileService, FileService>();
      services.AddSingleton<IExcelLoader, ExcelLoader>();
      services.AddSingleton<IRulesService, RulesService>();
      services.AddSingleton<IRuleEvaluatorService, RuleEvaluatorService>();

      // Views and ViewModels
      services.AddScoped<LoginPageViewModel>();
      services.AddScoped<ImportPageViewModel>();
      services.AddScoped<TransactionsViewModel>();
      services.AddScoped<RulesPageViewModel>(provider => new RulesPageViewModel(
        new RulesViewModel(provider.GetService<IRulesService>()!, RuleType.Import),
        new RulesViewModel(provider.GetService<IRulesService>()!, RuleType.Splitwise))
      );
      services.AddScoped<QueryPageViewModel>();
      services.AddScoped<AccountPageViewModel>();
      services.AddScoped<SettingsPageViewModel>();
      services.AddScoped<ShellPageViewModel>();

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

    private async void OnUserLoggedOut(App recipient, UserLoggedOutMessage message)
    {
      ReplaceServiceScope();
      await App.GetService<ITokenService>().SetTokenAsync(null);
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