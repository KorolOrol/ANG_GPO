using System;
using System.Collections.Generic;
using System.Linq;
using BaseClasses.Interface;
using BaseClasses.Services;

namespace BaseClasses.Model
{
    /// <summary>
    /// История.
    /// </summary>
    public class Plot
    {
        /// <summary>
        /// Элементы истории.
        /// </summary>
        public HashSet<IElement> Elements { get; } = new HashSet<IElement>();
        
        /// <summary>
        /// Связи между элементами истории.
        /// </summary>
        public HashSet<Relation> Relations { get; } = new HashSet<Relation>();
        
        /// <summary>
        /// Стратегия связывания элементов истории.
        /// </summary>
        public Binder Binder { get; } = new Binder();

        /// <summary>
        /// Время.
        /// </summary>
        public int Time { get; set; }

        /// <summary>
        /// Добавление элемента в историю.
        /// </summary>
        /// <param name="element">Элемент.</param>
        public void Add(IElement element) 
        {
            if (Elements.Contains(element)) return;
            if (element.Time == -1) element.Time = Time++;
            Elements.Add(element); 
        }

        /// <summary>
        /// Удаление элемента из истории.
        /// </summary>
        /// <param name="element">Удаляемый элемент.</param>
        public void Remove(IElement element)
        {
            if (!Elements.Contains(element)) return;
            foreach (var r in Relations.Where(r => r.Source.Equals(element) 
                                                   || r.Target.Equals(element)).ToList())
            {
                Binder.Unbind(r.Source, r.Target, r.Param, this);
            }
            Elements.Remove(element);
        }

        /// <summary>
        /// Связывание элементов истории через параметр.
        /// </summary>
        /// <param name="source">Источник связи.</param>
        /// <param name="target">Приемник связи.</param>
        /// <param name="paramKey">Параметр, описывающий связь.</param>
        /// <param name="value">Значение параметра, описывающее связь.</param>
        /// <exception cref="KeyNotFoundException">Выбрасывается, если источник или приемник связи
        /// не являются частью истории.</exception>
        /// <exception cref="ArgumentException">Выбрасывается, если значение не соответствует типу,
        /// определенному в paramKey.</exception>
        public void Bind(IElement source, IElement target, IParamKey paramKey, object? value)
        {
            if (!Elements.Contains(source)) 
                throw new KeyNotFoundException($"Source elements ({source}) must be part of the plot.");
            if (!Elements.Contains(target))
                throw new KeyNotFoundException($"Target elements ({target}) must be part of the plot.");
            if (!paramKey.IsTypeMatch(value))
                throw new ArgumentException($"Value {value} does not match the type defined in paramKey {paramKey}.");
            Binder.Bind(source, target, paramKey, value, this);
        }

        /// <summary>
        /// Удаление связи между элементами истории, описанной параметром.
        /// </summary>
        /// <param name="source">Источник связи.</param>
        /// <param name="target">Приемник связи.</param>
        /// <param name="paramKey">Параметр, описывающий связь.</param>
        /// <exception cref="KeyNotFoundException">Выбрасывается, если источник или приемник
        /// связи не являются частью истории.</exception>
        public void Unbind(IElement source, IElement target, IParamKey paramKey)
        {
            if (!Elements.Contains(source))
                throw new KeyNotFoundException($"Source elements ({source}) must be part of the plot.");
            if (!Elements.Contains(target))
                throw new KeyNotFoundException($"Target elements ({target}) must be part of the plot.");
            Binder.Unbind(source, target, paramKey, this);
        }
    }
}
