using CommunityToolkit.WinUI.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZoNo.Views.Import;

namespace ZoNo.Views.Account
{
  public sealed partial class UserView : UserControl
  {
    public static readonly DependencyProperty PictureProperty = DependencyProperty.Register(nameof(Picture), typeof(string), typeof(UserView), new PropertyMetadata("invalid"));
    public static readonly DependencyProperty FirstNameProperty = DependencyProperty.Register(nameof(FirstName), typeof(string), typeof(UserView), null);
    public static readonly DependencyProperty LastNameProperty = DependencyProperty.Register(nameof(LastName), typeof(string), typeof(UserView), null);
    public static readonly DependencyProperty EmailProperty = DependencyProperty.Register(nameof(Email), typeof(string), typeof(UserView), null);
    public static readonly DependencyProperty IsTextSelectionEnabledProperty = DependencyProperty.Register(nameof(IsTextSelectionEnabled), typeof(bool), typeof(UserView), new PropertyMetadata(false));

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

    public bool IsTextSelectionEnabled
    {
      get => (bool)GetValue(IsTextSelectionEnabledProperty);
      set => SetValue(IsTextSelectionEnabledProperty, value);
    }

    public UserView()
    {
      InitializeComponent();
    }

    private void Column_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (sender is FrameworkElement frameworkElement &&
          frameworkElement.GetValue(Grid.ColumnProperty) is int column &&
          frameworkElement.FindAscendants().FirstOrDefault(ascendant => ascendant.GetValue(Helpers.Grid.IsSharedSizeScopeProperty) is true) is DependencyObject ancestor &&
          ancestor.GetValue(Helpers.Grid.SharedSizeScopeProperty) is Dictionary<int, double> sharedSizeScope)
      {
        var maxWidth = sharedSizeScope.ContainsKey(column) ? sharedSizeScope[column] : 0;
        if (frameworkElement.DesiredSize.Width > maxWidth)
        {
          sharedSizeScope[column] = maxWidth = frameworkElement.DesiredSize.Width;
          foreach (UserView user in ancestor.FindDescendants().Where(descendant => descendant is UserView))
          {
            user.Grid.ColumnDefinitions[column].Width = new GridLength(maxWidth, GridUnitType.Pixel);
          }
        }
        else
        {
          Grid.ColumnDefinitions[column].Width = new GridLength(maxWidth, GridUnitType.Pixel);
        }
      }
    }
  }
}
