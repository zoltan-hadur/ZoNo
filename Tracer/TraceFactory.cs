using System.Diagnostics;
using System.Runtime.CompilerServices;
using Tracer.Contracts;
using Tracer.Utilities;

namespace Tracer
{
  public class TraceFactory(
    ITraceDetailFactory traceDetailFactory,
    ITraceDetailProcessor traceDetailProcessor) : ITraceFactory
  {
    private static int _correlationId = 1;
    private static readonly AsyncLocal<int> _asyncLocalCorrelationId = new() { Value = _correlationId };
    private readonly ITraceDetailFactory _traceDetailFactory = traceDetailFactory;
    private readonly ITraceDetailProcessor _traceDetailProcessor = traceDetailProcessor;

    public static int CorrelationId
    {
      get
      {
        lock (_asyncLocalCorrelationId)
        {
          if (_asyncLocalCorrelationId.Value == 0)
          {
            IncrementCorrelationId();
          }
          return _asyncLocalCorrelationId.Value;
        }
      }
    }

    public static void IncrementCorrelationId()
    {
      lock (_asyncLocalCorrelationId)
      {
        _asyncLocalCorrelationId.Value = ++_correlationId;
      }
    }

    public ITrace CreateNew([CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = -1)
    {
      var method = TraceDatabase.GetTraceMethod(filePath, lineNumber);
      Debug.Assert(method != null, "method is null!");
      return new Trace(_traceDetailFactory, _traceDetailProcessor, method);
    }
  }
}
