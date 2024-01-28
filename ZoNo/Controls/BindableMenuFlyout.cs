using Microsoft.UI.Xaml.Controls;
using Tracer.Contracts;

namespace ZoNo.Controls
{
  public class BindableMenuFlyout : MenuFlyout
  {
    private readonly ITraceFactory _traceFactory = App.GetService<ITraceFactory>();

    public BindableMenuFlyout()
    {
      using var trace = _traceFactory.CreateNew();
      Opening += BindableMenuFlyout_Opening;
    }

    private void BindableMenuFlyout_Opening(object sender, object e)
    {
      using var trace = _traceFactory.CreateNew();
      var dataContext = Target?.DataContext ?? (Target as ContentControl)?.Content;
      trace.Debug(Format([dataContext, Items.Count]));
      foreach (var item in Items)
      {
        item.DataContext = dataContext;
      }
    }
  }
}
