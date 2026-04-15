using System;
using BaseClasses.Interface;
using BaseClasses.Model;
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

            foreach (var r in plot.Relations
                         .Where(r => r.Source.Equals(mergedElement) || r.Target.Equals(mergedElement))
                         .ToList())
            {
                var other = r.Source.Equals(mergedElement) ? r.Target : r.Source;
                var param = r.Param;
                var value = r.Value;
                plot.Unbind(r.Source, r.Target, param);
                if (plot.Relations.Any(check =>
                        check.Param.Equals(param) &&
                        (check.Source.Equals(baseElement) || check.Target.Equals(baseElement)))
                    && basePriority)
                    continue;
                if (r.Source.Equals(mergedElement))
                    plot.Bind(baseElement, other, param, value);
                else
                    plot.Bind(other, baseElement, param, value);
            }

            baseElement.Time = Math.Max(baseElement.Time, mergedElement.Time);
        }
    }
}
