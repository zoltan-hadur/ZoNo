using System.Collections.Frozen;

namespace Tracer.Utilities
{
  public static class TraceDatabase
  {
    private static FrozenDictionary<(string FilePath, int LineNumber), string> _traceMethods = null;
    private static FrozenDictionary<(string FilePath, int LineNumber), string[]> _traceFormatParameters = null;

    public static void AddTraceMethods(IEnumerable<(string FilePath, int LineNumber, string TraceMethod)> values)
    {
      if (_traceMethods != null) throw new InvalidOperationException($"{nameof(TraceDatabase)} trace methods are already added!");
      _traceMethods = values.ToFrozenDictionary(tuple => (tuple.FilePath, tuple.LineNumber), tuple => tuple.TraceMethod);
    }

    public static string GetTraceMethod(string filePath, int lineNumber)
    {
      if (_traceMethods == null) throw new InvalidOperationException($"{nameof(TraceDatabase)} trace methods are not added yet!");
      return _traceMethods.TryGetValue((filePath, lineNumber), out var method) ? method : null;
    }

    public static void AddTraceFormatParameters(IEnumerable<(string FilePath, int LineNumber, IEnumerable<string> TraceFormatParameters)> values)
    {
      if (_traceFormatParameters != null) throw new InvalidOperationException($"{nameof(TraceDatabase)} trace format parameters are already added!");
      _traceFormatParameters = values.ToFrozenDictionary(tuple => (tuple.FilePath, tuple.LineNumber), tuple => tuple.TraceFormatParameters.ToArray());
    }

    public static string[] GetTraceFormatParameters(string filePath, int lineNumber)
    {
      if (_traceFormatParameters == null) throw new InvalidOperationException($"{nameof(TraceDatabase)} trace format parameters are not added yet!");
      return _traceFormatParameters.TryGetValue((filePath, lineNumber), out var formatParameters) ? formatParameters : null;
    }
  }
}
