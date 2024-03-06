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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override string ToString()
        {
            return "Локация: " + Name;
        }

        /// <summary>
        /// Возвращает полную информацию о локации
        /// </summary>
        /// <returns>Полная информация о локации</returns>
        public string FullInfo()
        {
            return $"Локация: {Name}\n" +
                   $"Описание: {Description}\n" +
                   $"Персонажи: {string.Join(", ", Characters.Select(c => c.Name))}\n" +
                   $"Вещи: {string.Join(", ", Items.Select(i => i.Name))}\n" +
                   $"События: {string.Join(", ", Events.Select(e => e.Name))}\n";
        }
    }
}
