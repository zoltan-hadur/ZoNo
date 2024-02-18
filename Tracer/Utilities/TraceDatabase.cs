using System.Collections.Frozen;

namespace Tracer.Utilities
{
  public static class TraceDatabase
  {
    private static FrozenDictionary<(string FilePath, int LineNumber), (string Method, bool IsAsyncVoid)> _traceMethodInfos = null;
    private static FrozenDictionary<(string FilePath, int LineNumber), string[]> _traceFormatParameters = null;

    public static void AddTraceMethodInfos(IEnumerable<(string FilePath, int LineNumber, string TraceMethod, bool IsAsyncVoid)> values)
    {
      if (_traceMethodInfos is not null) throw new InvalidOperationException($"{nameof(TraceDatabase)} trace methods are already added!");
      _traceMethodInfos = values.ToFrozenDictionary(tuple => (tuple.FilePath, tuple.LineNumber), tuple => (tuple.TraceMethod, tuple.IsAsyncVoid));
    }

    public static (string Method, bool IsAsyncVoid)? GetTraceMethodInfo(string filePath, int lineNumber)
    {
      if (_traceMethodInfos is null) throw new InvalidOperationException($"{nameof(TraceDatabase)} trace methods are not added yet!");
      return _traceMethodInfos.TryGetValue((filePath, lineNumber), out var traceMethodInfo) ? traceMethodInfo : null;
    }

    public static void AddTraceFormatParameters(IEnumerable<(string FilePath, int LineNumber, IEnumerable<string> TraceFormatParameters)> values)
    {
      if (_traceFormatParameters is not null) throw new InvalidOperationException($"{nameof(TraceDatabase)} trace format parameters are already added!");
      _traceFormatParameters = values.ToFrozenDictionary(tuple => (tuple.FilePath, tuple.LineNumber), tuple => tuple.TraceFormatParameters.ToArray());
    }

    public static string[] GetTraceFormatParameters(string filePath, int lineNumber)
    {
      if (_traceFormatParameters is null) throw new InvalidOperationException($"{nameof(TraceDatabase)} trace format parameters are not added yet!");
      return _traceFormatParameters.TryGetValue((filePath, lineNumber), out var formatParameters) ? formatParameters : null;
    }
  }
}
