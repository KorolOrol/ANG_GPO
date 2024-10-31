using BaseClasses.Enum;
using BaseClasses.Interface;
using System.Collections;

namespace BaseClasses.Model
{
    /// <summary>
    /// Элемент истории
    /// </summary>
    public class Element : IElement
    {
        /// <summary>
        /// Тип элемента
        /// </summary>
        private readonly ElemType _type;

        /// <summary>
        /// Тип элемента
        /// </summary>
        public ElemType Type { get { return _type; } }

        /// <summary>
        /// Название элемента
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание элемента
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Параметры элемента
        /// </summary>
        public Dictionary<string, object> Params { get; set; } = new();

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
                       Dictionary<string, object> @params = null, int time = -1)
        {
            _type = type;
            Name = name;
            Description = description;
            Params = @params ?? new Dictionary<string, object>();
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
                   $"{string.Join("\n", Params.Select(kvp => 
                   $"{kvp.Key}: {GetValueString(kvp.Value)}"))}\n" +
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
        private string GetValueString(object value)
        {
            if (value is IEnumerable enumerable)
            {
                return $"[{string.Join(", ", 
                    enumerable.Cast<object>().Select(item => item.ToString()))}]";
            }
            else
            {
                return value.ToString();
            }
        }
    }
}
