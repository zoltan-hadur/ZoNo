﻿using Tracer.Contracts;

namespace Tracer
{
  internal record TraceDetail : ITraceDetail
  {
    public ulong Id { get; init; }
    public TimeOnly Time { get; init; }
    public int CorrelationId { get; init; }
    public int ProcessId { get; init; }
    public int ThreadId { get; init; }
    public TraceDirection Direction { get; init; }
    public TraceLevel Level { get; init; }
    public string Method { get; init; }
    public string Message { get; init; }

    public string Compose() => $"{Time:HH\\:mm\\:ss.fffffff} #{CorrelationId,-5} {ProcessId,5}/{ThreadId,-3} [{Direction,-8}] **{Level,-11}** {Method} -->{Message}<--";
  }
}
