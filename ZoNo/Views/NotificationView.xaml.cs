using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ZoNo.Views
{
  public sealed partial class NotificationView : UserControl
  {
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(NotificationView), null);
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(nameof(Description), typeof(string), typeof(NotificationView), null);

    public string Title
    {
      get => (string)GetValue(TitleProperty);
      set => SetValue(TitleProperty, value);
    }

    public string Description
    {
      get => (string)GetValue(DescriptionProperty);
      set => SetValue(DescriptionProperty, value);
    }

    public NotificationView()
    {
      InitializeComponent();
    }
  }
}
