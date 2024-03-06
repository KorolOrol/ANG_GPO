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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override string ToString()
        {
            return "Событие: " + Name;
        }

        /// <summary>
        /// Возвращает полную информацию о событии
        /// </summary>
        /// <returns>Полная информация о событии</returns>
        public string FullInfo()
        {
            return $"Событие: {Name}\n" +
                   $"Описание: {Description}\n" +
                   $"Персонажи: {string.Join(", ", Characters.Select(c => c.Name))}\n" +
                   $"Локации: {string.Join(", ", Locations.Select(l => l.Name))}\n" +
                   $"Вещи: {string.Join(", ", Items.Select(i => i.Name))}\n";
        }
    }
}
