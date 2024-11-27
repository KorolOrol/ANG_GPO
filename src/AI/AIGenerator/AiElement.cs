using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Enum;
using BaseClasses.Services;
using System.Text.Json;

namespace AIGenerator
{
    public class AiElement
    {
        public string Type { get; set; } = "";
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public Dictionary<string, object> Params { get; set; } = new Dictionary<string, object>();

        public AiElement() { }

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
                            switch (list.First())
                            {
                                case IElement:
                                    {
                                        List<string> strList = new List<string>();
                                        foreach (IElement e in list)
                                        {
                                            strList.Add(e.Name);
                                        }
                                        Params.Add(kvp.Key, strList);
                                    }
                                    break;
                                case Relation:
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
                                case string:
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
                                    newEl[ElemType.Character].Add(rel);
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
                                    newEl[(ElemType)Enum.Parse(typeof(ElemType), 
                                        kvp.Key.Substring(0, kvp.Key.Length - 1))].Add(name);
                                }
                            }
                        }
                        break;
                    case "Host":
                        {
                            if (plot.Elements.FirstOrDefault(e => e.Name == (string)kvp.Value) == null)
                            {
                                newEl[ElemType.Character].Add((string)kvp.Value);
                            }
                        }
                        break;
                    case "Location":
                        {
                            if (plot.Elements.FirstOrDefault(e => e.Name == (string)kvp.Value) == null)
                            {
                                newEl[ElemType.Location].Add((string)kvp.Value);
                            }
                        }
                        break;
                }
            }
            return newEl;
        }

        public void ParamsJsonToSystem()
        {
            foreach (var kvp in Params)
            {
                switch (((JsonElement)kvp.Value).ValueKind)
                {
                    case JsonValueKind.Array:
                        {
                            Params[kvp.Key] =
                                ((JsonElement)kvp.Value).Deserialize<List<string>>() ?? new();
                        }
                        break;
                    case JsonValueKind.Object:
                        {
                            Params[kvp.Key] =
                                ((JsonElement)kvp.Value).Deserialize<Dictionary<string, double>>() ?? new();
                        }
                        break;
                    case JsonValueKind.String:
                        {
                            Params[kvp.Key] = ((JsonElement)kvp.Value).GetString() ?? "";
                        }
                        break;
                }
            }
        }
    }
}
