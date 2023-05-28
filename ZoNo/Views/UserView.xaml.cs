using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Splitwise.Models;

namespace ZoNo.Views
{
  public sealed partial class UserView : UserControl
  {
    public static readonly DependencyProperty UserProperty = DependencyProperty.Register(nameof(User), typeof(string), typeof(UserView), null);

    public User User
    {
      get => (User)GetValue(UserProperty);
      set => SetValue(UserProperty, value);
    }

    public UserView()
    {
      InitializeComponent();
    }
  }
}
