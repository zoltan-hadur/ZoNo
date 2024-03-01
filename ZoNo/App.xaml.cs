using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Splitwise;
using Splitwise.Contracts;
using Tracer;
using Tracer.Contracts;
using Tracer.Sinks;
using Windows.Storage.Pickers;
using WinRT.Interop;
using ZoNo.Contracts.Services;
using ZoNo.Messages;
using ZoNo.Services;
using ZoNo.ViewModels;
using ZoNo.Views;

namespace ZoNo
{
  public partial class App : Application,
    IRecipient<UserLoggedInMessage>,
    IRecipient<UserLoggedOutMessage>,
    IRecipient<ActivateMainWindowMessage>
  {
    private new static App Current => (App)Application.Current;
    private IServiceScope ServiceScope { get; set; }
    private ServiceProvider _serviceProvider;
    private readonly AutoResetEvent _windowClosed = new(false);

    public App()
    {
      // Fill up tracer method database
      TraceDatabaseFiller.Fill();

      // By default, ExcelDataReader throws a NotSupportedException "No data is available for encoding 1252." on .NET Core.
      // This fixes that.
      System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

      InitializeComponent();

      var services = new ServiceCollection();

      // Services
      services.AddHttpClient();
      services.AddSingleton<ISplitwiseConsumerCredentials>(new SplitwiseConsumerCredentials()
      {
        ConsumerKey = Environment.GetEnvironmentVariable("ZoNo_ConsumerKey"),
        ConsumerSecret = Environment.GetEnvironmentVariable("ZoNo_ConsumerSecret")
      });
      services.AddSingleton<ISplitwiseAuthorizationService, SplitwiseAuthorizationService>();
      services.AddScoped<ISplitwiseService, SplitwiseService>();
      services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
      services.AddSingleton<IEncryptionService, EncryptionService>();
      services.AddSingleton<ILocalSettingsService, LocalSettingsService>();
      services.AddSingleton<ITokenService, TokenService>();
      services.AddSingleton<IThemeSelectorService, ThemeSelectorService>();
      services.AddScoped<INavigationViewService, NavigationViewService>();
      services.AddSingleton<IActivationService, ActivationService>();
      services.AddSingleton<IPageService, PageService>();
      services.AddScoped<INavigationService, NavigationService>();
      services.AddSingleton<IExcelDocumentLoaderService, ExcelDocumentLoaderService>();
      services.AddSingleton<ITransactionProcessorService, TransactionProcessorService>();
      services.AddSingleton<IRulesService, RulesService>();
      services.AddSingleton<IRuleEvaluatorServiceBuilder, RuleEvaluatorServiceBuilder>();
      services.AddSingleton<IDialogService, DialogService>();
      services.AddSingleton<IRuleExpressionSyntaxCheckerService, RuleExpressionSyntaxCheckerService>();
      services.AddSingleton<ITraceDetailProcessor, TraceDetailProcessor>();
      services.AddSingleton<ITraceFactory, TraceFactory>();
      services.AddSingleton<ITraceDetailFactory, TraceDetailFactory>();
      services.AddSingleton<ITraceSink, InMemoryTraceSink>();
      services.AddSingleton<ITraceSink, FileTraceSink>();
      services.AddSingleton<INotificationService, NotificationService>();
      services.AddSingleton<IUpdateService, UpdateService>();

      // Views and ViewModels
      services.AddScoped<LoginPageViewModel>();
      services.AddScoped<ImportPageViewModel>();
      services.AddScoped<TransactionsViewModel>();
      services.AddScoped<ExpensesViewModel>();
      services.AddTransient<RulesViewModel>();  // Transient so a new instance can be created for RulesPageViewModel
      services.AddScoped<RulesPageViewModel>();
      services.AddScoped<QueryPageViewModel>();
      services.AddScoped<AccountPageViewModel>();
      services.AddSingleton<SettingsPageViewModel>();
      services.AddTransient<TracesViewModel>();
      services.AddScoped<ShellPageViewModel>();
      services.AddSingleton<MainWindow>();

      _serviceProvider = services.BuildServiceProvider();
      ServiceScope = _serviceProvider.CreateScope();

      UnhandledException += App_UnhandledException;

      GetService<IMessenger>().RegisterAll(this);
    }

    public static T GetService<T>() where T : class
    {
      if (Current.ServiceScope.ServiceProvider.GetService<T>() is not T service)
      {
        throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
      }
      return service;
    }

    public static IEnumerable<T> GetServices<T>() where T : class
    {
      if (Current.ServiceScope.ServiceProvider.GetServices<T>() is not IEnumerable<T> services)
      {
        throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
      }
      return services;
    }

    public void Receive(UserLoggedInMessage message)
    {
      using var trace = GetService<ITraceFactory>().CreateNew();
      NavigateToShellPage();
    }

    public void Receive(UserLoggedOutMessage message)
    {
      using var trace = GetService<ITraceFactory>().CreateNew();
      ReplaceServiceScope();
      NavigateToLoginPage();
    }

    public void Receive(ActivateMainWindowMessage message)
    {
      using var trace = GetService<ITraceFactory>().CreateNew();
      GetService<MainWindow>().Activate();
      NavigateToLoginPage();
    }

    private void NavigateToLoginPage()
    {
      using var trace = GetService<ITraceFactory>().CreateNew();
      GetService<MainWindow>().Frame.Navigate(typeof(LoginPage), parameter: null, infoOverride: new DrillInNavigationTransitionInfo());
    }

    private void NavigateToShellPage()
    {
      using var trace = GetService<ITraceFactory>().CreateNew();
      GetService<MainWindow>().Frame.Navigate(typeof(ShellPage), parameter: null, infoOverride: new DrillInNavigationTransitionInfo());
    }

    private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
      using var trace = GetService<ITraceFactory>().CreateNew();
      trace.Fatal(e.Exception.ToString());

      e.Handled = true;
      var path = string.Empty;
      var richEditBox = new RichEditBox()
      {
        FontFamily = new FontFamily("Courier New"),
        TextWrapping = TextWrapping.NoWrap,
        Padding = new Thickness(0, 0, 12, 12)
      };
      richEditBox.Document.SetText(TextSetOptions.None, e.Exception.ToString());
      richEditBox.IsReadOnly = true;
      var result = await GetService<IDialogService>().ShowDialogAsync(DialogType.SaveClose, "Unhandled Exception", new StackPanel()
      {
        Spacing = 6,
        Children =
        {
          richEditBox,
          new TextBlock() { Text = "You can save the traces to a file. The app will exit after the dialog closes." }
        }
      }, shouldCloseDialogOnPrimaryButtonClick: async () =>
      {
        var shouldCloseDialog = false;

        var savePicker = new FileSavePicker();
        var hWnd = WindowNative.GetWindowHandle(GetService<MainWindow>());
        InitializeWithWindow.Initialize(savePicker, hWnd);
        savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        savePicker.FileTypeChoices.Add("TRACE", [".trace"]);
        savePicker.SuggestedFileName = "ZoNo.trace";
        if (await savePicker.PickSaveFileAsync() is var file && file is not null)
        {
          path = file.Path;
          shouldCloseDialog = true;
        }

        return shouldCloseDialog;
      });

      if (result)
      {
        var inMemoryTraceSink = GetServices<ITraceSink>().Single(traceSink => traceSink is InMemoryTraceSink) as InMemoryTraceSink;
        File.WriteAllText(path, string.Join(Environment.NewLine, inMemoryTraceSink.Traces));
      }

      Exit();
    }

    private void ReplaceServiceScope()
    {
      using var trace = GetService<ITraceFactory>().CreateNew();
      var newScope = ServiceScope.ServiceProvider.CreateScope();
      ServiceScope.Dispose();
      ServiceScope = newScope;
    }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
      using var trace = GetService<ITraceFactory>().CreateNew();
      base.OnLaunched(args);

      new Thread(async () =>
      {
        _windowClosed.WaitOne();
        await Task.Delay(1000);
        _serviceProvider.Dispose();
        _serviceProvider = null;
      }).Start();

      GetService<MainWindow>().Closed += (s, e) =>
      {
        _windowClosed.Set();
      };

      await GetService<IActivationService>().ActivateAsync(args);
    }
  }
}