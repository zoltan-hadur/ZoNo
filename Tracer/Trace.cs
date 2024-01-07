using System.Diagnostics;
using Tracer.Contracts;

namespace Tracer
{
  internal class Trace : ITrace
  {
    private readonly ITraceDetailFactory _traceDetailFactory;
    private readonly ITraceDetailProcessor _traceDetailProcessor;
    private readonly TraceDomain _traceDomain;
    private readonly string _arguments;
    private readonly string _method;
    private readonly Stopwatch _stopwatch;

    internal Trace(
      ITraceDetailFactory traceDetailFactory,
      ITraceDetailProcessor traceDetailProcessor,
      TraceDomain traceDomain,
      string arguments,
      string method)
    {
      _traceDetailFactory = traceDetailFactory;
      _traceDetailProcessor = traceDetailProcessor;
      _traceDomain = traceDomain;
      _arguments = arguments;
      _method = method;
      _stopwatch = Stopwatch.StartNew();

      Log(TraceDirection.Entering, TraceLevel.Information, null);
    }

    public void Dispose()
    {
      Log(TraceDirection.Leaving, TraceLevel.Information, $"{_stopwatch.Elapsed.TotalMicroseconds} μs");
    }

    public void Debug(string message) => Log(TraceDirection.Inside, TraceLevel.Debug, message);
    public void Info(string message) => Log(TraceDirection.Inside, TraceLevel.Information, message);
    public void Warn(string message) => Log(TraceDirection.Inside, TraceLevel.Warning, message);
    public void Error(string message) => Log(TraceDirection.Inside, TraceLevel.Error, message);
    public void Fatal(string message) => Log(TraceDirection.Inside, TraceLevel.Fatal, message);

    private void Log(TraceDirection direction, TraceLevel level, string message)
    {
      _traceDetailProcessor.Process(_traceDetailFactory.Create(direction, level, _traceDomain, _method, _arguments, message));
    }
  }
}
