using BaseClasses.Interface;
using BaseClasses.Model;

namespace BaseClasses.Services
{
    /// <summary>
    /// Сервис объединения элементов
    /// </summary>
    public static class Merger
    {
        /// <summary>
        /// Объединение двух элементов
        /// </summary>
        /// <param name="baseElement">Базовый элемент</param>
        /// <param name="mergedElement">Объединяемый элемент</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static void Merge(IElement baseElement, IElement mergedElement)
        {
            if (baseElement is null || mergedElement is null)
                throw new ArgumentNullException("Элемент не может быть null.");
            if (baseElement.Type != mergedElement.Type)
                throw new ArgumentException("Неверный тип элемента.");

            if (baseElement.Name == "")
            {
                baseElement.Name = mergedElement.Name;
            }
            if (baseElement.Description == "")
            {
                baseElement.Description = mergedElement.Description;
            }

            foreach (KeyValuePair<string, object> kvp in mergedElement.Params)
            {
                switch (kvp.Value) 
                {
                    case List<IElement> elements:
                        {
                            foreach (IElement element in elements.ToList())
                            {
                                Binder.Bind(baseElement, element);
                                Binder.Unbind(mergedElement, element);
                            }
                        }
                        break;
                    case IElement element:
                        {
                            Binder.Bind(baseElement, element);
                            Binder.Unbind(mergedElement, element);
                        }
                        break;
                    case List<Relation> relationships:
                        {
                            foreach(Relation relation in relationships.ToList())
                            {
                                Binder.Bind(baseElement, relation.Character, relation.Value);
                                Binder.Unbind(mergedElement, relation.Character);
                            }
                        }
                        break;
                    case List<object> values: 
                        {
                            if (!baseElement.Params.ContainsKey(kvp.Key))
                            {
                                baseElement.Params.Add(kvp.Key, values);
                                break;
                            }
                            foreach (var value in values.ToList())
                            {
                                if (!((List<object>)baseElement.Params[kvp.Key]).Contains(value))
                                    ((List<object>)baseElement.Params[kvp.Key]).Add(value);
                            }
                        }
                        break;
                    case object value:
                        {
                            if (!baseElement.Params.ContainsKey(kvp.Key))
                            {
                                baseElement.Params.Add(kvp.Key, value);
                            }
                            else if (baseElement.Params[kvp.Key] is null || 
                                (baseElement.Params[kvp.Key] is string s && 
                                string.IsNullOrWhiteSpace(s)))
                            {
                                baseElement.Params[kvp.Key] = value;
                            }
                        }
                        break;
                }
            }
            
            mergedElement.Time = Math.Max(baseElement.Time, mergedElement.Time);
        }
    }
}
