using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using ZoNo.Models;

namespace ZoNo.Views
{
  public sealed partial class GroupView : UserControl
  {
    public static readonly DependencyProperty PictureProperty = DependencyProperty.Register(nameof(Picture), typeof(string), typeof(GroupView), new PropertyMetadata("invalid"));
    public static new readonly DependencyProperty NameProperty = DependencyProperty.Register(nameof(Name), typeof(string), typeof(GroupView), null);
    public static readonly DependencyProperty MembersProperty = DependencyProperty.Register(nameof(Members), typeof(IEnumerable<User>), typeof(GroupView), null);
    public static readonly DependencyProperty IsTextSelectionEnabledProperty = DependencyProperty.Register(nameof(IsTextSelectionEnabled), typeof(bool), typeof(GroupView), new PropertyMetadata(false));

    public string Picture
    {
      get => (string)GetValue(PictureProperty);
      set => SetValue(PictureProperty, value);
    }

    public new string Name
    {
      get => (string)GetValue(NameProperty);
      set => SetValue(NameProperty, value);
    }

    public IEnumerable<User> Members
    {
      get => (IEnumerable<User>)GetValue(MembersProperty);
      set => SetValue(MembersProperty, value);
    }

    public bool IsTextSelectionEnabled
    {
      get => (bool)GetValue(IsTextSelectionEnabledProperty);
      set => SetValue(IsTextSelectionEnabledProperty, value);
    }

    public GroupView()
    {
      InitializeComponent();
    }
  }
}
