using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ZoNo.Views.Account
{
  public sealed partial class UserView : UserControl
  {
    public static readonly DependencyProperty PictureProperty = DependencyProperty.Register(nameof(Picture), typeof(string), typeof(UserView), null);
    public static readonly DependencyProperty FirstNameProperty = DependencyProperty.Register(nameof(FirstName), typeof(string), typeof(UserView), null);
    public static readonly DependencyProperty LastNameProperty = DependencyProperty.Register(nameof(LastName), typeof(string), typeof(UserView), null);
    public static readonly DependencyProperty EmailProperty = DependencyProperty.Register(nameof(Email), typeof(string), typeof(UserView), null);

    public string Picture
    {
      get => (string)GetValue(PictureProperty);
      set => SetValue(PictureProperty, value);
    }

    public string FirstName
    {
      get => (string)GetValue(FirstNameProperty);
      set => SetValue(FirstNameProperty, value);
    }

    public string LastName
    {
      get => (string)GetValue(LastNameProperty);
      set => SetValue(LastNameProperty, value);
    }

    public string Email
    {
      get => (string)GetValue(EmailProperty);
      set => SetValue(EmailProperty, value);
    }

    public UserView()
    {
      InitializeComponent();
    }
  }
}
