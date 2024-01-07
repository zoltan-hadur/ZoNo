namespace Tracer
{
  public record TraceDomain(Type type)
  {
    public string Name { get; } = type.FullName;
  }
}
