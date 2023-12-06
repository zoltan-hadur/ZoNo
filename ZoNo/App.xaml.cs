using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Splitwise;
using Splitwise.Contracts;
using ZoNo.Activation;
using ZoNo.Contracts.Services;
using ZoNo.Messages;
using ZoNo.Models;
using ZoNo.Services;
using ZoNo.ViewModels;
using ZoNo.Views;

namespace ZoNo
{
  public partial class App : Application
  {
    private new static App Current => (App)Application.Current;
    private IServiceScope ServiceScope { get; set; }

    public static MainWindow MainWindow { get; } = new MainWindow();

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
      services.AddHttpClient();
      services.AddSingleton<ISplitwiseAuthorizationService>(provider =>
      {
        return new SplitwiseAuthorizationService(
          httpClientFactory: provider.GetService<IHttpClientFactory>(),
          consumerKey: Environment.GetEnvironmentVariable("ZoNo_ConsumerKey"),
          consumerSecret: Environment.GetEnvironmentVariable("ZoNo_ConsumerSecret")
        );
      });
      services.AddScoped<ISplitwiseService>(provider => new SplitwiseService(provider.GetService<IHttpClientFactory>(), provider.GetService<ITokenService>().Token));
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
        return new NavigationService(provider.GetService<ITopLevelPageService>())
        {
          Frame = MainWindow.Frame
        };
      });
      services.AddScoped<INavigationService, NavigationService>();

      services.AddSingleton<IFileService, FileService>();
      services.AddSingleton<IExcelDocumentLoader, ExcelDocumentLoader>();
      services.AddSingleton<IRulesService, RulesService>();
      services.AddSingleton<IRuleEvaluatorServiceBuilder, RuleEvaluatorServiceBuilder>();
      services.AddSingleton<IDialogService, DialogService>();
      services.AddSingleton<IRuleExpressionSyntaxCheckerService, RuleExpressionSyntaxCheckerService>();

      // Views and ViewModels
      services.AddScoped<LoginPageViewModel>();
      services.AddScoped<ImportPageViewModel>();
      services.AddScoped<TransactionsViewModel>();
      services.AddScoped<ExpensesViewModel>();
      services.AddScoped<RulesPageViewModel>(provider => new RulesPageViewModel(
        new RulesViewModel(
          provider.GetService<IRuleExpressionSyntaxCheckerService>(),
          provider.GetService<IDialogService>(),
          provider.GetService<IRulesService>(),
          RuleType.Transaction),
        new RulesViewModel(
          provider.GetService<IRuleExpressionSyntaxCheckerService>(),
          provider.GetService<IDialogService>(),
          provider.GetService<IRulesService>(),
          RuleType.Expense))
      );
      services.AddScoped<QueryPageViewModel>();
      services.AddScoped<AccountPageViewModel>();
      services.AddScoped<SettingsPageViewModel>();
      services.AddScoped<ShellPageViewModel>();

      // Configuration
      services.Configure<LocalSettingsOptions>(configuration.GetSection(nameof(LocalSettingsOptions)));

      ServiceScope = services.BuildServiceProvider().CreateScope();

      UnhandledException += App_UnhandledException;

      ServiceScope.ServiceProvider.GetService<IMessenger>().Register<App, UserLoggedOutMessage>(this, OnUserLoggedOut);

      var themeSelectorService = ServiceScope.ServiceProvider.GetService<IThemeSelectorService>();
      Task.Run(themeSelectorService.InitializeAsync).Wait();
      if (themeSelectorService.Theme != ElementTheme.Default)
      {
        RequestedTheme = themeSelectorService.Theme == ElementTheme.Light ? ApplicationTheme.Light : ApplicationTheme.Dark;
      }
    }

    private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
      e.Handled = true;
      await GetService<IDialogService>().ShowDialogAsync(DialogType.Ok, "Unhandled Exception", new TextBlock()
      {
        Text = e.Exception.ToString(),
        IsTextSelectionEnabled = true
      });
      Exit();
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