using System.Runtime.CompilerServices;

namespace Tracer.Utilities
{
  public static class TraceUtility
  {
    public static string Format(
      object value0,
      object value1 = null,
      object value2 = null,
      object value3 = null,
      object value4 = null,
      object value5 = null,
      object value6 = null,
      object value7 = null,
      object value8 = null,
      object value9 = null,
      [CallerArgumentExpression(nameof(value0))] string value0Expression = null,
      [CallerArgumentExpression(nameof(value1))] string value1Expression = null,
      [CallerArgumentExpression(nameof(value2))] string value2Expression = null,
      [CallerArgumentExpression(nameof(value3))] string value3Expression = null,
      [CallerArgumentExpression(nameof(value4))] string value4Expression = null,
      [CallerArgumentExpression(nameof(value5))] string value5Expression = null,
      [CallerArgumentExpression(nameof(value6))] string value6Expression = null,
      [CallerArgumentExpression(nameof(value7))] string value7Expression = null,
      [CallerArgumentExpression(nameof(value8))] string value8Expression = null,
      [CallerArgumentExpression(nameof(value9))] string value9Expression = null)
    {
      ArgumentNullException.ThrowIfNull(value0);
      if (value1 == null) return $"{value0Expression} = {value0}";
      if (value2 == null) return $"{value0Expression} = {value0}, {value1Expression} = {value1}";
      if (value3 == null) return $"{value0Expression} = {value0}, {value1Expression} = {value1}, {value2Expression} = {value2}";
      if (value4 == null) return $"{value0Expression} = {value0}, {value1Expression} = {value1}, {value2Expression} = {value2}, {value3Expression} = {value3}";
      if (value5 == null) return $"{value0Expression} = {value0}, {value1Expression} = {value1}, {value2Expression} = {value2}, {value3Expression} = {value3}, {value4Expression} = {value4}";
      if (value6 == null) return $"{value0Expression} = {value0}, {value1Expression} = {value1}, {value2Expression} = {value2}, {value3Expression} = {value3}, {value4Expression} = {value4}, {value5Expression} = {value5}";
      if (value7 == null) return $"{value0Expression} = {value0}, {value1Expression} = {value1}, {value2Expression} = {value2}, {value3Expression} = {value3}, {value4Expression} = {value4}, {value5Expression} = {value5}, {value6Expression} = {value6}";
      if (value8 == null) return $"{value0Expression} = {value0}, {value1Expression} = {value1}, {value2Expression} = {value2}, {value3Expression} = {value3}, {value4Expression} = {value4}, {value5Expression} = {value5}, {value6Expression} = {value6}, {value7Expression} = {value7}";
      if (value9 == null) return $"{value0Expression} = {value0}, {value1Expression} = {value1}, {value2Expression} = {value2}, {value3Expression} = {value3}, {value4Expression} = {value4}, {value5Expression} = {value5}, {value6Expression} = {value6}, {value7Expression} = {value7}, {value8Expression} = {value8}";
      return $"{value0Expression} = {value0}, {value1Expression} = {value1}, {value2Expression} = {value2}, {value3Expression} = {value3}, {value4Expression} = {value4}, {value5Expression} = {value5}, {value6Expression} = {value6}, {value7Expression} = {value7}, {value8Expression} = {value8}, {value9Expression} = {value9}";
    }

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
