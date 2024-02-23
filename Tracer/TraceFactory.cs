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
    private static readonly AsyncLocal<bool> _asyncLocalHandleAsAsyncVoid = new() { Value = false };
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
      var traceMethodInfo = TraceDatabase.GetTraceMethodInfo(filePath, lineNumber);
      Debug.Assert(traceMethodInfo is not null, "method info is null!");
      string message = null;
      if (traceMethodInfo.Value.IsAsyncVoid || _asyncLocalHandleAsAsyncVoid.Value)
      {
        _asyncLocalHandleAsAsyncVoid.Value = false;
        var previousCorrelationId = CorrelationId;
        IncrementCorrelationId();
        message = $"{previousCorrelationId} -> {CorrelationId}";
      }
      else if (_asyncLocalCorrelationId.Value == 0)
      {
        message = $"0 -> {CorrelationId}";
      }
      return new Trace(_traceDetailFactory, _traceDetailProcessor, traceMethodInfo.Value.Method, message);
    }

    public static async Task HandleAsAsyncVoid(Func<Task> func)
    {
      _asyncLocalHandleAsAsyncVoid.Value = true;
      await func();
    }

    public static async Task<T> HandleAsAsyncVoid<T>(Func<Task<T>> func)
    {
      _asyncLocalHandleAsAsyncVoid.Value = true;
      return await func();
    }
  }
}
