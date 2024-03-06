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
        /// Черты характера персонажа
        /// </summary>
        public List<string> Traits { get; set; }

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

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override string ToString()
        {
            return "Персонаж: " + Name;
        }

        /// <summary>
        /// Возвращает полную информацию о персонаже
        /// </summary>
        /// <returns>Полная информация о персонаже</returns>
        public string FullInfo()
        {
            return $"Персонаж: {Name}\n" +
                   $"Описание: {Description}\n" +
                   $"Черты характера: {string.Join(", ", Traits)}\n" +
                   $"Отношения: {string.Join(", ", Relations.Select(r => $"{r.Key.Name} ({r.Value})"))}\n" +
                   $"Местоположение: {string.Join(", ", Locations.Select(l => l.Name))}\n" +
                   $"Вещи: {string.Join(", ", Items.Select(i => i.Name))}\n" +
                   $"События: {string.Join(", ", Events.Select(e => e.Name))}\n";
        }
    }
}
