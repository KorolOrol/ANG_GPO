using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using BaseClasses.Interface;
using BaseClasses.Model.Params;

namespace BaseClasses.Services
{
    // TODO: CRITICAL ARCHITECTURE ISSUE - Significant code duplication with Serializer.cs
    // Both files contain identical implementations of:
    // 1. AliasTypes dictionary (lines 15-32 here, 22-39 in Serializer.cs)
    // 2. Assembly normalization regex patterns (lines 34-36 here, 41-43 in Serializer.cs)
    // 3. Type name resolution methods (GetFriendlyTypeName, GetFriendlyBaseName, ResolveTypeByName)
    // 4. FriendlyTypeNameParser nested class (lines 136-273 here, 474-611 in Serializer.cs)
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
    // Type resolution is called for every parameter during deserialization, without caching.
    // For ParamBag with many parameters, this causes repeated lookups.
    // RECOMMENDATION: Add ConcurrentDictionary<string, Type> cache in TypeResolutionHelper.
    public class ParamBagJsonConverter : JsonConverter<ParamBag>
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

        private static string NormalizeTypeName(string typeName)
        {
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

        private static bool TryResolveFriendlyType(string typeName, out Type? type)
        {
            var parser = new FriendlyTypeNameParser(typeName);
            return parser.TryParse(out type);
        }

        private static Type? ResolveType(string typeName)
        {
            return Type.GetType(typeName, ResolveAssembly, ResolveTypeName, throwOnError: false);
        }

        private static Type? ResolveTypeByName(string typeName)
        {
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

        private static Assembly? ResolveAssembly(AssemblyName name)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var assemblyName = assembly.GetName();
                if (string.Equals(assemblyName.Name, name.Name, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(assemblyName.FullName, name.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    return assembly;
                }
            }

            return null;
        }

        private static Type? ResolveTypeName(Assembly? assembly, string typeName, bool ignoreCase)
        {
            return assembly?.GetType(typeName, throwOnError: false, ignoreCase: ignoreCase);
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

                // TODO: CODE QUALITY - Type resolution logic duplicated with Serializer.cs
                // Multiple attempts to resolve type (direct, normalized, friendly)
                // Should be extracted into a single method: ResolveTypeFromName()
                var valueType = ResolveType(typeName);
                if (valueType == null)
                {
                    var normalizedTypeName = NormalizeTypeName(typeName);
                    valueType = ResolveType(normalizedTypeName);
                }
                if (valueType == null && TryResolveFriendlyType(typeName, out var friendlyType))
                {
                    valueType = friendlyType;
                }
                if (valueType == null)
                {
                    throw new JsonException($"Cannot resolve type '{typeName}'.");
                }

                // TODO: REFLECTION - Activator.CreateInstance is slow
                // Consider using delegates or Expression Trees for faster instantiation
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
            // TODO: OPTIMIZATION - GetFriendlyTypeName is called for every parameter
            // Consider caching type names in ParamKey<T> or in this converter
            // This would avoid repeated reflection-based type name generation
            writer.WriteStartArray();

            foreach (var (key, paramValue) in value.Enumerate())
            {
                writer.WriteStartObject();
                writer.WriteString("Namespace", key.Namespace);
                writer.WriteString("Name", key.Name);
                writer.WriteString("Type",
                    GetFriendlyTypeName(key.ValueType));
                writer.WritePropertyName("Value");
                // TODO: ERROR HANDLING - No exception handling for serialization errors
                // If JsonSerializer.Serialize fails, the error message could be more informative
                JsonSerializer.Serialize(writer, paramValue, key.ValueType, options);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
        }
    }
}
