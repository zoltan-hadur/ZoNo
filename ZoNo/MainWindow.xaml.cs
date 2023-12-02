using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Windows.UI;

namespace ZoNo
{
  public sealed partial class MainWindow : WindowEx
  {
    public Frame Frame => NavigationFrame;

    public MainWindow()
    {
      InitializeComponent();

      AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));

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

    private void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
    {
      var resourceKey = args.WindowActivationState == WindowActivationState.Deactivated ? "TitleFillColorDisabled" : "TitleFillColorPrimary";
      var storyboard = new Storyboard();
      var animation = new ColorAnimation()
      {
        To = ((SolidColorBrush)TitleBarTextBlock.Resources[resourceKey]).Color,
        Duration = TimeSpan.FromMilliseconds(200)
      };
      Storyboard.SetTarget(animation, TitleBarTextBlock);
      Storyboard.SetTargetProperty(animation, "(TextBlock.Foreground).(SolidColorBrush.Color)");
      storyboard.Children.Add(animation);
      storyboard.Completed += (s, e) =>
      {
        TitleBarTextBlock.Foreground = (SolidColorBrush)TitleBarTextBlock.Resources[resourceKey];
      };
      storyboard.Begin();
    }
  }
}