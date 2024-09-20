using BaseClasses.Enum;
using BaseClasses.Interface;
using System.Reflection;

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
        /// Конструктор элемента
        /// </summary>
        public ElemType Type { get { return _type; } }

        /// <summary>
        /// Название элемента
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Описание элемента
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Параметры элемента
        /// </summary>
        public Dictionary<string, object> Params { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// Время создания элемента
        /// </summary>
        public int Time { get; private set; } = 0;

        /// <summary>
        /// Объединение двух элементов
        /// </summary>
        /// <param name="element">Элемент для объединения</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public void Merge(IElement element)
        {
            if (element is null) throw new ArgumentNullException("Элемент не может быть null.");
            if (element.Type != Type) throw new ArgumentException("Неверный тип элемента.");
            if (Name == "")
            {
                Name = element.Name;
            }
            if (Description == "")
            {
                Description = element.Description;
            }
            foreach (KeyValuePair<string, object> kvp in element.Params)
            {
                Binder.Rebind(this, element, kvp.Key, kvp.Value);
            }
            Time = Math.Max(Time, element.Time);
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
                   $"{string.Join("\n", Params.Select(kvp => $"{kvp.Key}: {kvp.Value}"))}\n" +
                   $"Creation time: {Time}";
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
                   ) && Time == 0;
        }
    }
}
