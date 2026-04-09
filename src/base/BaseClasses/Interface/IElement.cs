using BaseClasses.Enum;
using BaseClasses.Model.Params;

namespace BaseClasses.Interface
{
    /// <summary>
    /// Интерфейс элемента истории
    /// </summary>
    public interface IElement
    {
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
        public ParamBag Params { get; }

        /// <summary>
        /// Время создания элемента
        /// </summary>
        public int Time { get; set; }

        /// <summary>
        /// Проверка на пустоту
        /// </summary>
        /// <returns>Ture, если элемент пуст, иначе False</returns>
        public bool IsEmpty();
    }
}
