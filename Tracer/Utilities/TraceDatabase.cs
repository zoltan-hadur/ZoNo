using System.Collections.Frozen;

namespace Tracer.Utilities
{
  public static class TraceDatabase
  {
    private static FrozenDictionary<(string FilePath, int LineNumber), string> _traceMethods = null;

    public static void AddTraceMethods(IEnumerable<(string FilePath, int LineNumber, string TraceMethod)> values)
    {
      if (_traceMethods != null) throw new InvalidOperationException($"{nameof(TraceDatabase)} trace methods are already added!");
      _traceMethods = values.ToFrozenDictionary(tuple => (tuple.FilePath, tuple.LineNumber), tuple => tuple.TraceMethod);
    }

    public static string GetTraceMethod(string filePath, int lineNumber)
    {
      if (_traceMethods == null) throw new InvalidOperationException($"{nameof(TraceDatabase)} trace methods are not added!");
      return _traceMethods.TryGetValue((filePath, lineNumber), out var method) ? method : null;
    }
  }
}
