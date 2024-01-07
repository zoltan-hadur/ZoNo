namespace Tracer.Contracts
{
  public interface ITraceDetailProcessor : IDisposable
  {
    void Process(ITraceDetail traceDetail);
    void Start();
  }
}
