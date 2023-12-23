using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System.Diagnostics;
using Windows.UI;

namespace ZoNo
{
  public sealed partial class MainWindow : WindowEx
  {
    public Frame Frame => NavigationFrame;

    public MainWindow()
    {
      InitializeComponent();

      AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/ZoNo.ico"));

      ExtendsContentIntoTitleBar = true;
      SetTitleBar(AppTitleBar);

      Activated += MainWindow_Activated;
      NavigationFrame.ActualThemeChanged += NavigationFrame_ActualThemeChanged;
      NavigationFrame_ActualThemeChanged(null, null);
    }

    private void NavigationFrame_ActualThemeChanged(FrameworkElement sender, object args)
    {
      if (AppWindowTitleBar.IsCustomizationSupported())
      {
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
            AppWindow.TitleBar.ButtonInactiveForegroundColor = Color.FromArgb(255, r, g, b);
            await Task.Delay(1);
          }
          stopwatch.Stop();
          AppWindow.TitleBar.ButtonInactiveForegroundColor = NavigationFrame.ActualTheme == ElementTheme.Light ? Colors.LightSlateGray : Colors.Gray;
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
            AppWindow.TitleBar.ButtonForegroundColor = Color.FromArgb(255, r, g, b);
            await Task.Delay(1);
          }
          stopwatch.Stop();
          AppWindow.TitleBar.ButtonForegroundColor = NavigationFrame.ActualTheme == ElementTheme.Light ? Colors.Black : Colors.White;
        }
      }
      catch
      {

      }
    }
  }
}