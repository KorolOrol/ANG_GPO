namespace BaseClasses.Interface
{
    /// <summary>
    /// Интерфейс части истории
    /// </summary>
    public interface IPart
    {
        /// <summary>
        /// Название части
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание части
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Объединение двух частей
        /// </summary>
        /// <param name="part">Часть для объединения</param>
        public void Merge(IPart part);

        /// <summary>
        /// Полная информация о части
        /// </summary>
        /// <returns>Полная информация о части</returns>
        public string FullInfo();

        /// <summary>
        /// Проверка на пустоту
        /// </summary>
        /// <returns>Ture, если часть пуста, иначе False</returns>
        public bool IsEmpty();
    }
}
