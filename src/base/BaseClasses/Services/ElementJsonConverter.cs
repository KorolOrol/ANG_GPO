using BaseClasses.Interface;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BaseClasses.Services
{
    public class ElementJsonConverter : JsonConverter<IElement>
    {
        public override IElement? Read(ref Utf8JsonReader reader, 
                                       Type typeToConvert, 
                                       JsonSerializerOptions options)
        {
            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                string json = document.RootElement.GetRawText();
                return JsonSerializer.Deserialize<IElement>(json, options);
            }
        }

        public override void Write(Utf8JsonWriter writer, 
                                   IElement value, 
                                   JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("$type", value.GetType().Name);
            foreach (PropertyInfo property in value.GetType().GetProperties())
            {
                object propertyValue = property.GetValue(value);
                string propertyName = options.PropertyNamingPolicy?.ConvertName(property.Name) 
                    ?? property.Name;
                writer.WritePropertyName(propertyName);
                JsonSerializer.Serialize(writer, propertyValue, property.PropertyType, options);
            }
            writer.WriteEndObject();
        }
    }
}
