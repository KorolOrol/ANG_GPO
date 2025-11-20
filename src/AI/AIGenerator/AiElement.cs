using System;
using System.Collections.Generic;
using System.Linq;
using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Enum;
using BaseClasses.Services;
using System.Text.Json;

namespace AIGenerator
{
    /// <summary>
    /// Класс для представления элемента сюжета в формате, удобном для AI
    /// </summary>
    public class AiElement
    {
        /// <summary>
        /// Тип элемента (Character, Item, Location, Event)
        /// </summary>
        public string Type { get; set; } = "";
        
        /// <summary>
        /// Имя элемента
        /// </summary>
        public string Name { get; set; } = "";
        
        /// <summary>
        /// Описание элемента
        /// </summary>
        public string Description { get; set; } = "";
        
        /// <summary>
        /// Параметры элемента
        /// </summary>
        public Dictionary<string, object> Params { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public AiElement() { }

        /// <summary>
        /// Конструктор, принимающий IElement и преобразующий его в AiElement
        /// </summary>
        /// <param name="element">Элемент сюжета</param>
        public AiElement(IElement element)
        {
            Type = element.Type.ToString();
            Name = element.Name;
            Description = element.Description;
            foreach (KeyValuePair<string, object> kvp in element.Params)
            {
                switch (kvp.Value)
                {
                    case List<object> list:
                        {
                            if (list.Count == 0) break;
                            switch (list.First())
                            {
                                case IElement _:
                                    {
                                        List<string> strList = new List<string>();
                                        foreach (IElement e in list)
                                        {
                                            strList.Add(e.Name);
                                        }
                                        Params.Add(kvp.Key, strList);
                                    }
                                    break;
                                case Relation _:
                                    {
                                        Dictionary<string, double> dict = 
                                            new Dictionary<string, double>();
                                        foreach (Relation r in list)
                                        {
                                            dict.Add(r.Character.Name, r.Value);
                                        }
                                        Params.Add(kvp.Key, dict);
                                    }
                                    break;
                                case string _:
                                    {
                                        Params.Add(kvp.Key, list);
                                    }
                                    break;
                            }

                        }
                        break;
                    case IElement el:
                        {
                            Params.Add(kvp.Key, el.Name);
                        }
                        break;
                    default:
                        {
                            Params.Add(kvp.Key, kvp.Value);
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Преобразование AiElement обратно в IElement с учетом существующих элементов сюжета
        /// </summary>
        /// <param name="plot">Сюжет, в который добавляется элемент</param>
        /// <returns>Созданный элемент сюжета</returns>
        public IElement Element(Plot plot)
        {
            Element element = new Element((ElemType)Enum.Parse(typeof(ElemType), Type), 
                                          Name, Description);
            var par = element.Params;
            foreach (KeyValuePair<string, object> kvp in Params)
            {
                switch (kvp.Key)
                {
                    case "Relations":
                        {
                            if (element.Type != ElemType.Character || plot.Characters.Count == 0) 
                                break;
                            foreach (KeyValuePair<string, double> rel in 
                                (Dictionary<string, double>)kvp.Value)
                            {
                                IElement? foundCharacter = 
                                    plot.Characters.FirstOrDefault(c => c.Name == rel.Key);
                                if (foundCharacter != null)
                                {
                                    Binder.Bind(element, foundCharacter, rel.Value);
                                }
                            }
                        }
                        break;
                    case "Characters":
                    case "Items":
                    case "Locations":
                    case "Events":
                        {
                            foreach (string name in (List<string>)kvp.Value)
                            {
                                IElement? foundElement = plot.Elements.FirstOrDefault(e => e.Name == name);
                                if (foundElement != null)
                                {
                                    Binder.Bind(element, foundElement);
                                }
                            }
                        }
                        break;
                    case "Host":
                    case "Location":
                        {
                            if (element.Type != ElemType.Item) break;
                            IElement? foundElement = plot.Elements.FirstOrDefault(
                                element => element.Name == kvp.Value.ToString());
                            if (foundElement != null)
                            {
                                Binder.Bind(element, foundElement);
                            }
                        }
                        break;
                    default:
                        {
                            par.Add(kvp.Key, kvp.Value);
                        }
                        break;
                }
            }
            return element;
        }
        
        /// <summary>
        /// Словарь для определения типа элемента по имени параметра
        /// </summary>
        private readonly static Dictionary<string, ElemType> _elementTypes = new Dictionary<string, ElemType>()
        {
            { "Relations", ElemType.Character },
            { "Host", ElemType.Character },
            { "Characters", ElemType.Character },
            { "Items", ElemType.Item },
            { "Locations", ElemType.Location },
            { "Location", ElemType.Location },
            { "Events", ElemType.Event }
        };

        /// <summary>
        /// Определение новых элементов, которые необходимо создать в сюжете
        /// на основе параметров AiElement, если они отсутствуют в текущем сюжете
        /// </summary>
        /// <param name="plot">Сюжет, в который добавляются элементы</param>
        /// <returns>Словарь новых элементов по типам</returns>
        public Dictionary<ElemType, List<string>> NewElements(Plot plot)
        {
            Dictionary<ElemType, List<string>> newEl = new Dictionary<ElemType, List<string>>
            {
                { ElemType.Character, new List<string>() },
                { ElemType.Item, new List<string>() },
                { ElemType.Location, new List<string>() },
                { ElemType.Event, new List<string>() }
            };
            foreach (KeyValuePair<string, object> kvp in Params)
            {
                switch (kvp.Key)
                {
                    case "Relations":
                        {
                            foreach (string rel in ((Dictionary<string, double>)kvp.Value).Keys)
                            {
                                if (plot.Characters.FirstOrDefault(c => c.Name == rel) == null)
                                {
                                    newEl[_elementTypes[kvp.Key]].Add(rel);
                                }
                            }
                        }
                        break;
                    case "Characters":
                    case "Items":
                    case "Locations":
                    case "Events":
                        {
                            foreach (string name in (List<string>)kvp.Value)
                            {
                                if (plot.Elements.FirstOrDefault(e => e.Name == name) == null)
                                {
                                    newEl[_elementTypes[kvp.Key]].Add(name);
                                }
                            }
                        }
                        break;
                    case "Host":
                        {
                            if (plot.Elements.FirstOrDefault(e => e.Name == (string)kvp.Value) == null)
                            {
                                newEl[_elementTypes[kvp.Key]].Add((string)kvp.Value);
                            }
                        }
                        break;
                    case "Location":
                        {
                            if (plot.Elements.FirstOrDefault(e => e.Name == (string)kvp.Value) == null)
                            {
                                newEl[_elementTypes[kvp.Key]].Add((string)kvp.Value);
                            }
                        }
                        break;
                }
            }
            return newEl;
        }
        
        /// <summary>
        /// Преобразование параметров из JSON-элементов обратно в соответствующие типы
        /// </summary>
        public void ParamsJsonToSystem()
        {
            foreach (var kvp in Params.ToDictionary(k => k.Key, v => v.Value))
            {
                if (kvp.Value is JsonElement jsonElement)
                {
                    switch (jsonElement.ValueKind)
                    {
                        case JsonValueKind.Array:
                            {
                                Params[kvp.Key] =
                                    jsonElement.Deserialize<List<string>>() ?? new List<string>();
                            }
                            break;
                        case JsonValueKind.Object:
                            {
                                Params[kvp.Key] =
                                    jsonElement.Deserialize<Dictionary<string, double>>() ?? new Dictionary<string, double>();
                            }
                            break;
                        case JsonValueKind.String:
                            {
                                Params[kvp.Key] = jsonElement.GetString() ?? "";
                            }
                            break;
                    }
                }
            }
        }
    }
}
