using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using BaseClasses.Interface;
using BaseClasses.Model.Params;

namespace BaseClasses.Services
{
    public class ParamBagJsonConverter : JsonConverter<ParamBag>
    {
        public override ParamBag? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
            {
                return null;
            }

            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("ParamBag must be a JSON array.");
            }

            var bag = new ParamBag();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return bag;
                }

                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException("Each ParamBag item must be a JSON object.");
                }

                using var itemDocument = JsonDocument.ParseValue(ref reader);
                var item = itemDocument.RootElement;

                var @namespace = item.GetProperty("Namespace").GetString()
                                 ?? throw new JsonException("Param namespace cannot be null.");
                var name = item.GetProperty("Name").GetString()
                           ?? throw new JsonException("Param name cannot be null.");
                var typeName = item.GetProperty("Type").GetString()
                               ?? throw new JsonException("Param type cannot be null.");

                var valueType = Type.GetType(typeName, throwOnError: false)
                               ?? throw new JsonException($"Unable to resolve type '{typeName}'.");

                var keyType = typeof(ParamKey<>).MakeGenericType(valueType);
                var key = (IParamKey?)Activator.CreateInstance(keyType, name, @namespace)
                          ?? throw new JsonException($"Unable to create ParamKey for type '{typeName}'.");

                item.TryGetProperty("Value", out var valueElement);
                var rawValue = valueElement.ValueKind == JsonValueKind.Undefined ||
                               valueElement.ValueKind == JsonValueKind.Null
                    ? null
                    : JsonSerializer.Deserialize(valueElement.GetRawText(), valueType, options);

                if (!key.TryConvertValue(rawValue, out var convertedValue))
                {
                    throw new JsonException(
                        $"Value for key '{@namespace}.{name}' cannot be converted to '{valueType.FullName}'.");
                }

                bag[key] = convertedValue;
            }

            throw new JsonException("Unexpected end of JSON while reading ParamBag.");
        }

        public override void Write(Utf8JsonWriter writer, ParamBag value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (var (key, paramValue) in value.Enumerate())
            {
                writer.WriteStartObject();
                writer.WriteString("Namespace", key.Namespace);
                writer.WriteString("Name", key.Name);
                writer.WriteString("Type",
                    key.ValueType.AssemblyQualifiedName ?? key.ValueType.FullName ?? key.ValueType.Name);
                writer.WritePropertyName("Value");
                JsonSerializer.Serialize(writer, paramValue, key.ValueType, options);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }
    }
}