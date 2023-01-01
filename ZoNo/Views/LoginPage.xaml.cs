using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZoNo.ViewModels;

namespace ZoNo.Views
{
  public sealed partial class LoginPage : Page
  {
    public LoginViewModel ViewModel { get; }

    public LoginPage()
    {
      ViewModel = App.GetService<LoginViewModel>();
      InitializeComponent();
      ViewModel.WebView = WebView;
    }

    private async void Page_Loading(FrameworkElement sender, object args)
    {
      await ViewModel.Load();
      Email.Focus(FocusState.Programmatic);
      Email.SelectionStart = Email.Text.Length;
    }

    private void TextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
      if (e.Key == Windows.System.VirtualKey.Enter)
      {
        Login.Command.Execute(null);
      }
    }
  }
}