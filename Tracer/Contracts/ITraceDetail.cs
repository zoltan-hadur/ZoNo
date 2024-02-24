namespace Tracer.Contracts
{
  public interface ITraceDetail
  {
    ulong Id { get; }
    TimeOnly Time { get; }
    int CorrelationId { get; }
    int ProcessId { get; }
    int ThreadId { get; }
    TraceDirection Direction { get; }
    TraceLevel Level { get; }
    string Method { get; }
    string Message { get; }

    public string Compose();
  }
}
