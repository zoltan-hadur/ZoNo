using System.Runtime.CompilerServices;

namespace Tracer.Utilities
{
  public static class TraceUtility
  {
    public static string Format(object value, [CallerArgumentExpression(nameof(value))] string expression = null) => $"{expression} = {value}";
    public static string GetTypeName(Type type)
    {
      if (type.IsGenericType)
      {
        return $"{type.FullName[..type.FullName.IndexOf('`')]}<{string.Join(", ", type.GetGenericArguments().Select(GetTypeName))}>";
      }
      else
      {
        return type.FullName;
      }
    }
  }
}
