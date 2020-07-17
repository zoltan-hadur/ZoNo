using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ZoNo.Extensions
{
  public static class DispatcherExtension
  {
    public static void DelayInvoke(this Dispatcher dispatcher, TimeSpan timeSpan, Action action)
    {
      Task.Delay(timeSpan).ContinueWith((wTask) => dispatcher.Invoke(action));
    }
  }
}
