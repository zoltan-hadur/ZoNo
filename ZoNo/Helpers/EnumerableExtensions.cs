using System.Collections;
using Tracer.Contracts;

namespace ZoNo.Helpers
{
  public static class EnumerableExtensions
  {
    private static readonly ITraceFactory _traceFactory = App.GetService<ITraceFactory>();

    public static IEnumerable OrEmpty(this IEnumerable enumerable)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([enumerable is null]));
      return enumerable ?? Enumerable.Empty<object>();
    }

    public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> enumerable)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([enumerable is null, GetTypeName(typeof(T))]));
      return enumerable ?? Enumerable.Empty<T>();
    }
  }
}
