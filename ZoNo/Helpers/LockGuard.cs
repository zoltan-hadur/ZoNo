using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoNo.Helpers
{
  public sealed class LockGuard : IDisposable
  {
    private SemaphoreSlim _semaphore;

    private LockGuard(SemaphoreSlim semaphore)
    {
      _semaphore = semaphore;
    }

    public static async Task<LockGuard> CreateAsync(SemaphoreSlim semaphore, TimeSpan timeout)
    {
      if (await semaphore.WaitAsync(timeout) == false)
      {
        throw new InvalidOperationException("Lock has timed out!");
      }
      return new LockGuard(semaphore);
    }

    public void Dispose()
    {
      _semaphore.Release();
    }
  }
}
