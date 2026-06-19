using System;
using System.Collections;
using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Model.Params;
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
        /// <param name="plot">История, в которой находятся элементы</param>
        /// <param name="basePriority">Приоритет базового элемента</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static void Merge(IElement baseElement, IElement mergedElement, Plot plot, bool basePriority = true)
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

            foreach (var kvp in mergedElement.Params.Enumerate())
            {
                var param = kvp.Key;
                var value = kvp.Value;

                if (!baseElement.Params.ContainsKey(param))
                {
                    baseElement.Params.Add(param, value);
                    continue;
                }

                if (param.IsCollection)
                {
                    MergeCollectionParam(baseElement.Params, param, value);
                    continue;
                }

                if (!basePriority)
                {
                    baseElement.Params.Set(param, value);
                }
            }

            var relationsToMove = plot.Relations
                .Where(r => r.Source.Equals(mergedElement) || r.Target.Equals(mergedElement))
                .ToList();

            foreach (var relation in relationsToMove)
            {
                // Удаляем старую связь с mergedElement и затем пересоздаем ее на baseElement.
                plot.Relations.Remove(relation);

                var newSource = relation.Source.Equals(mergedElement) ? baseElement : relation.Source;
                var newTarget = relation.Target.Equals(mergedElement) ? baseElement : relation.Target;

                var conflict = plot.Relations.FirstOrDefault(existing =>
                    existing.Source.Equals(newSource) &&
                    existing.Target.Equals(newTarget) &&
                    existing.Param.Equals(relation.Param));

                if (conflict is null)
                {
                    plot.Relations.Add(new Relation(newSource, newTarget, relation.Param, relation.Value));
                    continue;
                }

                if (!basePriority)
                {
                    conflict.Value = relation.Value;
                }
            }

            baseElement.Time = Math.Max(baseElement.Time, mergedElement.Time);
        }

        private static void MergeCollectionParam(ParamBag bag, IParamKey key, object? mergedValue)
        {
            if (mergedValue is null) return;

            bag.TryGetValue(key, out var baseValue);
            if (baseValue is null)
            {
                bag.Set(key, mergedValue);
                return;
            }

            if (!TryGetEnumerable(baseValue, out var baseEnumerable) ||
                !TryGetEnumerable(mergedValue, out var mergedEnumerable))
            {
                bag.Set(key, mergedValue);
                return;
            }

            bool useExisting;
            IList target;
            if (baseValue is IList existingList && baseValue.GetType() == key.ValueType)
            {
                useExisting = true;
                target = existingList;
            }
            else
            {
                useExisting = false;
                target = (IList)Activator.CreateInstance(key.ValueType)!;
            }

            if (!useExisting)
            {
                foreach (var item in baseEnumerable)
                {
                    target.Add(item);
                }
            }

            foreach (var item in mergedEnumerable)
            {
                if (!ContainsItem(target, item))
                {
                    target.Add(item);
                }
            }

            if (!useExisting)
            {
                bag.Set(key, target);
            }
        }

        private static bool TryGetEnumerable(object value, out IEnumerable enumerable)
        {
            if (value is string)
            {
                enumerable = null!;
                return false;
            }

            if (value is IEnumerable found)
            {
                enumerable = found;
                return true;
            }

            enumerable = null!;
            return false;
        }

        private static bool ContainsItem(IList list, object? value)
        {
            foreach (var item in list)
            {
                if (Equals(item, value))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
