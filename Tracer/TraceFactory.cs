using System.Runtime.CompilerServices;
using Tracer.Contracts;

namespace Tracer
{
  public class TraceFactory(
    ITraceDetailFactory traceDetailFactory,
    ITraceDetailProcessor traceDetailProcessor) : ITraceFactory
  {
    private static int _correlationId = 1;
    private static readonly AsyncLocal<int> _asyncLocalCorrelationId = new();
    private readonly ITraceDetailFactory _traceDetailFactory = traceDetailFactory;
    private readonly ITraceDetailProcessor _traceDetailProcessor = traceDetailProcessor;

    public static int CorrelationId => _asyncLocalCorrelationId.Value;

    static TraceFactory()
    {
      _asyncLocalCorrelationId.Value = _correlationId;
    }

    public static void IncrementCorrelationId()
    {
      lock (_asyncLocalCorrelationId)
      {
        _asyncLocalCorrelationId.Value = ++_correlationId;
      }
    }

    public ITrace CreateNew(TraceDomain traceDomain, string arguments = null, [CallerMemberName] string method = null)
    {
      return new Trace(_traceDetailFactory, _traceDetailProcessor, traceDomain, arguments, method);
    }
  }
}
