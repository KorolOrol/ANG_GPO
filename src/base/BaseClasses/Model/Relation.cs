using System;
using BaseClasses.Interface;

namespace BaseClasses.Model
{
    /// <summary>
    /// Класс, представляющий отношение между элементами истории. Содержит информацию о том,
    /// от какого элемента к какому элементу идет отношение, какой параметр описывает это отношение
    /// и какое значение этому параметру соответствует.
    /// </summary>
    public class Relation : IEquatable<Relation>
    {
        private object? _value;

        /// <summary>
        /// Элемент, от которого исходит отношение.
        /// </summary>
        public IElement Source { get; }
        
        /// <summary>
        /// Элемент, к которому направлено отношение.
        /// </summary>
        public IElement Target { get; }
        
        /// <summary>
        /// Параметр, описывающий отношение между элементами.
        /// </summary>
        public IParamKey Param { get; }
        
        /// <summary>
        /// Значение параметра, описывающего отношение между элементами.
        /// </summary>
        public object? Value
        {
            get => _value;
            set
            {
                if (!Param.IsTypeMatch(value))
                {
                    throw new ArgumentException($"Value must be of type {Param.ValueType.FullName}.");
                }
                _value = value;
            }
        }
        
        /// <summary>
        /// Конструктор для создания нового отношения между элементами истории.
        /// </summary>
        /// <param name="source">Элемент, от которого исходит отношение.</param>
        /// <param name="target">Элемент, к которому направлено отношение.</param>
        /// <param name="param">Параметр, описывающий отношение между элементами.</param>
        /// <param name="value">Значение параметра, описывающего отношение между элементами.</param>
        public Relation(IElement source, IElement target, IParamKey param, object? value)
        {
            Source = source;
            Target = target;
            Param = param;
            Value = value;
        }
        
        public override string ToString()
        {
            return $"From {Source.Type}:{Source.Name} to {Target.Type}:{Target.Name} " +
                   $"with {Param.Namespace}.{Param.Name} = {Value}";
        }


        public bool Equals(Relation? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Source.Equals(other.Source) && Target.Equals(other.Target) && Param.Equals(other.Param);
        }

        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Relation)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Source, Target, Param);
        }
    }
}
