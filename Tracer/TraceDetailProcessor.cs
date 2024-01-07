using System.Collections.Concurrent;
using Tracer.Contracts;

namespace Tracer
{
  public class TraceDetailProcessor : ITraceDetailProcessor
  {
    private readonly Thread _distributorThread;
    private readonly (Thread ConsumerThread, BlockingCollection<ITraceDetail> ConsumerTraceDetails)[] _consumerThreads;
    private readonly BlockingCollection<ITraceDetail> _traceDetails = [];

    public TraceDetailProcessor(IEnumerable<ITraceSink> traceSinks)
    {
      _consumerThreads = traceSinks.Select(traceSink =>
      {
        var consumerTraceDetails = new BlockingCollection<ITraceDetail>();
        var consumerThread = new Thread(() => Consume(consumerTraceDetails, traceSink));
        return (consumerThread, consumerTraceDetails);
      }).ToArray();

      _distributorThread = new Thread(Distribute);
    }

    public void Process(ITraceDetail traceDetail)
    {
      _traceDetails.Add(traceDetail);
    }

    public void Start()
    {
      foreach (var (consumerThread, _) in _consumerThreads)
      {
        consumerThread.Start();
      }

      _distributorThread.Start();
    }

    public void Dispose()
    {
      _traceDetails.CompleteAdding();
      _distributorThread.Join();
      _traceDetails.Dispose();

      foreach (var (consumerThread, consumerTraceDetails) in _consumerThreads)
      {
        consumerTraceDetails.CompleteAdding();
        consumerThread.Join();
        consumerTraceDetails.Dispose();
      }
    }

    private void Distribute()
    {
      foreach (var traceDetail in _traceDetails.GetConsumingEnumerable())
      {
        foreach (var (_, consumerTraceDetails) in _consumerThreads)
        {
          consumerTraceDetails.Add(traceDetail);
        }
      }
    }

    private static void Consume(BlockingCollection<ITraceDetail> traceDetails, ITraceSink traceSink)
    {
      while (!traceDetails.IsCompleted)
      {
        try
        {
          var batchTraceDetails = new List<ITraceDetail>() { traceDetails.Take() };
          while (traceDetails.Count > 0)
          {
            batchTraceDetails.Add(traceDetails.Take());
          }
          traceSink.Write(batchTraceDetails);
        }
        catch (InvalidOperationException) { }
      }
    }
  }
}
