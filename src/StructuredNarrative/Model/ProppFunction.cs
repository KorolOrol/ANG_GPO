using BaseClasses.Enum;
using BaseClasses.Model;
using BaseClasses.Services;
using StructuredNarrative.Enum;

namespace StructuredNarrative.Model;

/// <summary>
/// Функция действующего лица по В.Я. Проппу.
/// Хранит все данные о функции: акт, участвующие роли,
/// связи с другими функциями и заготовку события.
/// </summary>
public class ProppFunction
{
    /// <summary>
    /// Порядковый номер функции в канонической последовательности Проппа (0-31)
    /// </summary>
    public int Order { get; init; }
    
    /// <summary>
    /// Символ функции по Проппу (α, β, γ, А, В, С и т.д.)
    /// </summary>
    public string Symbol { get; init; }

    /// <summary>
    /// Краткое название функции
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// Развёрнутое описание функции
    /// </summary>
    public string Description { get; init; }
    
    /// <summary>
    /// Акт, к которому принадлежит функция
    /// </summary>
    public NarrativePhase Phase { get; init; }

    /// <summary>
    /// Является ли функция обязательной в последовательности
    /// (некоторые функции у Проппа факультативны)
    /// </summary>
    public bool IsOptional { get; init; }

    /// <summary>
    /// Порядковые номера функций, которые должны присутствовать, если присутствует данная функция.
    /// </summary>
    public List<int> DependentFunctions { get; init; }
    
    /// <summary>
    /// Роли, являющиеся главными действующими лицами данной функции.
    /// Например, для «Борьба» — Hero и Villain.
    /// </summary>
    public List<ProppRoleType> PrimaryRoles { get; init; }

    /// <summary>
    /// Роли, которые могут участвовать, но не обязательны.
    /// Например, Helper может присутствовать в «Борьбе».
    /// </summary>
    public List<ProppRoleType> SecondaryRoles { get; init; }
    
    /// <summary>
    /// Создаёт заготовку Element(ElemType.Event) для данной функции.
    /// Заготовка содержит метаданные функции в Params, но не имеет 
    /// конкретных имён и описаний — это скелет для заполнения ИИ или вручную.
    /// </summary>
    /// <returns>Элемент-событие с параметрами функции</returns>
    public Element CreateEventSkeleton(Plot plot)
    {
        var @params = new Dictionary<string, object>
        {
            { "ProppFunction", Name },
            { "ProppSymbol", Symbol },
            { "NarrativePhase", Phase.ToString() },
            { "FunctionOrder", Order },
            { "Characters", new List<BaseClasses.Interface.IElement>() },
            { "Items", new List<BaseClasses.Interface.IElement>() },
            { "Locations", new List<BaseClasses.Interface.IElement>() }
        };

        var functionEvent = new Element(ElemType.Event, Name, Description, @params);

        foreach (var primaryRole in PrimaryRoles)
        {
            var roledElement = 
                plot.Characters.FirstOrDefault(c => c != null
                                                    && c.Params.ContainsKey("ProppRole")
                                                    && c.Params["ProppRole"].ToString() == primaryRole.ToString(),
                null) 
                ?? new Element(ElemType.Character, 
                    primaryRole.ToString(), 
                    $"Персонаж в роли {primaryRole}", 
                    new Dictionary<string, object>
            {
                { "ProppRole", primaryRole.ToString() }
            });
            Binder.Bind(functionEvent, roledElement);
            plot.Add(roledElement);
        }

        foreach (var secondaryRole in SecondaryRoles)
        {
            var roledElement =
                plot.Characters.FirstOrDefault(c => c != null 
                                                    && c.Params.ContainsKey("ProppRole")
                                                    && c.Params["ProppRole"].ToString() == secondaryRole.ToString(),
                    null);
            if (roledElement == null) continue;
            Binder.Bind(functionEvent, roledElement);
        }

        plot.Add(functionEvent);
        return functionEvent;
    }

    /// <summary>
    /// Создание функции Проппа
    /// </summary>
    /// <param name="order">Порядковый номер (0-30)</param>
    /// <param name="symbol">Символ по Проппу</param>
    /// <param name="name">Название</param>
    /// <param name="description">Описание</param>
    /// <param name="dependentFunctions">Порядковые номера функций, от которых зависит данная функция</param>
    /// <param name="phase">Акт</param>
    /// <param name="isOptional">Факультативность функции</param>
    /// <param name="primaryRoles">Главные участвующие роли</param>
    /// <param name="secondaryRoles">Необязательные участвующие роли</param>
    public ProppFunction(
        int order,
        string symbol,
        string name,
        string description,
        bool isOptional,
        List<int> dependentFunctions,
        NarrativePhase phase,
        List<ProppRoleType> primaryRoles,
        List<ProppRoleType>? secondaryRoles = null
        )
    {
        Order = order;
        Symbol = symbol;
        Name = name;
        Description = description;
        Phase = phase;
        IsOptional = isOptional;
        DependentFunctions = dependentFunctions;
        PrimaryRoles = primaryRoles;
        SecondaryRoles = secondaryRoles ?? new List<ProppRoleType>();
    }
    
    public override string ToString()
    {
        return $"[{Symbol}] {Name} (Акт {Phase}, #{Order})";
    }
}