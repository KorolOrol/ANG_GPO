namespace BaseClasses.Model
{
    /// <summary>
    /// Персонаж истории
    /// </summary>
    public class Character
    {
        /// <summary>
        /// Имя персонажа
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание персонажа
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Характер персонажа
        /// </summary>
        public string Temper { get; set; }

        /// <summary>
        /// Отношения персонажа с другими персонажами
        /// </summary>
        public Dictionary<Character, double> Relations { get; set; }

        /// <summary>
        /// Местоположение персонажа
        /// </summary>
        public List<Location> Locations { get; set; }

        /// <summary>
        /// Вещи персонажа
        /// </summary>
        public List<Item> Items { get; set; }

        /// <summary>
        /// События, в которых участвует персонаж
        /// </summary>
        public List<Event> Events { get; set; }
    }
}
