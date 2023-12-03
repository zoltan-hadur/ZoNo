using CommunityToolkit.WinUI.UI.Animations;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Markup;

namespace ZoNo.Controls
{
  [ContentProperty(Name = nameof(Content))]
  public class LoaderContentControl : ContentControl
  {
    public static new readonly DependencyProperty ContentProperty = DependencyProperty.Register(nameof(Content), typeof(object), typeof(LoaderContentControl), new PropertyMetadata(null, OnContentChanged));
    public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(LoaderContentControl), new PropertyMetadata(null, OnIsLoadingChanged));

    public new object Content
    {
      get => GetValue(ContentProperty);
      set => SetValue(ContentProperty, value);
    }

    public bool IsLoading
    {
      get => (bool)GetValue(IsLoadingProperty);
      set => SetValue(IsLoadingProperty, value);
    }

    private readonly ContentControl _contentControl = new()
    {
      HorizontalContentAlignment = HorizontalAlignment.Stretch,
      VerticalContentAlignment = VerticalAlignment.Stretch,
    };

    private readonly ProgressRing _progressRing = new()
    {
      Width = 100,
      Height = 100,
      IsActive = true,
      Visibility = Visibility.Collapsed
    };

    public LoaderContentControl()
    {
      base.Content = new Grid() { Children = { _contentControl, _progressRing } };

      ElementCompositionPreview.SetImplicitShowAnimation(_progressRing, new OpacityAnimation()
      {
        Duration = TimeSpan.FromMilliseconds(250),
        From = 0,
        To = 1
      }.GetAnimation(_progressRing, out _));
      ElementCompositionPreview.SetImplicitHideAnimation(_progressRing, new OpacityAnimation()
      {
        Duration = TimeSpan.FromMilliseconds(250),
        From = 1,
        To = 0
      }.GetAnimation(_progressRing, out _));
    }

    public static void OnContentChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      if (sender is LoaderContentControl loaderContentControl)
      {
        loaderContentControl._contentControl.Content = e.NewValue;
      }
    }

    public static void OnIsLoadingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      if (sender is LoaderContentControl loaderContentControl && e.NewValue is bool isLoading)
      {
        loaderContentControl._contentControl.IsEnabled = !isLoading;
        loaderContentControl._progressRing.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
      }
    }
  }
}
