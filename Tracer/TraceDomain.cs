using Tracer.Utilities;

namespace Tracer
{
  public record TraceDomain(Type type)
  {
    public string Name { get; } = TraceUtility.GetTypeName(type);
  }
}
