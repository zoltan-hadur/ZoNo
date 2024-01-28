using System.Text;
using System.Text.Json;
using Tracer.Contracts;

namespace ZoNo.Helpers
{
  public static class Json
  {
    private static readonly ITraceFactory _traceFactory = App.GetService<ITraceFactory>();
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new() { Converters = { new TupleJsonConverter() } };

    public static async Task<T> ToObjectAsync<T>(string value)
    {
      using var trace = _traceFactory.CreateNew();
      trace.Debug(Format([GetTypeName(typeof(T)), value]));
      using var stream = new MemoryStream(Encoding.UTF8.GetBytes(value));
      return await JsonSerializer.DeserializeAsync<T>(stream, _jsonSerializerOptions);
    }

    public static async Task<string> StringifyAsync(object value)
    {
      using var trace = _traceFactory.CreateNew();
      using var stream = new MemoryStream();
      await JsonSerializer.SerializeAsync(stream, value, _jsonSerializerOptions);
      var result = Encoding.UTF8.GetString(stream.ToArray());
      trace.Debug(Format([GetTypeName(value.GetType()), result]));
      return result;
    }
  }
}