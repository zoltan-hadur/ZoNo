namespace Tracer.Contracts
{
  public interface ITraceDetailFactory
  {
    ITraceDetail Create(TraceDirection direction, TraceLevel level, TraceDomain domain, string method, string arguments, string message);
  }
}
