﻿using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Splitwise;
using Splitwise.Contracts;
using Tracer;
using Tracer.Contracts;
using Tracer.Sinks;
using Windows.Storage.Pickers;
using WinRT.Interop;
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

    public static IEnumerable<T> GetServices<T>() where T : class
    {
      if (Current.ServiceScope.ServiceProvider.GetServices<T>() is not IEnumerable<T> services)
      {
        throw new ArgumentException($"{typeof(T)} needs to be registered in ConfigureServices within App.xaml.cs.");
      }
      return services;
    }

    public App()
    {
      // Fill up tracer method database
      TraceMethodDatabaseFiller.Fill();

      // By default, ExcelDataReader throws a NotSupportedException "No data is available for encoding 1252." on .NET Core.
      // This fixes that.
      System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

      InitializeComponent();

      var services = new ServiceCollection();

      // Default Activation Handler
      services.AddSingleton<ActivationHandler<LaunchActivatedEventArgs>, DefaultActivationHandler>();

      // Other Activation Handlers

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

      ServiceScope = services.BuildServiceProvider().CreateScope();

      UnhandledException += App_UnhandledException;

      GetService<IMessenger>().Register<App, UserLoggedOutMessage>(this, OnUserLoggedOut);

      // Must set the app theme in the constructor, otherwise an exception is thrown
      var themeSelectorService = GetService<IThemeSelectorService>();
      Task.Run(themeSelectorService.InitializeAsync).Wait();
      if (themeSelectorService.Theme != ElementTheme.Default)
      {
        RequestedTheme = themeSelectorService.Theme == ElementTheme.Light ? ApplicationTheme.Light : ApplicationTheme.Dark;
      }

      // Must set tracing settings
      var settingsPageViewModel = GetService<SettingsPageViewModel>();
      Task.Run(settingsPageViewModel.LoadAsync).Wait();
    }

    private async void App_UnhandledException(object sender, Microsoft.UI.Xaml.UnhandledExceptionEventArgs e)
    {
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
        var hWnd = WindowNative.GetWindowHandle(App.MainWindow);
        InitializeWithWindow.Initialize(savePicker, hWnd);
        savePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
        savePicker.FileTypeChoices.Add("TRACE", new List<string>() { ".trace" });
        savePicker.SuggestedFileName = "ZoNo.trace";
        if (await savePicker.PickSaveFileAsync() is var file && file != null)
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