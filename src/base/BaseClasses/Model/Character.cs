using BaseClasses.Interface;
using BaseClasses.Services;

namespace BaseClasses.Model
{
    /// <summary>
    /// Персонаж истории
    /// </summary>
    public class Character : IPart
    {
        /// <summary>
        /// Имя персонажа
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Описание персонажа
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Черты характера персонажа
        /// </summary>
        public List<string> Traits { get; set; } = new List<string>();

        /// <summary>
        /// Отношения персонажа с другими персонажами
        /// </summary>
        public List<Relation> Relations { get; set; } = new List<Relation>();

        /// <summary>
        /// Местоположение персонажа
        /// </summary>
        public List<Location> Locations { get; set; } = new List<Location>();

        /// <summary>
        /// Вещи персонажа
        /// </summary>
        public List<Item> Items { get; set; } = new List<Item>();

        /// <summary>
        /// События, в которых участвует персонаж
        /// </summary>
        public List<Event> Events { get; set; } = new List<Event>();

        /// <summary>
        /// Время создания персонажа
        /// </summary>
        public int Time { get; set; } = -1;

        /// <summary>
        /// Объединение персонажа с другим персонажем
        /// </summary>
        /// <param name="part">Персонаж, с которым объединяется текущий</param>
        public void Merge(IPart part)
        {
            if (part is null) throw new ArgumentNullException("Персонаж не может быть null");
            if (part is not Character) throw new ArgumentException("Неверный тип объекта");
            Character character = (Character)part;
            if (Name == "")
            {
                Name = character.Name;
            }
            if (Description == "")
            {
                Description = character.Description;
            }
            foreach (var trait in character.Traits)
            {
                if (!Traits.Contains(trait))
                {
                    Traits.Add(trait);
                }
            }
            foreach (var relation in character.Relations)
            {
                Binder.Bind(this, relation.Character, relation.Value);
                Binder.Unbind(relation.Character, character);
            }
            foreach (var location in character.Locations)
            {
                Binder.Bind(this, location);
                Binder.Unbind(character, location);
            }
            foreach (var item in character.Items)
            {
                Binder.Bind(this, item);
                Binder.Unbind(character, item);
            }
            foreach (var @event in character.Events)
            {
                Binder.Bind(this, @event);
                Binder.Unbind(character, @event);
            }
            Time = Math.Max(Time, character.Time);
        }

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
                   $"Отношения: {string.Join(", ", Relations.Select(r => 
                                                   $"{r.Character.Name} ({r.Value})"))}\n" +
                   $"Местоположение: {string.Join(", ", Locations.Select(l => l.Name))}\n" +
                   $"Вещи: {string.Join(", ", Items.Select(i => i.Name))}\n" +
                   $"События: {string.Join(", ", Events.Select(e => e.Name))}\n" +
                   $"Время создания: {Time}\n";
        }

        /// <summary>
        /// Проверка на пустоту персонажа
        /// </summary>
        /// <returns>True, если персонаж пуст, иначе false</returns>
        public bool IsEmpty()
        {
            return Name == "" && Description == "" && Traits.Count == 0 && Relations.Count == 0 &&
                   Locations.Count == 0 && Items.Count == 0 && Events.Count == 0 && Time == -1;
        }
    }
}
