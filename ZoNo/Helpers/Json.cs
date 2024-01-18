using System.Text;
using System.Text.Json;

namespace ZoNo.Helpers
{
  public static partial class Json
  {
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
      Converters = { new TupleJsonConverter() }
    };

    public static async Task<T> ToObjectAsync<T>(string value)
    {
      using var stream = new MemoryStream(Encoding.UTF8.GetBytes(value));
      return await JsonSerializer.DeserializeAsync<T>(stream, _jsonSerializerOptions);
    }

    public static async Task<string> StringifyAsync(object value)
    {
      using var stream = new MemoryStream();
      await JsonSerializer.SerializeAsync(stream, value, _jsonSerializerOptions);
      return Encoding.UTF8.GetString(stream.ToArray());
    }
  }
}