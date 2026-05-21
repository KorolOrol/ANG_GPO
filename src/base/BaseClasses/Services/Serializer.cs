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
using System.Text.RegularExpressions;

namespace BaseClasses.Services
{
    /// <summary>
    /// Сериализатор.
    /// </summary>
    // TODO: CRITICAL ARCHITECTURE ISSUE - Significant code duplication with ParamBagJsonConverter
    // Both files contain identical implementations of:
    // 1. AliasTypes dictionary (lines 22-39 here, 15-32 in ParamBagJsonConverter)
    // 2. Assembly normalization regex patterns (lines 41-43 here, 34-36 in ParamBagJsonConverter)
    // 3. Type name resolution methods (GetFriendlyTypeName, GetFriendlyBaseName, ResolveTypeByName)
    // 4. FriendlyTypeNameParser nested class (lines 474-611 here, 136-273 in ParamBagJsonConverter)
    // 
    // RECOMMENDATION: Extract all type resolution logic into a new static class 'TypeResolutionHelper'
    // This would:
    // - Eliminate duplication (DRY principle)
    // - Ensure consistency of type resolution
    // - Make changes to type handling affect both correctly
    // - Enable easier addition of caching
    // - Reduce maintenance burden
    //
    // TODO: PERFORMANCE - No caching of resolved types
    // ResolveTypeByName() iterates through ALL AppDomain.CurrentDomain.GetAssemblies() for each type.
    // For ParamBag with many parameters, this is O(n*m) where n=params and m=assemblies.
    // RECOMMENDATION: Add ConcurrentDictionary<string, Type> cache with cache invalidation strategy.
    //
    // TODO: REFLECTION OPTIMIZATION
    // Multiple calls to:
    // - GetType() with ignoreCase: true (slower than case-sensitive)
    // - Activator.CreateInstance() without optimization
    // - AppDomain assembly iteration (very slow for large domains)
    // Consider Expression Trees or delegate factories for performance-critical paths.
    public static class Serializer
    {
        private static readonly Dictionary<string, Type> AliasTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            ["string"] = typeof(string),
            ["bool"] = typeof(bool),
            ["byte"] = typeof(byte),
            ["sbyte"] = typeof(sbyte),
            ["short"] = typeof(short),
            ["ushort"] = typeof(ushort),
            ["int"] = typeof(int),
            ["uint"] = typeof(uint),
            ["long"] = typeof(long),
            ["ulong"] = typeof(ulong),
            ["float"] = typeof(float),
            ["double"] = typeof(double),
            ["decimal"] = typeof(decimal),
            ["char"] = typeof(char),
            ["object"] = typeof(object)
        };

        private static readonly Regex AssemblyVersionPart = new Regex(@",\s*Version=[^,\]]+", RegexOptions.Compiled);
        private static readonly Regex AssemblyCulturePart = new Regex(@",\s*Culture=[^,\]]+", RegexOptions.Compiled);
        private static readonly Regex AssemblyTokenPart = new Regex(@",\s*PublicKeyToken=[^,\]]+", RegexOptions.Compiled);

        private static readonly FieldInfo? _ElementIdField = typeof(Element).GetField("_id",
            BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// Настройки сериализации.
        /// </summary>
        public static JsonSerializerOptions Options { get; } = new JsonSerializerOptions
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
        /// <returns>JSON-представление связи между элементами истории.</returns>
        // TODO: TYPO FIX - Previous version had double '>' in XML comment tag
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
                    var items = param.Value as IEnumerable<object>;
                    value = items != null ? $"[{string.Join(", ", items)}]" : "null";
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
                Type = GetFriendlyTypeName(relation.Param.ValueType),
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
                CreateParamKey(dto.Name, dto.Namespace, ResolveType(dto.Type)),
                DeserializeTypedValue(dto.Value, ResolveType(dto.Type))
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

                var valueType = ResolveType(relationDto.Type);
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
            
            // TODO: REFLECTION PERFORMANCE - Using reflection to access private field on every deserialization
            // Consider:
            // 1. Making GetId() a public virtual method on Element
            // 2. Caching the FieldInfo at class initialization
            // 3. Creating a delegate for faster access than reflection
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
            // TODO: REFLECTION - Activator.CreateInstance is slow
            // Consider creating and caching delegates for common types, or using Expression Trees
            // This is called for every parameter during deserialization
            var keyType = typeof(ParamKey<>).MakeGenericType(valueType);
            return (IParamKey?)Activator.CreateInstance(keyType, name, @namespace)
                   ?? throw new JsonException($"Unable to create relation key '{@namespace}.{name}'.");
        }

        private static object? DeserializeTypedValue(object? valueToken, Type valueType)
        {
            // TODO: REFACTORING - This method could be simplified
            // The logic tries to handle JsonElement, null, and other objects
            // Consider extracting null handling to a separate step
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

        private static Type ResolveType(string typeName)
        {
            // TODO: PERFORMANCE - Multiple resolution attempts without early exit optimization
            // This method tries resolution 3 times:
            // 1. Direct ResolveType call
            // 2. After normalization
            // 3. As friendly type name
            // Consider consolidating these attempts and adding caching.
            var resolved = ResolveTypeByName(typeName)
                          ?? ResolveTypeByName(NormalizeTypeName(typeName));
            
            if (resolved == null && TryResolveFriendlyType(typeName, out var friendlyType))
            {
                resolved = friendlyType;
            }

            return resolved
                   ?? throw new JsonException($"Unable to resolve relation type '{typeName}'.");
        }

