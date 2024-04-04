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

        /// <summary>
        /// Время создания предмета
        /// </summary>
        public int Time { get; set; }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override string ToString()
        {
            return "Предмет: " + Name;
        }

        /// <summary>
        /// Возвращает полную информацию о предмете
        /// </summary>
        /// <returns>Полная информация о предмете</returns>
        public string FullInfo()
        {
            return $"Предмет: {Name}\n" +
                   $"Описание: {Description}\n" +
                   $"Хозяин: {Host?.Name ?? "нет"}\n" +
                   $"Локация: {Location?.Name ?? "нет"}\n" +
                   $"События: {string.Join(", ", Events.Select(e => e.Name))}\n" +
                   $"Время создания: {Time}";
        }
    }
}
