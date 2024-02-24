using Tracer.Contracts;

namespace ZoNo.Helpers
{
  public sealed class LockGuard : IDisposable
  {
    private static readonly ITraceFactory _traceFactory = App.GetService<ITraceFactory>();
    private readonly SemaphoreSlim _semaphore;

    private LockGuard(SemaphoreSlim semaphore)
    {
      using var trace = _traceFactory.CreateNew();
      _semaphore = semaphore;
    }

    public static async Task<LockGuard> CreateAsync(SemaphoreSlim semaphore, TimeSpan timeout)
    {
      using var trace = _traceFactory.CreateNew();
      if (await semaphore.WaitAsync(timeout) == false)
      {
        throw new InvalidOperationException("Lock has timed out!");
      }
      return new LockGuard(semaphore);
    }

    public void Dispose()
    {
      using var trace = _traceFactory.CreateNew();
      _semaphore.Release();
    }
  }
}