        private static string NormalizeTypeName(string typeName)
        {
            // TODO: OPTIMIZATION - Normalization regex replacements could be combined
            // Or could use a single regex pattern for all replacements at once
            var normalized = AssemblyVersionPart.Replace(typeName, string.Empty);
            normalized = AssemblyCulturePart.Replace(normalized, string.Empty);
            normalized = AssemblyTokenPart.Replace(normalized, string.Empty);
            return normalized;
        }

        private static string GetFriendlyTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                var definition = type.GetGenericTypeDefinition();
                var arguments = type.GetGenericArguments();
                var argsText = string.Join(", ", arguments.Select(GetFriendlyTypeName));
                return $"{GetFriendlyBaseName(definition)}<{argsText}>";
            }

            return GetFriendlyBaseName(type);
        }

        private static string GetFriendlyBaseName(Type type)
        {
            // TODO: OPTIMIZATION - Iteration through AliasTypes for every type lookup
            // Consider reversing this: build a lookup by type -> alias name at class init
            // Or use a switch expression with pattern matching for common types
            foreach (var (alias, aliasType) in AliasTypes)
            {
                if (aliasType == type)
                {
                    return alias;
                }
            }

            var name = type.Name;
            var genericTick = name.IndexOf('`');
            if (genericTick >= 0)
            {
                name = name[..genericTick];
            }

            if (string.Equals(type.Namespace, "System", StringComparison.Ordinal) ||
                string.Equals(type.Namespace, "System.Collections.Generic", StringComparison.Ordinal))
            {
                return name;
            }

            return type.FullName ?? name;
        }

        private static Type? ResolveTypeByName(string typeName)
        {
            // TODO: PERFORMANCE - This method is called frequently during deserialization
            // but performs expensive operations:
            // 1. Type.GetType() call (can be slow)
            // 2. Iteration through ALL AppDomain.CurrentDomain.GetAssemblies() (VERY slow)
            // 
            // RECOMMENDATION:
            // 1. Add caching with ConcurrentDictionary<string, Type>
            // 2. Cache results from each assembly for faster subsequent lookups
            // 3. Consider limiting assembly search to loaded application assemblies only
            // 4. Use ignoreCase: false by default for better performance
            var type = Type.GetType(typeName, throwOnError: false, ignoreCase: true);
            if (type != null)
            {
                return type;
            }

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName, throwOnError: false, ignoreCase: true);
                if (type != null)
                {
                    return type;
                }
            }

            return null;
        }

        private static bool TryResolveFriendlyType(string typeName, out Type? type)
        {
            var parser = new FriendlyTypeNameParser(typeName);
            return parser.TryParse(out type);
        }

        private sealed class FriendlyTypeNameParser
        {
            private readonly string _text;
            private int _pos;

            public FriendlyTypeNameParser(string text)
            {
                _text = text;
            }

            public bool TryParse(out Type? type)
            {
                type = ParseType();
                SkipWhitespace();
                return type != null && _pos == _text.Length;
            }

            private Type? ParseType()
            {
                SkipWhitespace();
                var name = ParseName();
                if (string.IsNullOrWhiteSpace(name))
                {
                    return null;
                }

                SkipWhitespace();
                if (TryConsume('<'))
                {
                    var arguments = new List<Type>();
                    do
                    {
                        var argumentType = ParseType();
                        if (argumentType == null)
                        {
                            return null;
                        }

                        arguments.Add(argumentType);
                        SkipWhitespace();
                    }
                    while (TryConsume(','));
                    
                    if (!TryConsume('>'))
                    {
                        return null;
                    }

                    return ResolveGenericType(name, arguments);
                }

                return ResolveNonGenericType(name);
            }

            private string? ParseName()
            {
                SkipWhitespace();
                var start = _pos;

                while (_pos < _text.Length)
                {
                    var current = _text[_pos];
                    if (char.IsLetterOrDigit(current) || current == '_' || current == '.' || current == '+')
                    {
                        _pos++;
                        continue;
                    }

                    break;
                }

                if (_pos == start)
                {
                    return null;
                }

                return _text[start.._pos];
            }

            private static Type? ResolveNonGenericType(string name)
            {
                if (AliasTypes.TryGetValue(name, out var aliasType))
                {
                    return aliasType;
                }

                var type = ResolveTypeByName(name);
                if (type != null)
                {
                    return type;
                }

                if (!name.Contains('.'))
                {
                    type = ResolveTypeByName($"System.{name}");
                    if (type != null)
                    {
                        return type;
                    }

                    return ResolveTypeByName($"System.Collections.Generic.{name}");
                }

                return null;
            }

            private static Type? ResolveGenericType(string name, IReadOnlyList<Type> arguments)
            {
                var definitionName = $"{name}`{arguments.Count}";
                var definition = ResolveTypeByName(definitionName);
                if (definition == null && !name.Contains('.'))
                {
                    definition = ResolveTypeByName($"System.Collections.Generic.{definitionName}");
                }

                return definition?.MakeGenericType(arguments.ToArray());
            }

            private void SkipWhitespace()
            {
                while (_pos < _text.Length && char.IsWhiteSpace(_text[_pos]))
                {
                    _pos++;
                }
            }

            private bool TryConsume(char expected)
            {
                SkipWhitespace();
                if (_pos < _text.Length && _text[_pos] == expected)
                {
                    _pos++;
                    return true;
                }

                return false;
            }
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
