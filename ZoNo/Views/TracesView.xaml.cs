using CommunityToolkit.WinUI.UI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.Frozen;
using Tracer;
using Tracer.Contracts;
using Windows.Foundation;
using ZoNo.ViewModels;

namespace ZoNo.Views
{
  public sealed partial class TracesView : UserControl
  {
    private double _width = 0;
    private readonly FrozenDictionary<TraceLevel, SolidColorBrush> _colors = new Dictionary<TraceLevel, SolidColorBrush>()
    {
      { TraceLevel.Debug, new SolidColorBrush(Colors.Gray) },
      { TraceLevel.Warning, new SolidColorBrush(Colors.Orange) },
      { TraceLevel.Error, new SolidColorBrush(Colors.Crimson) },
      { TraceLevel.Fatal, new SolidColorBrush(Colors.DeepPink) },
    }.ToFrozenDictionary();

    public TracesViewModel ViewModel { get; }

    public TracesView()
    {
      ViewModel = App.GetService<TracesViewModel>();

      if (ViewModel.TraceDetails.Source.Cast<ITraceDetail>().ToArray() is var traceDetails && traceDetails.Length != 0)
      {
        var traces = traceDetails.SelectMany(x => x.Compose().Split('\r', '\n')).ToArray();
        var longestTraceLength = traces.Max(str => str.Length);
        var longestTrace = traces.First(str => str.Length == longestTraceLength);
        var textBlock = new TextBlock() { FontFamily = new FontFamily("Courier New"), Text = longestTrace };
        textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        _width = textBlock.DesiredSize.Width;
      }

      InitializeComponent();
    }

    private void ListView_Loaded(object sender, RoutedEventArgs e)
    {
      if (sender is ListView listView)
      {
        if (listView.FindDescendant("ScrollContentPresenter") is ScrollContentPresenter scrollContentPresenter)
        {
          scrollContentPresenter.Margin = new Thickness(0, 0, 14, 12);
        }
      }
    }

    private void TextBlock_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
    {
      if (sender is TextBlock traceText && traceText.DataContext is ITraceDetail traceDetail)
      {
        if (traceDetail.Level == TraceLevel.Information)
        {
          traceText.ClearValue(TextBlock.ForegroundProperty);
        }
        else
        {
          traceText.Foreground = _colors[traceDetail.Level];
        }
      }
    }

    private void TextBlock_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (sender is TextBlock traceText)
      {
        traceText.Width = _width;
      }
    }

    private async void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
      await ViewModel.LoadAsync();
    }
  }
}
