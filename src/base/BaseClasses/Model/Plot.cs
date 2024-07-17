namespace BaseClasses.Model
{
    /// <summary>
    /// История
    /// </summary>
    public class Plot
    {
        /// <summary>
        /// Список персонажей
        /// </summary>
        public List<Character> Characters { get; set; } = new();

        /// <summary>
        /// Список локаций
        /// </summary>
        public List<Location> Locations { get; set; } = new();

        /// <summary>
        /// Список предметов
        /// </summary>
        public List<Item> Items { get; set; } = new();

        /// <summary>
        /// Список событий
        /// </summary>
        public List<Event> Events { get; set; } = new();

        /// <summary>
        /// Время
        /// </summary>
        public int Time { get; set; } = 0;

        /// <summary>
        /// Полная информация об истории
        /// </summary>
        /// <returns>Полная информация об истории</returns>
        public string FullInfo()
        {
            string info = "";
            foreach (var c in Characters)
            {
                info += c.FullInfo() + "\n";
            }
            foreach (var l in Locations)
            {
                info += l.FullInfo() + "\n";
            }
            foreach (var i in Items)
            {
                info += i.FullInfo() + "\n";
            }
            foreach (var e in Events)
            {
                info += e.FullInfo() + "\n";
            }
            return info;
        }
    }
}
