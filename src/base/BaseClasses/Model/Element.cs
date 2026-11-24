using System;
using BaseClasses.Enum;
using BaseClasses.Interface;
using System.Linq;
using BaseClasses.Model.Params;

namespace BaseClasses.Model
{
    /// <summary>
    /// Элемент истории.
    /// </summary>
    public class Element : IElement, IEquatable<Element>
    {
        private readonly Guid _id = Guid.NewGuid();
        
        /// <summary>
        /// Тип элемента.
        /// </summary>
        public ElemType Type { get; }

        /// <summary>
        /// Название элемента.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание элемента.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Типизированные параметры элемента.
        /// </summary>
        public ParamBag Params { get; }

        /// <summary>
        /// Время создания элемента.
        /// </summary>
        public int Time { get; set; }

        /// <summary>
        /// Конструктор элемента.
        /// </summary>
        /// <param name="type">Тип элемента.</param>
        /// <param name="name">Название элемента.</param>
        /// <param name="description">Описание элемента.</param>
        /// <param name="time">Время создания элемента.</param>
        public Element(ElemType type, string name = "", string description = "", int time = -1)
        {
            Type = type;
            Name = name;
            Description = description;
            Params = new ParamBag();
            Time = time;
        }

        public override string ToString()
        {
            return $"{Type}: {Name}";
        }

        /// <summary>
        /// Проверка на пустоту элемента.
        /// </summary>
        /// <returns>True, если элемент пуст, иначе False.</returns>
        public bool IsEmpty()
        {
            return Name == "" && Description == "" &&
                   (Params.Count == 0 || Params.Keys.All(k => Params[k] == null)
                   ) && Time == -1;
        }

        public override bool Equals(object? obj)
        {
            return obj?.GetType() == GetType() && Equals((Element)obj);
        }

        public bool Equals(Element? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Type == other.Type &&
                   Name == other.Name &&
                   Description == other.Description &&
                   Time == other.Time;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_id);
        }
    }
}
