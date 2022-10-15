using ZoNo2.Helpers;

namespace ZoNo2;

public sealed partial class MainWindow : WindowEx
{
  public MainWindow()
  {
    InitializeComponent();

    AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
    Content = null;
    Title = "AppDisplayName".GetLocalized();
  }
}
