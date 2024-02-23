namespace BaseClasses.Model
{
    /// <summary>
    /// Событие в истории
    /// </summary>
    public class Event
    {
        /// <summary>
        /// Название события
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание события
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Персонажи, участвующие в событии
        /// </summary>
        public List<Character> Characters { get; set; }

        /// <summary>
        /// Локация, в которой происходит событие
        /// </summary>
        public List<Location> Locations { get; set; }

        /// <summary>
        /// Вещи, участвующие в событии
        /// </summary>
        public List<Item> Items { get; set; }
    }
}
