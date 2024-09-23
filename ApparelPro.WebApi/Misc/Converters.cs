using System.Text.Json.Serialization;
using System.Text.Json;

namespace ApparelPro.WebApi.Misc
{
    public class Converters
    {}

    internal sealed class JsonToByteArrayConverter : JsonConverter<byte[]?>
    {
        // Converts base64 encoded string to byte[].
        public override byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (!reader.TryGetBytesFromBase64(out byte[]? result) || result == default)
            {
                throw new Exception("Failed to convert to byte array. Invalid configuration");
            }
            return result;
        }

        // Converts byte[] to base64 encoded string.
        public override void Write(Utf8JsonWriter writer, byte[]? value, JsonSerializerOptions options)
        {
            writer.WriteBase64StringValue(value);
        }
    }


    public class ByteArrayConverter : JsonConverter<byte[]>
    {
        public override byte[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            short[] sByteArray = JsonSerializer.Deserialize<short[]>(ref reader);
            byte[] value = new byte[sByteArray.Length];
            for (int i = 0; i < sByteArray.Length; i++)
            {
                value[i] = (byte)sByteArray[i];
            }

            return value;
        }

        public override void Write(Utf8JsonWriter writer, byte[] value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (var val in value)
            {
                writer.WriteNumberValue(val);
            }

            writer.WriteEndArray();
        }

        public class EverythingToStringJsonConverter : JsonConverter<string>
        {
            public override string Read(ref Utf8JsonReader reader,
                Type typeToConvert,
                JsonSerializerOptions options)
            {

                if (reader.TokenType == JsonTokenType.String)
                {
                    return reader.GetString() ?? String.Empty;
                }
                else if (reader.TokenType == JsonTokenType.Number)
                {
                    var stringValue = reader.GetDouble();
                    return stringValue.ToString();
                }
                else if (reader.TokenType == JsonTokenType.False ||
                    reader.TokenType == JsonTokenType.True)
                {
                    return reader.GetBoolean().ToString();
                }
                else if (reader.TokenType == JsonTokenType.StartObject)
                {
                    reader.Skip();
                    return "(not supported)";
                }
                else
                {
                    Console.WriteLine($"Unsupported token type: {reader.TokenType}");

                    throw new System.Text.Json.JsonException();
                }
            }

            public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
            {
                writer.WriteStringValue(value);
            }
        }
    }
}
