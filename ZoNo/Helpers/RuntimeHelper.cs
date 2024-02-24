using System.Runtime.InteropServices;
using System.Text;
using Tracer.Contracts;

namespace ZoNo.Helpers
{
  public class RuntimeHelper
  {
    private static readonly ITraceFactory _traceFactory = App.GetService<ITraceFactory>();

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern int GetCurrentPackageFullName(ref int packageFullNameLength, StringBuilder packageFullName);

    public static bool IsMSIX
    {
      get
      {
        using var trace = _traceFactory.CreateNew();
        var length = 0;
        var result = GetCurrentPackageFullName(ref length, null) != 15700L;
        trace.Debug(Format([result]));
        return result;
      }
    }
  }
}