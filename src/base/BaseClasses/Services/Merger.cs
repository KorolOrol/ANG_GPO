using System;
using BaseClasses.Interface;
using BaseClasses.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
        /// <param name="basePriority">Приоритет базового элемента</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static void Merge(IElement baseElement, IElement mergedElement, bool basePriority = true)
        {
            if (baseElement is null)
                throw new ArgumentNullException(nameof(baseElement), "Элемент не может быть null.");
            if (mergedElement is null)
                throw new ArgumentNullException(nameof(mergedElement), "Элемент не может быть null.");
            if (baseElement.Type != mergedElement.Type)
                throw new ArgumentException("Неверный тип элемента.", nameof(mergedElement));

            if (baseElement.Name == "" || !basePriority)
            {
                baseElement.Name = mergedElement.Name;
            }
            if (baseElement.Description == "" || !basePriority)
            {
                baseElement.Description = mergedElement.Description;
            }

            foreach (KeyValuePair<string, object> kvp in 
                mergedElement.Params.ToDictionary(pair => pair.Key, pair => pair.Value))
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
                            foreach (Relation relation in relationships.ToList())
                            {
                                Binder.Bind(baseElement, relation.Character, relation.Value);
                                Binder.Unbind(mergedElement, relation.Character);
                            }
                        }
                        break;
                    case IList values:
                        {
                            if (baseElement.Params.TryAdd(kvp.Key, values))
                            {
                                break;
                            }
                            foreach (var value in values.Cast<object>().ToList())
                            {
                                if (!((IList)baseElement.Params[kvp.Key]).Contains(value))
                                    ((IList)baseElement.Params[kvp.Key]).Add(value);
                            }
                        }
                        break;
                    case object value:
                        {
                            if (baseElement.Params.TryAdd(kvp.Key, value))
                            {
                            }
                            else if (baseElement.Params[kvp.Key] is null ||
                                (baseElement.Params[kvp.Key] is string s &&
                                string.IsNullOrWhiteSpace(s)) ||
                                !basePriority)
                            {
                                baseElement.Params[kvp.Key] = value;
                            }
                        }
                        break;
                }
            }

            baseElement.Time = Math.Max(baseElement.Time, mergedElement.Time);
        }
    }
}
