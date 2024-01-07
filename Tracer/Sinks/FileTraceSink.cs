using Tracer.Contracts;

namespace Tracer.Sinks
{
  public class FileTraceSink : ITraceSink
  {
    private readonly object _lock = new();

    private string _path;
    public string Path
    {
      get { lock (_lock) { return _path; } }
      set { lock (_lock) { _path = value; } }
    }

    private bool _isEnabled;
    public bool IsEnabled
    {
      get { lock (_lock) { return _isEnabled; } }
      set { lock (_lock) { _isEnabled = value; } }
    }

    private TraceLevel _level;
    public TraceLevel Level
    {
      get { lock (_lock) { return _level; } }
      set { lock (_lock) { _level = value; } }
    }

    public void Write(IEnumerable<ITraceDetail> traceDetails)
    {
      lock (_lock)
      {
        if (!IsEnabled) return;
        File.AppendAllLines(Path, traceDetails.Where(traceDetail => traceDetail.Level >= Level).Select(traceDetail => traceDetail.Compose()));
      }
    }
  }
}
