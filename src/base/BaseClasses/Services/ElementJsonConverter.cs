using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using BaseClasses.Enum;
using BaseClasses.Model;
using BaseClasses.Model.Params;

namespace BaseClasses.Services
{
    public class ElementJsonConverter : JsonConverter<Element>
    {
        private static readonly FieldInfo? _IdField = typeof(Element).GetField("_id", 
            BindingFlags.Instance | BindingFlags.NonPublic);

        public override Element? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException("Element must be a JSON object.");
            }

            using var document = JsonDocument.ParseValue(ref reader);
            var root = document.RootElement;

            var id = root.GetProperty("Id").GetGuid();
            var type = ReadElementType(root.GetProperty("Type"));
            var name = root.GetProperty("Name").GetString() ?? string.Empty;
            var description = root.GetProperty("Description").GetString() ?? string.Empty;
            var time = root.GetProperty("Time").GetInt32();

            var element = new Element(type, name, description, time);
            SetElementId(element, id);

            if (!root.TryGetProperty("Params", out var paramsProperty)
                || paramsProperty.ValueKind == JsonValueKind.Null) return element;
            var paramBag = JsonSerializer.Deserialize<ParamBag>(paramsProperty.GetRawText(), options);
            if (paramBag == null) return element;
            foreach (var pair in paramBag.Enumerate())
            {
                element.Params[pair.Key] = pair.Value;
            }

            return element;
        }

        public override void Write(Utf8JsonWriter writer, Element value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteString("Id", GetElementId(value));
            writer.WriteNumber("Type", (int)value.Type);
            writer.WriteString("Name", value.Name);
            writer.WriteString("Description", value.Description);
            writer.WritePropertyName("Params");
            JsonSerializer.Serialize(writer, value.Params, options);
            writer.WriteNumber("Time", value.Time);

            writer.WriteEndObject();
        }

        private static ElemType ReadElementType(JsonElement typeProperty)
        {
            return typeProperty.ValueKind switch
            {
                JsonValueKind.Number => (ElemType)typeProperty.GetInt32(),
                JsonValueKind.String when System.Enum.TryParse<ElemType>(typeProperty.GetString(),
                    true, out var parsed) => parsed,
                _ => throw new JsonException("Invalid element type value.")
            };
        }

        private static Guid GetElementId(Element element)
        {
            if (_IdField == null)
            {
                throw new JsonException("Element id field was not found.");
            }

            if (_IdField.GetValue(element) is Guid id)
            {
                return id;
            }

            throw new JsonException("Element id field contains invalid value.");
        }

        private static void SetElementId(Element element, Guid id)
        {
            if (_IdField == null)
            {
                throw new JsonException("Element id field was not found.");
            }

            try
            {
                _IdField.SetValue(element, id);
            }
            catch (Exception ex)
            {
                throw new JsonException("Unable to set element id.", ex);
            }
        }
    }
}