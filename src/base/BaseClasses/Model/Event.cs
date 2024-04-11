using BaseClasses.Services;

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
        public string Name { get; set; } = "";

        /// <summary>
        /// Описание события
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Персонажи, участвующие в событии
        /// </summary>
        public List<Character> Characters { get; set; } = new List<Character>();

        /// <summary>
        /// Локация, в которой происходит событие
        /// </summary>
        public List<Location> Locations { get; set; } = new List<Location>();

        /// <summary>
        /// Вещи, участвующие в событии
        /// </summary>
        public List<Item> Items { get; set; } = new List<Item>();

        /// <summary>
        /// Время события
        /// </summary>
        public int Time { get; set; } = -1;

        /// <summary>
        /// Объединение события с другим событием
        /// </summary>
        /// <param name="event">Событие, с которым объединяется текущее</param>
        public void Merge(Event @event)
        {
            if (Name == "")
            {
                Name = @event.Name;
            }
            if (Description == "")
            {
                Description = @event.Description;
            }
            foreach (var character in @event.Characters.ToList())
            {
                Binder.Bind(this, character);
                Binder.Unbind(@event, character);
            }
            foreach (var location in @event.Locations.ToList())
            {
                Binder.Bind(this, location);
                Binder.Unbind(@event, location);
            }
            foreach (var item in @event.Items.ToList())
            {
                Binder.Bind(this, item);
                Binder.Unbind(@event, item);
            }
        }

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
                   $"Вещи: {string.Join(", ", Items.Select(i => i.Name))}\n" +
                   $"Время события: {Time}";
        }
    }
}
