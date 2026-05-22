using System;
using System.Collections.Generic;
using System.Linq;

namespace BaseClasses.Services
{
    /// <summary>
    /// Сервис для получения строкового представления типа и разрешения типа по строковому имени.
    /// Поддерживает базовые типы и некоторые обобщенные типы (например, List&lt;T&gt;, Dictionary&lt;TKey, TValue&gt;).
    /// </summary>
    public static class TypeNameHelper
    {
        private static readonly Dictionary<Type, string> _Aliases = new Dictionary<Type, string>()
        {
            [typeof(bool)] = "bool",
            [typeof(byte)] = "byte",
            [typeof(sbyte)] = "sbyte",
            [typeof(short)] = "short",
            [typeof(ushort)] = "ushort",
            [typeof(int)] = "int",
            [typeof(uint)] = "uint",
            [typeof(long)] = "long",
            [typeof(ulong)] = "ulong",
            [typeof(float)] = "float",
            [typeof(double)] = "double",
            [typeof(decimal)] = "decimal",
            [typeof(string)] = "string",
            [typeof(char)] = "char",
            [typeof(DateTime)] = "datetime",
            [typeof(Guid)] = "guid"
        };
        
        private static readonly Dictionary<string, Type> _ReverseAliases = 
            _Aliases.ToDictionary(x => x.Value, x => x.Key);

        /// <summary>
        /// Получает строковое представление типа, используя алиасы для базовых типов и поддерживая обобщенные типы.
        /// </summary>
        /// <param name="type">Тип для получения имени.</param>
        /// <returns>Строковое представление типа.</returns>
        public static string GetTypeName(Type type)
        {
            if (_Aliases.TryGetValue(type, out string? alias)) return alias;

            if (!type.IsGenericType) return type.FullName ?? type.Name;
            
            var genericDef = type.GetGenericTypeDefinition();
            string genericName = genericDef.Name;
            genericName = genericName[..genericName.IndexOf('`')];
            var args = type.GetGenericArguments().Select(GetTypeName);
            return $"{genericName}<{string.Join(", ", args)}>";

        }

        /// <summary>
        /// Разрешает тип по строковому имени, поддерживая алиасы для базовых типов и обобщенные типы
        /// в формате "GenericType&lt;Arg1, Arg2&gt;".
        /// </summary>
        /// <param name="typeName">Строковое имя типа для разрешения.</param>
        /// <returns>>Разрешенный тип.</returns>
        /// <exception cref="TypeLoadException">Выбрасывается, если тип не найден или формат обобщенного типа неверный.</exception>
        public static Type ResolveType(string typeName)
        {
            if (_ReverseAliases.TryGetValue(typeName, out var aliasType)) return aliasType;
            if (typeName.Contains('<')) return ParseGenericType(typeName);
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .Select(a => a.GetType(typeName))
                .FirstOrDefault(t => t != null);
            return type ?? throw new TypeLoadException($"Type '{typeName}' not found.");
        }

        private static Type ParseGenericType(string text)
        {
            int genericStart = text.IndexOf('<');
            string genericName = text[..genericStart];
            string argsText = text[(genericStart + 1)..^1];
            var argNames = SplitGenericArguments(argsText);
            var argTypes = argNames.Select(ResolveType).ToArray();
            var genericType = genericName switch
            {
                "List" => typeof(List<>),
                "Dictionary" => typeof(Dictionary<,>),
                _ => throw new TypeLoadException($"Generic type '{genericName}' not supported.")
            };
            return genericType.MakeGenericType(argTypes);
        }

        private static List<string> SplitGenericArguments(string text)
        {
            var result = new List<string>();

            int depth = 0;
            int start = 0;

            for (int i = 0; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '<':
                        depth++;
                        break;
                    case '>':
                        depth--;
                        break;
                    case ',' when depth == 0:
                        result.Add(text[start..i]);
                        start = i + 1;
                        break;
                }
            }
            result.Add(text[start..]);
            return result.Select(x => x.Trim()).ToList();
        }
    }
}