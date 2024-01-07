namespace Tracer.Contracts
{
  public interface ITraceDetail
  {
    TimeOnly Time { get; }
    int ProcessId { get; }
    int ThreadId { get; }
    TraceDirection Direction { get; }
    TraceLevel Level { get; }
    TraceDomain Domain { get; }
    string Method { get; }
    string Arguments { get; }
    string Message { get; }

    public string Compose();
  }
}
