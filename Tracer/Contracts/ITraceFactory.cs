using System.Runtime.CompilerServices;

namespace Tracer.Contracts
{
  public interface ITraceFactory
  {
    ITrace CreateNew(TraceDomain traceDomain, string arguments = null, [CallerMemberName] string method = null);
  }
}
