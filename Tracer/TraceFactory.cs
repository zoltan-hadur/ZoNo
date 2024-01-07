using System.Runtime.CompilerServices;
using Tracer.Contracts;

namespace Tracer
{
  public class TraceFactory(
    ITraceDetailFactory traceDetailFactory,
    ITraceDetailProcessor traceDetailProcessor) : ITraceFactory
  {
    private readonly ITraceDetailFactory _traceDetailFactory = traceDetailFactory;
    private readonly ITraceDetailProcessor _traceDetailProcessor = traceDetailProcessor;

    public ITrace CreateNew(TraceDomain traceDomain, string arguments = null, [CallerMemberName] string method = null)
    {
      return new Trace(_traceDetailFactory, _traceDetailProcessor, traceDomain, arguments, method);
    }
  }
}
