namespace Tracer.Contracts
{
  public interface ITraceSink
  {
    bool IsEnabled { get; set; }
    TraceLevel Level { get; set; }
    void Write(IEnumerable<ITraceDetail> traceDetails);
  }
}
