using System.Collections;

namespace ZoNo.Helpers
{
  public static class EnumerableExtensions
  {
    public static IEnumerable OrEmpty(this IEnumerable enumerable)
    {
      return enumerable ?? Enumerable.Empty<object>();
    }
  }
}
