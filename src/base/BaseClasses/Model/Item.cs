namespace BaseClasses.Model
{
    /// <summary>
    /// Предмет в истории
    /// </summary>
    public class Item
    {
        /// <summary>
        /// Название предмета
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание предмета
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Хозяин предмета
        /// </summary>
        public Character Host { get; set; }

        /// <summary>
        /// Локация, в которой находится предмет
        /// </summary>
        public Location Location { get; set; }

        /// <summary>
        /// События, в которых участвует предмет
        /// </summary>
        public List<Event> Events { get; set; }
    }
}
