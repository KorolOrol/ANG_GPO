using StructuredNarrative.Enum;
using StructuredNarrative.Model;

namespace StructuredNarrative.Data;

/// <summary>
/// Реестр всех функций Проппа.
/// </summary>
public static class ProppFunctionRegistry
{
    private static readonly List<ProppFunction> _All = new List<ProppFunction>();

    /// <summary>
    /// Все функция Проппа в канонической последовательности
    /// </summary>
    public static IReadOnlyList<ProppFunction> All => _All.AsReadOnly();

    /// <summary>
    /// Получить функцию по порядковому номеру
    /// </summary>
    public static ProppFunction? ByOrder(int order)
    {
        return _All.FirstOrDefault(f => f.Order == order);
    }

    /// <summary>
    /// Получить функцию по символу
    /// </summary>
    public static ProppFunction? BySymbol(string symbol)
    {
        return _All.FirstOrDefault(f => f.Symbol == symbol);
    }

    /// <summary>
    /// Получить все функции указанного акта
    /// </summary>
    public static List<ProppFunction> ByPhase(NarrativePhase phase)
    {
        return _All.Where(f => f.Phase == phase).ToList();
    }

    /// <summary>
    /// Получить все функции, в которых участвует указанная роль
    /// </summary>
    public static List<ProppFunction> ByRole(ProppRoleType role)
    {
        return _All.Where(f =>
            f.PrimaryRoles.Contains(role) ||
            f.SecondaryRoles.Contains(role)).ToList();
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
        foreach (var line in lines)
        {
            try
            {
                var function = System.Text.Json.JsonSerializer.Deserialize<ProppFunction>(line);
                if (function != null) _All.Add(function);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке функции Проппа из строки: {line}");
                Console.WriteLine(ex.Message);
            }
        }
        _All.Sort((f1, f2) => f1.Order.CompareTo(f2.Order));
    }
}