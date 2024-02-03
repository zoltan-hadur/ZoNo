using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System.Diagnostics;
using Tracer.Contracts;
using Windows.Foundation;
using Windows.UI;
using ZoNo.Contracts.Services;
using ZoNo.ViewModels;

namespace ZoNo
{
  public sealed partial class MainWindow : WindowEx
  {
    private readonly INotificationService _notificationService;
    private readonly ITraceFactory _traceFactory;

    private IReadOnlyCollection<NotificationViewModel> Notifications => _notificationService.Notifications;
    public Frame Frame => NavigationFrame;

    public MainWindow(
      INotificationService notificationService,
      ITraceFactory traceFactory)
    {
      using var trace = traceFactory.CreateNew();
      _notificationService = notificationService;
      _traceFactory = traceFactory;

      InitializeComponent();

      AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/ZoNo.ico"));

      AppTitleBar.Loaded += (s, e) => SetRegionsForCustomTitleBar();
      AppTitleBar.SizeChanged += (s, e) => SetRegionsForCustomTitleBar();
      NotificationsButton.SizeChanged += (s, e) => SetRegionsForCustomTitleBar();
      NotificationsButton.LayoutUpdated += (s, e) => SetRegionsForCustomTitleBar();
      ExtendsContentIntoTitleBar = true;

      Activated += MainWindow_Activated;
      NavigationFrame.ActualThemeChanged += NavigationFrame_ActualThemeChanged;
      NavigationFrame_ActualThemeChanged(null, null);
    }

    private void SetRegionsForCustomTitleBar()
    {
      if (App.IsClosed || !ExtendsContentIntoTitleBar) return;

      var scaleAdjustment = AppTitleBar.XamlRoot.RasterizationScale;

      RightPaddingColumn.Width = new GridLength(AppWindow.TitleBar.RightInset / scaleAdjustment);
      LeftPaddingColumn.Width = new GridLength(AppWindow.TitleBar.LeftInset / scaleAdjustment);

      var transform = NotificationsButton.TransformToVisual(null);
      var bounds = transform.TransformBounds(new Rect(0, 0, NotificationsButton.ActualWidth, NotificationsButton.ActualHeight));
      var rect = GetRect(bounds, scaleAdjustment);

      var nonClientInputSrc = InputNonClientPointerSource.GetForWindowId(AppWindow.Id);
      nonClientInputSrc.SetRegionRects(NonClientRegionKind.Passthrough, [rect]);
    }

    private static Windows.Graphics.RectInt32 GetRect(Rect bounds, double scale)
    {
      return new Windows.Graphics.RectInt32(
          _X: (int)Math.Round(bounds.X * scale),
          _Y: (int)Math.Round(bounds.Y * scale),
          _Width: (int)Math.Round(bounds.Width * scale),
          _Height: (int)Math.Round(bounds.Height * scale)
      );
    }

    private void NavigationFrame_ActualThemeChanged(FrameworkElement sender, object args)
    {
      using var trace = _traceFactory.CreateNew();
      if (AppWindowTitleBar.IsCustomizationSupported())
      {
        trace.Debug(Format([NavigationFrame.ActualTheme]));
        if (NavigationFrame.ActualTheme == ElementTheme.Light)
        {
          AppWindow.TitleBar.ButtonForegroundColor = Colors.Black;
          AppWindow.TitleBar.ButtonHoverForegroundColor = Colors.Black;
          AppWindow.TitleBar.ButtonHoverBackgroundColor = Colors.Gainsboro;
          AppWindow.TitleBar.ButtonPressedForegroundColor = Colors.Black;
          AppWindow.TitleBar.ButtonPressedBackgroundColor = Color.FromArgb(0xFF, 0xE8, 0xE8, 0xE8);

          AppWindow.TitleBar.ButtonInactiveForegroundColor = Colors.LightSlateGray;
        }
        else
        {
          AppWindow.TitleBar.ButtonForegroundColor = Colors.White;
          AppWindow.TitleBar.ButtonHoverForegroundColor = Colors.White;
          AppWindow.TitleBar.ButtonHoverBackgroundColor = Color.FromArgb(0xFF, 0x40, 0x40, 0x40);
          AppWindow.TitleBar.ButtonPressedForegroundColor = Colors.White;
          AppWindow.TitleBar.ButtonPressedBackgroundColor = Color.FromArgb(0xFF, 0x38, 0x38, 0x38);

          AppWindow.TitleBar.ButtonInactiveForegroundColor = Colors.Gray;
        }
      }
    }

