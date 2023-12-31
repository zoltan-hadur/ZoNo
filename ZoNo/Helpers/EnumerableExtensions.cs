using System.Collections;

namespace ZoNo.Helpers
{
  public static class EnumerableExtensions
  {
    public static IEnumerable OrEmpty(this IEnumerable enumerable)
    {
      return enumerable ?? Enumerable.Empty<object>();
    }

    public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> enumerable)
    {
      return enumerable ?? Enumerable.Empty<T>();
    }
  }
}
