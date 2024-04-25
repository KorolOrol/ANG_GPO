using BaseClasses.Interface;
using BaseClasses.Services;

namespace BaseClasses.Model
{
    /// <summary>
    /// Локация в истории
    /// </summary>
    public class Location : IPart
    {
        /// <summary>
        /// Название локации
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Описание локации
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Персонажи, находящиеся в локации
        /// </summary>
        public List<Character> Characters { get; set; } = new List<Character>();

        /// <summary>
        /// Вещи, находящиеся в локации
        /// </summary>
        public List<Item> Items { get; set; } = new List<Item>();

        /// <summary>
        /// События, происходящие в локации
        /// </summary>
        public List<Event> Events { get; set; } = new List<Event>();

        /// <summary>
        /// Время создания локации
        /// </summary>
        public int Time { get; set; }

        /// <summary>
        /// Объединение локации с другой локацией
        /// </summary>
        /// <param name="location">Локация, с которой объединяется текущая</param>
        public void Merge(IPart part)
        {
            if (part is null) throw new ArgumentNullException("Персонаж не может быть null");
            if (part is not Character) throw new ArgumentException("Неверный тип объекта");
            Location location = (Location)part;
            if (Name == "")
            {
                Name = location.Name;
            }
            if (Description == "")
            {
                Description = location.Description;
            }
            foreach (var character in location.Characters.ToList())
            {
                Binder.Bind(this, character);
                Binder.Unbind(location, character);
            }
            foreach (var item in location.Items.ToList())
            {
                Binder.Bind(this, item);
                Binder.Unbind(location, item);
            }
            foreach (var ev in location.Events.ToList())
            {
                Binder.Bind(this, ev);
                Binder.Unbind(location, ev);
            }
            if (Time == -1)
            {
                Time = location.Time;
            }
        }

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
                   $"События: {string.Join(", ", Events.Select(e => e.Name))}\n" +
                   $"Время создания: {Time}\n";
        }
    }
}
