namespace BaseClasses.Model
{
    /// <summary>
    /// Локация в истории
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Название локации
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание локации
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Персонажи, находящиеся в локации
        /// </summary>
        public List<Character> Characters { get; set; }

        /// <summary>
        /// Вещи, находящиеся в локации
        /// </summary>
        public List<Item> Items { get; set; }

        /// <summary>
        /// События, происходящие в локации
        /// </summary>
        public List<Event> Events { get; set; }
    }
}
