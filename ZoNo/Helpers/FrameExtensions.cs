using Microsoft.UI.Xaml.Controls;
using Tracer.Contracts;

namespace ZoNo.Helpers
{
  public static class FrameExtensions
  {
    private static readonly ITraceFactory _traceFactory = App.GetService<ITraceFactory>();

    public static object GetPageViewModel(this Frame frame)
    {
      using var trace = _traceFactory.CreateNew();
      return frame?.Content?.GetType().GetProperty("ViewModel")?.GetValue(frame.Content, null);
    }
  }
}