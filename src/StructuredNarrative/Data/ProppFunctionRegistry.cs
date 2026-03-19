using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using StructuredNarrative.Enum;
using StructuredNarrative.Model;

namespace StructuredNarrative.Data
{
    /// <summary>
    /// Реестр всех функций Проппа.
    /// </summary>
    public static class ProppFunctionRegistry
    {
        /// <summary>
        /// Словарь всех функций Проппа, сгруппированных по порядковому номеру.
        /// </summary>
        private static readonly Dictionary<int, List<ProppFunction>> _All = new Dictionary<int, List<ProppFunction>>();

        /// <summary>
        /// Список всех ролей, участвующих в функциях Проппа.
        /// </summary>
        private static readonly List<string> _Roles = new List<string>();

        /// <summary>
        /// Настройки десериализации JSON (camelCase).
        /// </summary>
        private static readonly JsonSerializerOptions _JsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    
        /// <summary>
        /// Все функция Проппа в канонической последовательности.
        /// </summary>
        public static IReadOnlyList<ProppFunction> All => _All
            .OrderBy(kvp => kvp.Key)
            .SelectMany(kvp => kvp.Value)
            .ToList();
    
        /// <summary>
        /// Все роли, участвующие в функциях Проппа (Hero, Villain, Helper и т.д.).
        /// </summary>
        public static IReadOnlyList<string> Roles => _Roles;

        /// <summary>
        /// Получить функции по порядковому номеру.
        /// </summary>
        public static List<ProppFunction> ByOrder(int order)
        {
            return _All.TryGetValue(order, out var value) ? value.ToList() : new List<ProppFunction>();
        }

        /// <summary>
        /// Получить функцию по символу.
        /// </summary>
        public static ProppFunction? BySymbol(string symbol)
        {
            return All.FirstOrDefault(f => f.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Получить все функции указанного акта.
        /// </summary>
        public static List<ProppFunction> ByPhase(NarrativePhase phase)
        {
            return All.Where(f => f.Phase == phase).ToList();
        }

        /// <summary>
        /// Получить все функции, в которых участвует указанная роль.
        /// </summary>
        public static List<ProppFunction> ByRole(string role)
        {
            return All.Where(f => f.PrimaryRoles.Contains(role) || f.SecondaryRoles.Contains(role)).ToList();
        }

        /// <summary>
        /// Загрузка данных о функциях Проппа из JSONL файла.
        /// </summary>
        public static void Load(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Файл с данными о функциях Проппа не найден: {path}");
            }

            var lines = File.ReadAllLines(path);
            var nonEmptyLines = lines.Where(l => !string.IsNullOrWhiteSpace(l)).ToArray();
            if (nonEmptyLines.Length == 0)
            {
                throw new InvalidDataException($"Файл с данными о функциях Проппа пуст или содержит только пустые строки: {path}");
            }

            var rolesLine = nonEmptyLines.First();
            var roles = JsonSerializer.Deserialize<List<string>>(rolesLine, _JsonOptions);
            if (roles != null)
            {
                foreach (var roleLine in roles.Where(roleLine => !_Roles.Contains(roleLine)))
                {
                    _Roles.Add(roleLine);
                }
            }
            foreach (var line in nonEmptyLines.Skip(1))
            {
                try
                {
                    var function = JsonSerializer.Deserialize<ProppFunction>(line, _JsonOptions);
                    if (function == null) continue;
                    if (BySymbol(function.Symbol) != null) continue;
                    if (!_All.ContainsKey(function.Order))
                    {
                        _All[function.Order] = new List<ProppFunction>();
                    }
                    _All[function.Order].Add(function);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при загрузке функции Проппа из строки: {line}");
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}