using BaseClasses.Interface;
using BaseClasses.Services;

namespace BaseClasses.Model
{
    /// <summary>
    /// Предмет в истории
    /// </summary>
    public class Item : IPart
    {
        /// <summary>
        /// Название предмета
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Описание предмета
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Хозяин предмета
        /// </summary>
        public Character Host { get; set; } = null;

        /// <summary>
        /// Локация, в которой находится предмет
        /// </summary>
        public Location Location { get; set; } = null;

        /// <summary>
        /// События, в которых участвует предмет
        /// </summary>
        public List<Event> Events { get; set; } = new List<Event>();

        /// <summary>
        /// Время создания предмета
        /// </summary>
        public int Time { get; set; } = -1;

        /// <summary>
        /// Объединение предмета с другим предметом
        /// </summary>
        /// <param name="item">Предмет, с которым объединяется текущий</param>
        public void Merge(IPart part)
        {
            if (part is null) throw new ArgumentNullException("Персонаж не может быть null");
            if (part is not Item) throw new ArgumentException("Неверный тип объекта");
            Item item = (Item)part;
            if (Name == "")
            {
                Name = item.Name;
            }
            if (Description == "")
            {
                Description = item.Description;
            }
            Binder.Bind(this, item.Host);
            Binder.Unbind(item, item.Host);
            Binder.Bind(this, item.Location);
            Binder.Unbind(item, item.Location);
            foreach (var ev in item.Events.ToList())
            {
                Binder.Bind(this, ev);
                Binder.Unbind(item, ev);
            }
        }

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
                   $"Время создания: {Time}\n";
        }
    }
}
