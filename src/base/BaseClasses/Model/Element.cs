using System;
using BaseClasses.Enum;
using BaseClasses.Interface;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BaseClasses.Model.Params;

namespace BaseClasses.Model
{
    /// <summary>
    /// Элемент истории
    /// </summary>
    public class Element : IElement, IEquatable<Element>
    {
        [Obsolete("Используйте TypedParams вместо Params для новых разработок.")]
        private IDictionary<string, object> _params = null!;

        /// <summary>
        /// Тип элемента
        /// </summary>
        public ElemType Type { get; }

        /// <summary>
        /// Название элемента
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание элемента
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Типизированные параметры элемента
        /// </summary>
        public ParamBag TypedParams { get; }

        /// <summary>
        /// Параметры элемента
        /// </summary>
        [Obsolete("Используйте TypedParams вместо Params для новых разработок.")]
        public IDictionary<string, object> Params
        {
            get => _params;
            set
            {
                _params = value as LegacyParamsDictionary ?? new LegacyParamsDictionary(TypedParams, value);
            }
        }

        /// <summary>
        /// Время создания элемента
        /// </summary>
        public int Time { get; set; }

        /// <summary>
        /// Конструктор элемента
        /// </summary>
        /// <param name="type">Тип элемента</param>
        /// <param name="name">Название элемента</param>
        /// <param name="description">Описание элемента</param>
        /// <param name="params">Параметры элемента</param>
        /// <param name="time">Время создания элемента</param>
        public Element(ElemType type, string name = "", string description = "",
                       IDictionary<string, object>? @params = null, int time = -1)
        {
            Type = type;
            Name = name;
            Description = description;
            TypedParams = new ParamBag();
            Params = new LegacyParamsDictionary(TypedParams, @params);
            Time = time;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Type}: {Name}";
        }

        /// <summary>
        /// Полная информация об элементе
        /// </summary>
        /// <returns>Полная информация об элементе</returns>
        public string FullInfo()
        {
            return $"{Type}: {Name}\n" +
                   $"Description: {Description}\n" +
                   $"{string.Join("\n", Params.Select(kvp => $"{kvp.Key}: {GetValueString(kvp.Value)}"))}\n" +
                   $"Creation time: {Time}\n";
        }

        /// <summary>
        /// Проверка на пустоту элемента
        /// </summary>
        /// <returns>True, если элемент пуст, иначе False</returns>
        public bool IsEmpty()
        {
            return Name == "" && Description == "" &&
                   (Params.Count == 0 || Params.All(kvp =>
                   {
                       if (kvp.Value is null) return true;
                       if (kvp.Value is string str) return str == "";
                       return false;
                   })
                   ) && Time == -1;
        }

        /// <summary>
        /// Получение строки значения параметра
        /// </summary>
        /// <param name="value">Значение параметра</param>
        /// <returns>Строка значения параметра</returns>
        private static string GetValueString(object value)
        {
            return (value switch
            {
                IEnumerable enumerable when value.GetType() != typeof(string) =>
                    $"[{string.Join(", ", enumerable.Cast<object>().Select(item => item.ToString()))}]",
                null => "null",
                _ => value.ToString() ?? string.Empty
            });
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
    }
}
