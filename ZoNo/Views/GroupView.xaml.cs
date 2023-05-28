using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Splitwise.Models;

namespace ZoNo.Views
{
  public sealed partial class GroupView : UserControl
  {
    public static readonly DependencyProperty GroupProperty = DependencyProperty.Register(nameof(Group), typeof(string), typeof(GroupView), null);

    public Group Group
    {
      get => (Group)GetValue(GroupProperty);
      set => SetValue(GroupProperty, value);
    }

    public GroupView()
    {
      InitializeComponent();
    }
  }
}
