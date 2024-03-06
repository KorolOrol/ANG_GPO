using BaseClasses.Model;

namespace AIGenerator
{
    /// <summary>
    /// Персонаж истории, полученный от ИИ
    /// </summary>
    public class AICharacter
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
        /// Характер персонажа
        /// </summary>
        public List<string> Traits { get; set; }

        /// <summary>
        /// Отношения персонажа с другими персонажами
        /// </summary>
        public Dictionary<string, double> Relations { get; set; }

        /// <summary>
        /// Местоположение персонажа
        /// </summary>
        public List<string> Locations { get; set; }

        /// <summary>
        /// Вещи персонажа
        /// </summary>
        public List<string> Items { get; set; }

        /// <summary>
        /// Преобразование в стандартный класс персонажа
        /// </summary>
        /// <param name="characters">Список персонажей</param>
        /// <param name="locations">Список локаций</param>
        /// <param name="items">Список предметов</param>
        /// <param name="events">Список событий</param>
        /// <returns>Стандартный класс персонажа</returns>
        /// <exception cref="ArgumentNullException">ИИ вернул пустой ответ</exception>
        public Character ToCharacter(List<Character> characters, List<Location> locations, 
                                     List<Item> items, List<Event> events)
        {
            if (this == null)
            {
                throw new ArgumentNullException("ИИ вернул пустой ответ");
            }
            Character character = new Character();
            character.Name = Name;
            character.Description = Description;
            character.Traits = Traits;
            character.Relations = new Dictionary<Character, double>();
            foreach (var relation in Relations)
            {
                Character rel = characters.First(c => c.Name == relation.Key);
                character.Relations.Add(rel, relation.Value);
                rel.Relations.Add(character, relation.Value);
            }
            character.Locations = new List<Location>();
            foreach (var location in Locations)
            {
                Location loc = locations.First(l => l.Name == location);
                character.Locations.Add(loc);
                loc.Characters.Add(character);
            }
            character.Items = new List<Item>();
            foreach (var item in Items)
            {
                Item foundItem = items.First(i => i.Name == item);
                character.Items.Add(foundItem);
                foundItem.Host = character;
            }
            character.Events = new List<Event>();
            return character;
        }
    }

    /// <summary>
    /// Локация в истории, полученная от ИИ
    /// </summary>
    public class AILocation
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
        public List<string> Characters { get; set; }

        /// <summary>
        /// Вещи, находящиеся в локации
        /// </summary>
        public List<string> Items { get; set; }

        /// <summary>
        /// Преобразование в стандартный класс локации
        /// </summary>
        /// <param name="characters">Список персонажей</param>
        /// <param name="locations">Список локаций</param>
        /// <param name="items">Список предметов</param>
        /// <param name="events">Список событий</param>
        /// <returns>Стандартный класс локации</returns>
        /// <exception cref="ArgumentNullException">ИИ вернул пустой ответ</exception>
        public Location ToLocation(List<Character> characters, List<Location> locations, 
                                   List<Item> items, List<Event> events)
        {
            if (this == null)
            {
                throw new ArgumentNullException("ИИ вернул пустой ответ");
            }
            Location location = new Location();
            location.Name = Name;
            location.Description = Description;
            location.Characters = new List<Character>();
            foreach (var character in Characters)
            {
                Character charac = characters.First(c => c.Name == character);
                location.Characters.Add(charac);
                charac.Locations.Add(location);
            }
            location.Items = new List<Item>();
            foreach (var item in Items)
            {
                Item foundItem = items.First(i => i.Name == item);
                location.Items.Add(foundItem);
                foundItem.Location = location;
            }
            location.Events = new List<Event>();
            return location;
        }
    }

    /// <summary>
    /// Предмет в истории, полученный от ИИ
    /// </summary>
    public class AIItem
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
        /// Местоположение предмета
        /// </summary>
        public string Location { get; set; }

        /// <summary>
        /// Хозяин предмета
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Преобразование в стандартный класс предмета
        /// </summary>
        /// <param name="characters">Список персонажей</param>
        /// <param name="locations">Список локаций</param>
        /// <param name="items">Список предметов</param>
        /// <param name="events">Список событий</param>
        /// <returns>Стандартный класс предмета</returns>
        /// <exception cref="ArgumentNullException">ИИ вернул пустой ответ</exception>
        public Item ToItem(List<Character> characters, List<Location> locations, 
                           List<Item> items, List<Event> events)
        {
            if (this == null)
            {
                throw new ArgumentNullException("ИИ вернул пустой ответ");
            }
            Item item = new Item();
            item.Name = Name;
            item.Description = Description;
            item.Location = locations.First(l => l.Name == Location);
            item.Host = characters.First(c => c.Name == Host);
            item.Events = new List<Event>();
            return item;
        }
    }

    /// <summary>
    /// Событие в истории, полученное от ИИ
    /// </summary>
    public class AIEvent
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
        public List<string> Characters { get; set; }

        /// <summary>
        /// Локации, в которых происходит событие
        /// </summary>
        public List<string> Locations { get; set; }

        /// <summary>
        /// Вещи, участвующие в событии
        /// </summary>
        public List<string> Items { get; set; }

        /// <summary>
        /// Преобразование в стандартный класс события
        /// </summary>
        /// <param name="characters">Список персонажей</param>
        /// <param name="locations">Список локаций</param>
        /// <param name="items">Список предметов</param>
        /// <param name="events">Список событий</param>
        /// <returns>Стандартный класс события</returns>
        /// <exception cref="ArgumentNullException">ИИ вернул пустой ответ</exception>
        public Event ToEvent(List<Character> characters, List<Location> locations, 
                             List<Item> items, List<Event> events)
        {
            if (this == null)
            {
                throw new ArgumentNullException("ИИ вернул пустой ответ");
            }
            Event @event = new Event();
            @event.Name = Name;
            @event.Description = Description;
            @event.Characters = new List<Character>();
            foreach (var character in Characters)
            {
                Character charac = characters.First(c => c.Name == character);
                @event.Characters.Add(charac);
                charac.Events.Add(@event);
            }
            @event.Locations = new List<Location>();
            foreach (var location in Locations)
            {
                Location loc = locations.First(l => l.Name == location);
                @event.Locations.Add(loc);
                loc.Events.Add(@event);
            }
            @event.Items = new List<Item>();
            foreach (var item in Items)
            {
                Item foundItem = items.First(i => i.Name == item);
                @event.Items.Add(foundItem);
                foundItem.Events.Add(@event);
            }
            return @event;
        }
    }
}
