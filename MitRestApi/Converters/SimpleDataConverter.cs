using System.Text.Json.Serialization;
using System.Text.Json;
using System.Globalization;

namespace MitRestApi.Converters
{
    public class SimpleDateConverter : JsonConverter<DateOnly>
    {
        private const string Format = "dd-MMM-yyyy";

        public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString();
            return DateOnly.ParseExact(value, Format, CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(Format));
        }
    }

}