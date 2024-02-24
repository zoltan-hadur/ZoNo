using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ZoNo.Helpers
{
  internal class TupleJsonConverter : JsonConverter<ITuple>
  {
    public override bool CanConvert(Type typeToConvert)
    {
      return typeof(ITuple).IsAssignableFrom(typeToConvert);
    }

    public override ITuple Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
      if (reader.TokenType != JsonTokenType.StartArray)
      {
        throw new JsonException($"Json needs to be an array in order to convert it to {typeToConvert.FullName}!");
      }

      var type = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
      if (!typeof(ITuple).IsAssignableFrom(type))
      {
        throw new JsonException($"{type.FullName} is not implementing {typeof(ITuple).FullName}!");
      }

      var array = JsonSerializer.Deserialize<JsonElement[]>(ref reader, options);
      var types = type.GetGenericArguments();
      if (array.Length != types.Length)
      {
        throw new JsonException("Tuple length does not match array length");
      }

      var values = array.Zip(types, (jsonElement, type) => jsonElement.Deserialize(type, options)).ToArray();
      return (ITuple)Activator.CreateInstance(type, values);
    }

    public override void Write(Utf8JsonWriter writer, ITuple tuple, JsonSerializerOptions options)
    {
      writer.WriteStartArray();
      for (int i = 0; i < tuple.Length; ++i)
      {
        JsonSerializer.Serialize(writer, tuple[i], options);
      }
      writer.WriteEndArray();
    }
  }
}