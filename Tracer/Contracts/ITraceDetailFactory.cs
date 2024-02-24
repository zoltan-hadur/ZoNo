namespace Tracer.Contracts
{
  public interface ITraceDetailFactory
  {
    ITraceDetail Create(TraceDirection direction, TraceLevel level, string method, string message);
  }
}
