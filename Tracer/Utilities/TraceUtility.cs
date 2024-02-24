using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Tracer.Utilities
{
  public static class TraceUtility
  {
    public static string Format(IEnumerable<object> objects, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = -1)
    {
      Debug.Assert(objects.Count() == TraceDatabase.GetTraceFormatParameters(filePath, lineNumber).Length, "object.Count is not equal to parameters.Length!");
      return string.Join(", ", TraceDatabase.GetTraceFormatParameters(filePath, lineNumber).Zip(objects, (parameter, obj) => $"{parameter} = {obj}"));
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
