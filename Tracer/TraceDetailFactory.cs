using Tracer.Contracts;

namespace Tracer
{
  public class TraceDetailFactory : ITraceDetailFactory
  {
    public ITraceDetail Create(TraceDirection direction, TraceLevel level, TraceDomain domain, string method, string arguments, string message)
    {
      return new TraceDetail()
      {
        Time = TimeOnly.FromDateTime(DateTime.Now),
        CorrelationId = TraceFactory.CorrelationId,
        ProcessId = Environment.ProcessId,
        ThreadId = Environment.CurrentManagedThreadId,
        Direction = direction,
        Level = level,
        Domain = domain,
        Method = method,
        Arguments = arguments,
        Message = message
      };
    }
  }
}
