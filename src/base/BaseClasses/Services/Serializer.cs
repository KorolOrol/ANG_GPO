using System;
using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Model.Params;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BaseClasses.Services
{
    /// <summary>
    /// Сериализатор.
    /// </summary>
    public static class Serializer
    {
        private static readonly FieldInfo? _ElementIdField = typeof(Element).GetField("_id",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// Настройки сериализации.
        /// </summary>
        public static JsonSerializerOptions Options { get; set; } = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic),
            Converters = { new ParamBagJsonConverter(), new ElementJsonConverter() }
        };

        /// <summary>
        /// Сериализация истории.
        /// </summary>
        /// <param name="plot">История.</param>
        /// <param name="path">Путь к файлу.</param>
        public static void Serialize(Plot plot, string path)
        {
            File.WriteAllText(path, SerializeToString(plot));
        }

        /// <summary>
        /// Сериализация истории в JSON-строку без записи в файл.
        /// </summary>
        /// <param name="plot">История.</param>
        /// <returns>JSON-представление истории.</returns>
        public static string SerializeToString(Plot plot)
        {
            return JsonSerializer.Serialize(ToPlotDto(plot), Options);
        }

        /// <summary>
        /// Сериализация элемента истории.
        /// </summary>
        /// <param name="element">Элемент.</param>
        /// <param name="path">Путь к файлу.</param>
        public static void Serialize(IElement element, string path)
        {
            File.WriteAllText(path, SerializeToString(element));
        }

        /// <summary>
        /// Сериализация элемента истории в JSON-строку без записи в файл.
        /// </summary>
        /// <param name="element">Элемент.</param>
        /// <returns>JSON-представление элемента.</returns>
        public static string SerializeToString(IElement element)
        {
            if (!(element is Element concreteElement))
            {
                throw new ArgumentException($"Only {nameof(Element)} is supported for serialization.", nameof(element));
            }

            return JsonSerializer.Serialize(concreteElement, Options);
        }
        
        /// <summary>
        /// Сериализация связи между элементами истории.
        /// </summary>
        /// <param name="relation">Связь между элементами истории.</param>
        /// <param name="path">Путь к файлу.</param>
        public static void Serialize(Relation relation, string path)
        {
            File.WriteAllText(path, SerializeToString(relation));
        }

        /// <summary>
        /// Сериализация связи между элементами истории в JSON-строку без записи в файл.
        /// </summary>
        /// <param name="relation">Связь между элементами истории.</param>
        /// <returns>>JSON-представление связи между элементами истории.</returns>
        public static string SerializeToString(Relation relation)
        {
            return JsonSerializer.Serialize(ToRelationDto(relation), Options);
        }

        /// <summary>
        /// Десериализация.
        /// </summary>
        /// <typeparam name="T">Тип объекта.</typeparam>
        /// <param name="path">Путь к файлу.</param>
        /// <param name="plot">История, необходимая для корректного восстановления
        /// связей между элементами при десериализации отношений.</param>
        /// <returns>Объект.</returns>
        public static T Deserialize<T>(string path, Plot? plot = null)
        {
            var json = File.ReadAllText(path);
            return DeserializeString<T>(json, plot);
        }

        /// <summary>
        /// Десериализация из JSON-строки.
        /// </summary>
        /// <param name="json">JSON-строка, представляющая сериализованный объект.</param>
        /// <param name="plot">История, необходимая для корректного восстановления
        /// связей между элементами при десериализации отношений.</param>
        /// <typeparam name="T">Тип объекта.</typeparam>
        /// <returns>Объект.</returns>
        /// <exception cref="JsonException">Выбрасывается при ошибках десериализации,
        /// таких как несоответствие типов, отсутствие необходимых полей или некорректные значения.</exception>
        /// <exception cref="ArgumentNullException">Выбрасывается, если для десериализации отношения
        /// не была предоставлена история.</exception>
        /// <exception cref="ArgumentException">Выбрасывается, если тип T не поддерживается для десериализации.</exception>
        public static T DeserializeString<T>(string json, Plot? plot = null)
        {
            if (typeof(T) == typeof(Plot))
            {
                var dto = JsonSerializer.Deserialize<PlotDto>(json, Options)
                          ?? throw new JsonException("Failed to deserialize plot JSON.");
                var plotFromDto = FromPlotDto(dto);
                return (T)Convert.ChangeType(plotFromDto, typeof(T));
            }

            if (typeof(T) == typeof(Element))
            {
                var element = JsonSerializer.Deserialize<Element>(json, Options)
                              ?? throw new JsonException("Failed to deserialize element JSON.");
                return (T)Convert.ChangeType(element, typeof(T));
            }

            if (typeof(T) == typeof(Relation))
            {
                if (plot == null)
                {
                    throw new ArgumentNullException(nameof(plot), "Plot must be provided to deserialize a Relation.");
                }
                var dto = JsonSerializer.Deserialize<RelationDto>(json, Options)
                          ?? throw new JsonException("Failed to deserialize relation JSON.");
                var relation = FromRelationDto(dto, plot);
                return (T)Convert.ChangeType(relation, typeof(T));
            }

            throw new ArgumentException(
                $"Only '{nameof(Plot)}', '{nameof(Element)}' and '{nameof(Relation)}' " +
                $"types are supported by this method. " +
                $"Attempted to deserialize '{typeof(T).Name}'.");
        }

        /// <summary>
        /// Печать информации об истории.
        /// </summary>
        /// <param name="plot">История.</param>
        /// <param name="path">Путь к файлу.</param>
        public static void Print(Plot plot, string path)
        {
            File.WriteAllText(path, PrintToString(plot));
        }

        /// <summary>
        /// Формирование текстового представления истории без записи в файл.
        /// </summary>
        /// <param name="plot">История.</param>
        /// <returns>Текстовое представление истории.</returns>
        public static string PrintToString(Plot plot)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Time: {plot.Time}");
            sb.AppendLine("Elements:");
            foreach (var element in plot.Elements)
            {
                sb.AppendLine(PrintToString(element));
            }

            sb.AppendLine("Relations:");
            foreach (var relation in plot.Relations)
            {
                sb.AppendLine($"- {relation}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Печать информации об элементе истории.
        /// </summary>
        /// <param name="element">Элемент.</param>
        /// <param name="path">Путь к файлу.</param>
        public static void Print(IElement element, string path)
        {
            File.WriteAllText(path, PrintToString(element));
        }

        /// <summary>
        /// Формирование текстового представления элемента без записи в файл.
        /// </summary>
        /// <param name="element">Элемент.</param>
        /// <returns>Текстовое представление элемента.</returns>
        public static string PrintToString(IElement element)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"- {element.Type}: {element.Name} ({element.Time})");
            sb.AppendLine($"  Description: {element.Description}");
            sb.AppendLine($"  Params:");
            foreach (var param in element.Params.Enumerate())
            {
                string value;
                if (param.Key.IsCollection)
                {
                    value = param.Value is IEnumerable<object> items ? $"[{string.Join(", ", items)}]" : "null";
                }
                else
                {
                    value = param.Value?.ToString() ?? "null";
                }
                sb.AppendLine($"    - {param.Key.Namespace}.{param.Key.Name} ({param.Key.ValueType.Name}): {value}");
            }
            return sb.ToString();
        }

        private static RelationDto ToRelationDto(Relation relation)
        {
            return new RelationDto
            {
                Source = GetElementId(RequireElement(relation.Source)),
                Target = GetElementId(RequireElement(relation.Target)),
                Namespace = relation.Param.Namespace,
                Name = relation.Param.Name,
                Type = TypeNameHelper.GetTypeName(relation.Param.ValueType),
                Value = relation.Value
            };
        }

        private static Relation FromRelationDto(RelationDto dto, Plot plot)
        {
            return new Relation(
                RequireElement(plot.Elements.FirstOrDefault(e => GetElementId(e) == dto.Source)
                               ?? throw new JsonException($"Relation source with id '{dto.Source}' was not found.")),
                RequireElement(plot.Elements.FirstOrDefault(e => GetElementId(e) == dto.Target)
                               ?? throw new JsonException($"Relation target with id '{dto.Target}' was not found.")),
                CreateParamKey(dto.Name, dto.Namespace, TypeNameHelper.ResolveType(dto.Type)),
                DeserializeTypedValue(dto.Value, TypeNameHelper.ResolveType(dto.Type))
            );
        }

        private static PlotDto ToPlotDto(Plot plot)
        {
            var elements = plot.Elements.Select(RequireElement).ToList();
            var relations = plot.Relations.Select(ToRelationDto).ToList();

            return new PlotDto
            {
                Time = plot.Time,
                Elements = elements,
                Relations = relations
            };
        }

        private static Plot FromPlotDto(PlotDto dto)
        {
            var plot = new Plot { Time = dto.Time };
            var elementsById = new Dictionary<Guid, IElement>();

            foreach (var element in dto.Elements)
            {
                var concreteElement = RequireElement(element);
                plot.Add(concreteElement);
                elementsById[GetElementId(concreteElement)] = concreteElement;
            }

            foreach (var relationDto in dto.Relations)
            {
                if (!elementsById.TryGetValue(relationDto.Source, out var source))
                {
                    throw new JsonException($"Relation source '{relationDto.Source}' was not found in Elements.");
                }

                if (!elementsById.TryGetValue(relationDto.Target, out var target))
                {
                    throw new JsonException($"Relation target '{relationDto.Target}' was not found in Elements.");
                }

                var valueType = TypeNameHelper.ResolveType(relationDto.Type);
                var key = CreateParamKey(relationDto.Name, relationDto.Namespace, valueType);
                var rawValue = DeserializeTypedValue(relationDto.Value, valueType);

                if (!key.TryConvertValue(rawValue, out var convertedValue))
                {
                    throw new JsonException(
                        $"Relation value for key '{relationDto.Namespace}.{relationDto.Name}' " +
                        $"cannot be converted to '{valueType.FullName}'.");
                }

                plot.Relations.Add(new Relation(source, target, key, convertedValue));
            }

            return plot;
        }

        private static Element RequireElement(IElement element)
        {
            if (element is Element concreteElement)
            {
                return concreteElement;
            }

            throw new JsonException($"Only {nameof(Element)} implementations of {nameof(IElement)} are supported.");
        }

        private static Guid GetElementId(IElement element)
        {
            var re = RequireElement(element);
            
            if (_ElementIdField == null)
            {
                throw new JsonException("Element id field was not found.");
            }

            if (_ElementIdField.GetValue(re) is Guid id)
            {
                return id;
            }

            throw new JsonException("Element id field contains invalid value.");
        }
        
        private static IParamKey CreateParamKey(string name, string @namespace, Type valueType)
        {
            var keyType = typeof(ParamKey<>).MakeGenericType(valueType);
            return (IParamKey?)Activator.CreateInstance(keyType, name, @namespace)
                   ?? throw new JsonException($"Unable to create relation key '{@namespace}.{name}'.");
        }

        private static object? DeserializeTypedValue(object? valueToken, Type valueType)
        {
            switch (valueToken)
            {
                case null:
                case JsonElement jsonElement when jsonElement.ValueKind == JsonValueKind.Null ||
                                                  jsonElement.ValueKind == JsonValueKind.Undefined:
                    return null;
                case JsonElement jsonElement:
                    return JsonSerializer.Deserialize(jsonElement.GetRawText(), valueType, Options);
            }

            return valueType.IsInstanceOfType(valueToken)
                ? valueToken
                : JsonSerializer.Deserialize(JsonSerializer.Serialize(valueToken, Options), valueType, Options);
        }

        
        private sealed class PlotDto
        {
            public int Time { get; set; }
            public List<Element> Elements { get; set; } = new List<Element>();
            public List<RelationDto> Relations { get; set; } = new List<RelationDto>();
        }

        private sealed class RelationDto
        {
            public Guid Source { get; set; }
            public Guid Target { get; set; }
            public string Namespace { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string Type { get; set; } = string.Empty;
            public object? Value { get; set; }
        }
    }
}
