using Tracer.Contracts;

namespace Tracer.Sinks
{
  public class InMemoryTraceSink : ITraceSink
  {
    private readonly object _lock = new();
    private Queue<ITraceDetail> _traceDetails = new();

    private int _size;
    public int Size
    {
      get { lock (_lock) { return _size; } }
      set
      {
        lock (_lock)
        {
          _size = value;
          DequeueUnnecessaryTraceDetails();
        }
      }
    }

    public string[] Traces
    {
      get { lock (_lock) { return _traceDetails.Select(traceDetail => traceDetail.Compose()).ToArray(); } }
    }

    public ITraceDetail[] TraceDetails
    {
      get { lock (_lock) { return _traceDetails.ToArray(); } }
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
      set
      {
        lock (_lock)
        {
          _level = value;
          _traceDetails = new(_traceDetails.Where(traceDetail => traceDetail.Level >= Level));
        }
      }
    }

    public void Write(IEnumerable<ITraceDetail> traceDetails)
    {
      lock (_lock)
      {
        if (!IsEnabled) return;
        foreach (var traceDetail in traceDetails.Where(traceDetail => traceDetail.Level >= Level))
        {
          _traceDetails.Enqueue(traceDetail);
          DequeueUnnecessaryTraceDetails();
        }
      }
    }

    private void DequeueUnnecessaryTraceDetails()
    {
      lock (_lock)
      {
        while (_traceDetails.Count > _size)
        {
          _traceDetails.Dequeue();
        }
      }
    }
  }
}
