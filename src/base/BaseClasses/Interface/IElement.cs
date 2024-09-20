using BaseClasses.Enum;

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
        /// Параметры элемента
        /// </summary>
        public Dictionary<string, object> Params { get; set; }

        /// <summary>
        /// Время создания элемента
        /// </summary>
        public int Time { get; }

        /// <summary>
        /// Объединение двух элементов
        /// </summary>
        /// <param name="element">Элемент для объединения</param>
        public void Merge(IElement element);

        /// <summary>
        /// Полная информация об элементе
        /// </summary>
        /// <returns>Полная информация об элементе</returns>
        public string FullInfo();

        /// <summary>
        /// Проверка на пустоту
        /// </summary>
        /// <returns>Ture, если элемент пуст, иначе False</returns>
        public bool IsEmpty();
    }
}
