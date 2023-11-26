using CommunityToolkit.WinUI.UI;
using CommunityToolkit.WinUI.UI.Animations;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Shapes;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace ZoNo.Controls
{
  [ContentProperty(Name = nameof(Children))]
  public class AutomaticStackPanel : ContentControl
  {
    public static readonly DependencyProperty ChildrenProperty = DependencyProperty.Register(nameof(Children), typeof(ObservableCollection<object>), typeof(AutomaticStackPanel), new PropertyMetadata(null, OnChildrenChanged));
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(AutomaticStackPanel), new PropertyMetadata(Orientation.Vertical, OnOrientationChanged));
    public static readonly DependencyProperty StartingOrientationProperty = DependencyProperty.Register(nameof(StartingOrientation), typeof(Orientation), typeof(AutomaticStackPanel), new PropertyMetadata(Orientation.Vertical, OnStartingOrientationChanged));
    public static readonly DependencyProperty ThresholdProperty = DependencyProperty.Register(nameof(Threshold), typeof(double), typeof(AutomaticStackPanel), null);
    public static readonly DependencyProperty SpacingProperty = DependencyProperty.Register(nameof(Spacing), typeof(double), typeof(AutomaticStackPanel), new PropertyMetadata(null, OnSpacingChanged));

    public ObservableCollection<object> Children
    {
      get => (ObservableCollection<object>)GetValue(ChildrenProperty);
      set => SetValue(ChildrenProperty, value);
    }

    public Orientation Orientation
    {
      get => (Orientation)GetValue(OrientationProperty);
      set => SetValue(OrientationProperty, value);
    }

    public Orientation StartingOrientation
    {
      get => (Orientation)GetValue(StartingOrientationProperty);
      set => SetValue(StartingOrientationProperty, value);
    }

    public double Threshold
    {
      get => (double)GetValue(ThresholdProperty);
      set => SetValue(ThresholdProperty, value);
    }

    public double Spacing
    {
      get => (double)GetValue(SpacingProperty);
      set => SetValue(SpacingProperty, value);
    }

    private readonly Grid _grid = new();
    private readonly Canvas _canvas = new();

    public AutomaticStackPanel()
    {
      VerticalContentAlignment = VerticalAlignment.Stretch;
      HorizontalContentAlignment = HorizontalAlignment.Stretch;
      Children = [];
      Content = new Grid() { Children = { _grid, _canvas } };
      SizeChanged += AutomaticStackPanel_SizeChanged;
    }

    private void AutomaticStackPanel_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      var newValue = StartingOrientation == Orientation.Vertical ? e.NewSize.Width : e.NewSize.Height;
      if (newValue >= Threshold)
      {
        Orientation = StartingOrientation == Orientation.Vertical ? Orientation.Horizontal : Orientation.Vertical;
      }
      else
      {
        Orientation = StartingOrientation;
      }
    }

    public static void OnChildrenChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      if (sender is AutomaticStackPanel panel)
      {
        if (e.OldValue is ObservableCollection<object> oldChildren)
        {
          oldChildren.CollectionChanged -= panel.Children_CollectionChanged;
        }
        if (e.NewValue is ObservableCollection<object> newChildren)
        {
          newChildren.CollectionChanged += panel.Children_CollectionChanged;
        }
      }
    }

    public static void OnOrientationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      if (sender is AutomaticStackPanel panel)
      {
        panel.RearrangeChildren();
      }
    }

    public static void OnStartingOrientationChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      if (sender is AutomaticStackPanel panel && e.NewValue is Orientation orientation)
      {
        panel.Orientation = orientation;
      }
    }

    public static void OnSpacingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {
      if (sender is AutomaticStackPanel panel)
      {
        panel._grid.RowSpacing = panel._grid.ColumnSpacing = panel.Spacing;
      }
    }

    private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      switch (e.Action)
      {
        case NotifyCollectionChangedAction.Add:
          {
            foreach (UIElement item in e.NewItems)
            {
              OnItemAdded(item);
            }
          }
          break;
      }
    }

    private void OnItemAdded(UIElement item)
    {
      var rectangle = new Rectangle();
      _grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
      _grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
      _grid.Children.Add(rectangle);
      _canvas.Children.Add(item);
      rectangle.Loaded += (s, e) =>
      {
        item.SetValue(WidthProperty, rectangle.ActualWidth);
        item.SetValue(HeightProperty, rectangle.ActualHeight);
        item.GetVisual().Offset = rectangle.ActualOffset;
        rectangle.Tag = true;
      };
      BindItems(rectangle, item);
      RearrangeChildren();
    }

    private void RearrangeChildren()
    {
      for (int i = 0; i < _grid.Children.Count; ++i)
      {
        switch (Orientation)
        {
          case Orientation.Vertical:
            _grid.Children[i].SetValue(Grid.RowProperty, i);
            _grid.Children[i].SetValue(Grid.ColumnProperty, 0);
            _grid.Children[i].SetValue(Grid.RowSpanProperty, 1);
            _grid.Children[i].SetValue(Grid.ColumnSpanProperty, _grid.Children.Count);
            break;
          case Orientation.Horizontal:
            _grid.Children[i].SetValue(Grid.RowProperty, 0);
            _grid.Children[i].SetValue(Grid.ColumnProperty, i);
            _grid.Children[i].SetValue(Grid.RowSpanProperty, _grid.Children.Count);
            _grid.Children[i].SetValue(Grid.ColumnSpanProperty, 1);
            break;
        }
      }
    }

    private static void BindItems(FrameworkElement elementInGrid, UIElement elementInCanvas)
    {
      elementInGrid.SizeChanged += (s, e) =>
      {
        if (elementInGrid.Tag is bool isLoaded && isLoaded)
        {
          AnimationBuilder.Create()
          .Size(
            to: elementInGrid.ActualSize,
            duration: TimeSpan.FromMilliseconds(600),
            easingMode: EasingMode.EaseOut,
            layer: FrameworkLayer.Xaml)
          .Offset(
            to: elementInGrid.ActualOffset,
            easingMode: EasingMode.EaseOut,
            duration: TimeSpan.FromMilliseconds(600))
          .Start(elementInCanvas);
        }
      };
    }
  }
}
