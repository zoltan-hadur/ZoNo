using ZoNo.Helpers;

namespace ZoNo
{
  public sealed partial class MainWindow : WindowEx
  {
    public MainWindow()
    {
      InitializeComponent();

      AppWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets/WindowIcon.ico"));
      Title = "AppDisplayName".GetLocalized();
    }
  }
}