using System.Collections.Frozen;

namespace Tracer.Utilities
{
  public static class TraceMethodDatabase
  {
    private static bool _isFinished = false;
    private static Dictionary<(string FilePath, int LineNumber), string> _methodsForFilling = [];
    private static FrozenDictionary<(string FilePath, int LineNumber), string> _methods;

    public static void AddMethod(string filePath, int lineNumber, string method)
    {
      if (_isFinished) throw new InvalidOperationException($"{nameof(TraceMethodDatabase)} method adding is finished!");
      _methodsForFilling.Add((filePath, lineNumber), method);
    }

    public static void FinishAdding()
    {
      _isFinished = true;
      _methods = _methodsForFilling.ToFrozenDictionary();
      _methodsForFilling.Clear();
      _methodsForFilling = null;
    }

    public static string GetMethod(string filePath, int lineNumber)
    {
      if (!_isFinished) throw new InvalidOperationException($"{nameof(TraceMethodDatabase)} method adding is not finished!");
      return _methods.TryGetValue((filePath, lineNumber), out var method) ? method : null;
    }
  }
}