    private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([args.WindowActivationState]));

      // Animate app icon
      CommunityToolkit.WinUI.UI.Animations.AnimationBuilder
        .Create().Opacity(to: args.WindowActivationState == WindowActivationState.Deactivated ? 0.5 : 1)
        .Start(TitleBarImage);

      // Animate app title
      var resourceKey = args.WindowActivationState == WindowActivationState.Deactivated ? "TitleFillColorDisabled" : "TitleFillColorPrimary";
      var storyboard = new Storyboard();
      var animation = new ColorAnimation()
      {
        To = ((SolidColorBrush)TitleBarTextBlock.Resources[resourceKey]).Color,
        Duration = TimeSpan.FromMilliseconds(250)
      };
      Storyboard.SetTarget(animation, TitleBarTextBlock);
      Storyboard.SetTargetProperty(animation, "(TextBlock.Foreground).(SolidColorBrush.Color)");
      storyboard.Children.Add(animation);
      storyboard.Completed += (s, e) =>
      {
        TitleBarTextBlock.Foreground = (SolidColorBrush)TitleBarTextBlock.Resources[resourceKey];
      };
      storyboard.Begin();

      // Animate caption buttons
      try
      {
        if (args.WindowActivationState == WindowActivationState.Deactivated)
        {
          var duration = 250.0;
          var stopwatch = Stopwatch.StartNew();
          while (stopwatch.ElapsedMilliseconds < duration)
          {
            var from = NavigationFrame.ActualTheme == ElementTheme.Light ? Colors.Black : Colors.White;
            var to = NavigationFrame.ActualTheme == ElementTheme.Light ? Colors.LightSlateGray : Colors.Gray;
            var r = Convert.ToByte(double.Lerp(from.R, to.R, Math.Min(stopwatch.ElapsedMilliseconds / duration, 1.0)));
            var g = Convert.ToByte(double.Lerp(from.G, to.G, Math.Min(stopwatch.ElapsedMilliseconds / duration, 1.0)));
            var b = Convert.ToByte(double.Lerp(from.B, to.B, Math.Min(stopwatch.ElapsedMilliseconds / duration, 1.0)));
            if (!App.IsClosed)
            {
              AppWindow.TitleBar.ButtonInactiveForegroundColor = Color.FromArgb(255, r, g, b);
            }
            else
            {
              break;
            }
            await Task.Delay(1);
          }
          stopwatch.Stop();
          if (!App.IsClosed)
          {
            AppWindow.TitleBar.ButtonInactiveForegroundColor = NavigationFrame.ActualTheme == ElementTheme.Light ? Colors.LightSlateGray : Colors.Gray;
          }
        }
        else
        {
          var duration = 250.0;
          var stopwatch = Stopwatch.StartNew();
          while (stopwatch.ElapsedMilliseconds < duration)
          {
            var from = NavigationFrame.ActualTheme == ElementTheme.Light ? Colors.LightSlateGray : Colors.Gray;
            var to = NavigationFrame.ActualTheme == ElementTheme.Light ? Colors.Black : Colors.White;
            var r = Convert.ToByte(double.Lerp(from.R, to.R, Math.Min(stopwatch.ElapsedMilliseconds / duration, 1.0)));
            var g = Convert.ToByte(double.Lerp(from.G, to.G, Math.Min(stopwatch.ElapsedMilliseconds / duration, 1.0)));
            var b = Convert.ToByte(double.Lerp(from.B, to.B, Math.Min(stopwatch.ElapsedMilliseconds / duration, 1.0)));
            if (!App.IsClosed)
            {
              AppWindow.TitleBar.ButtonForegroundColor = Color.FromArgb(255, r, g, b);
            }
            else
            {
              break;
            }
            await Task.Delay(1);
          }
          stopwatch.Stop();
          if (!App.IsClosed)
          {
            AppWindow.TitleBar.ButtonForegroundColor = NavigationFrame.ActualTheme == ElementTheme.Light ? Colors.Black : Colors.White;
          }
        }
      }
      catch
      {

      }
    }

    private void NotificationView_Click(object sender, RoutedEventArgs e)
    {
      using var trace = _traceFactory.CreateNew();
      if (sender is Button button &&
          button.DataContext is NotificationViewModel notificationViewModel)
      {
        _notificationService.RemoveNotification(notificationViewModel);
        NotificationsFlyout.Hide();
      }
    }
  }
}