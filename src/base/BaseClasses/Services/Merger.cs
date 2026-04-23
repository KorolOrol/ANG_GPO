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
    }
}
