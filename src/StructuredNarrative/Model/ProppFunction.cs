using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using BaseClasses.Enum;
using BaseClasses.Model;
using BaseClasses.Services.Binds;
using StructuredNarrative.Data;
using StructuredNarrative.Enum;

namespace StructuredNarrative.Model
{
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
        public int Order { get; }
    
        /// <summary>
        /// Символ функции по Проппу (α, β, γ, А, В, С и т.д.)
        /// </summary>
        public string Symbol { get; }

        /// <summary>
        /// Краткое название функции
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Развёрнутое описание функции
        /// </summary>
        public string Description { get; }
    
        /// <summary>
        /// Акт, к которому принадлежит функция
        /// </summary>
        public NarrativePhase Phase { get; }

        /// <summary>
        /// Является ли функция обязательной в последовательности
        /// (некоторые функции у Проппа факультативны)
        /// </summary>
        public bool IsOptional { get; }
    
        /// <summary>
        /// Роли, являющиеся главными действующими лицами данной функции.
        /// Например, для «Борьба» — Hero и Villain.
        /// </summary>
        public List<string> PrimaryRoles { get; }

        /// <summary>
        /// Роли, которые могут участвовать, но не обязательны.
        /// Например, Helper может присутствовать в «Борьбе».
        /// </summary>
        public List<string> SecondaryRoles { get; }

        /// <summary>
        /// Символы функций, которые должны присутствовать после данной функции.
        /// </summary>
        public List<string> RequiredNextFunctions { get; }
    
        /// <summary>
        /// Символы функций, которые должны присутствовать до данной функции.
        /// </summary>
        public List<string> RequiredPreviousFunctions { get; }
    
        /// <summary>
        /// Создаёт заготовку Element(ElemType.Event) для данной функции.
        /// Заготовка содержит метаданные функции в Params, но не имеет 
        /// конкретных имён и описаний — это скелет для заполнения ИИ или вручную.
        /// </summary>
        /// <returns>Элемент-событие с параметрами функции</returns>
        public Element CreateEventSkeleton(Plot plot)
        {
            var functionEvent = new Element(ElemType.Event, Name, Description);
            functionEvent.Params.Set(ProppParamKeys.Function, Name);
            functionEvent.Params.Set(ProppParamKeys.Symbol, Symbol);
            functionEvent.Params.Set(ProppParamKeys.NarrativePhase, Phase.ToString());
            functionEvent.Params.Set(ProppParamKeys.Order, Order);
            
            plot.Add(functionEvent);

            foreach (var roledElement in PrimaryRoles.Select(primaryRole => GetCharacterByRole(primaryRole,
                         plot,
                         true)!))
            {
                plot.Bind(functionEvent, roledElement, BaseRelationKeys.Involves, true);
            }

            foreach (var roledElement in SecondaryRoles.Select(secondaryRole => GetCharacterByRole(secondaryRole,
                             plot,
                             false))
                         .OfType<Element>())
            {
                plot.Bind(functionEvent, roledElement, BaseRelationKeys.Involves, false);
            }

            return functionEvent;
        }
        
        private static Element? GetCharacterByRole(string role, Plot plot, bool createIfMissing)
        {
            var element = plot.Elements.FirstOrDefault(c => c is { Type: ElemType.Character } &&
                                                            c.Params.TryGet(ProppParamKeys.Role, out var existingRole) &&
                                                            existingRole == role) as Element;

            if (element != null || !createIfMissing)
            {
                return element;
            }

            element = new Element(ElemType.Character, role, $"Персонаж в роли {role}")
            {
                Params =
                {
                    [ProppParamKeys.Role] = role
                }
            };

            plot.Add(element);
            return element;
        }

        /// <summary>
        /// Создание функции Проппа
        /// </summary>
        /// <param name="order">Порядковый номер (0-30)</param>
        /// <param name="symbol">Символ по Проппу</param>
        /// <param name="name">Название</param>
        /// <param name="description">Описание</param>
        /// <param name="phase">Акт</param>
        /// <param name="isOptional">Факультативность функции</param>
        /// <param name="primaryRoles">Главные участвующие роли</param>
        /// <param name="secondaryRoles">Необязательные участвующие роли</param>
        /// <param name="requiredNextFunctions">Символы функций, которые должны следовать после данной</param>
        /// <param name="requiredPreviousFunctions">Символы функций, которые должны предшествовать данной</param>
        [JsonConstructor]
        public ProppFunction(
            int order,
            string symbol,
            string name,
            string description,
            NarrativePhase phase,
            bool isOptional,
            List<string>? primaryRoles = null,
            List<string>? secondaryRoles = null,
            List<string>? requiredNextFunctions = null,
            List<string>? requiredPreviousFunctions = null
        )
        {
            Order = order;
            Symbol = symbol;
            Name = name;
            Description = description;
            Phase = phase;
            IsOptional = isOptional;
            PrimaryRoles = primaryRoles ?? new List<string>();
            SecondaryRoles = secondaryRoles ?? new List<string>();
            RequiredNextFunctions = requiredNextFunctions ?? new List<string>();
            RequiredPreviousFunctions = requiredPreviousFunctions ?? new List<string>();
        }
    
        public override string ToString()
        {
            return $"[{Symbol}] {Name} (Акт {Phase}, #{Order})";
        }
    }
}