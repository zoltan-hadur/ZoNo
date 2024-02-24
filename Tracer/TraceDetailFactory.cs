using Tracer.Contracts;

namespace Tracer
{
  public class TraceDetailFactory : ITraceDetailFactory
  {
    private static uint _id = 0;

    public ITraceDetail Create(TraceDirection direction, TraceLevel level, string method, string message)
    {
      return new TraceDetail()
      {
        Id = Interlocked.Increment(ref _id),
        Time = TimeOnly.FromDateTime(DateTime.Now),
        CorrelationId = TraceFactory.CorrelationId,
        ProcessId = Environment.ProcessId,
        ThreadId = Environment.CurrentManagedThreadId,
        Direction = direction,
        Level = level,
        Method = method,
        Message = message
      };
    }
  }
}
