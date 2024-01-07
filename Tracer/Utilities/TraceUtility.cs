using System.Runtime.CompilerServices;

namespace Tracer.Utilities
{
  public static class TraceUtility
  {
    public static string Format(object value, [CallerArgumentExpression(nameof(value))] string expression = null) => $"{expression} = {value}";
  }
}
