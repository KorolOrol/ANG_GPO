using BaseClasses.Model;
using BaseClasses.Services;

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
        public string Name { get; set; } = "";

        /// <summary>
        /// Описание персонажа
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Характер персонажа
        /// </summary>
        public List<string> Traits { get; set; } = new List<string>();

        /// <summary>
        /// Отношения персонажа с другими персонажами
        /// </summary>
        public Dictionary<string, double> Relations { get; set; } = new Dictionary<string, double>();

        /// <summary>
        /// Местоположение персонажа
        /// </summary>
        public List<string> Locations { get; set; } = new List<string>();

        /// <summary>
        /// Вещи персонажа
        /// </summary>
        public List<string> Items { get; set; } = new List<string>();

        /// <summary>
        /// События, в которых участвует персонаж
        /// </summary>
        public List<string> Events { get; set; } = new List<string>();

        /// <summary>
        /// Преобразование в стандартный класс персонажа
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Стандартный класс персонажа</returns>
        public Character ToCharacter(Plot plot)
        {
            Character character = new Character();
            character.Name = Name;
            character.Description = Description;
            character.Traits = Traits;
            character.Relations = new List<Relation>();
            character.Time = plot.Time++;
            if (plot.Characters != null && plot.Characters.Count != 0 && Relations != null)
            {
                foreach (var relation in Relations)
                {
                    Character foundCharacter = plot.Characters.FirstOrDefault(c => c.Name == relation.Key);
                    if (foundCharacter == null) continue;
                    Binder.Bind(character, foundCharacter, relation.Value);
                }
            }
            character.Locations = new List<Location>();
            if (plot.Locations != null && plot.Locations.Count != 0 && Locations != null)
            {
                foreach (var location in Locations)
                {
                    Location foundLocation = plot.Locations.FirstOrDefault(l => l.Name == location);
                    if (foundLocation == null) continue;
                    Binder.Bind(character, foundLocation);
                }
            }
            character.Items = new List<Item>();
            if (plot.Items != null && plot.Items.Count != 0 && Items != null)
            {
                foreach (var item in Items)
                {
                    Item foundItem = plot.Items.FirstOrDefault(i => i.Name == item);
                    if (foundItem == null || foundItem.Host != null) continue;
                    Binder.Bind(character, foundItem);
                }
            }
            character.Events = new List<Event>();
            if (plot.Events != null && plot.Events.Count != 0 && Events != null)
            {
                foreach (var @event in Events)
                {
                    Event foundEvent = plot.Events.FirstOrDefault(e => e.Name == @event);
                    if (foundEvent == null) continue;
                    Binder.Bind(character, foundEvent);
                }
            }
            return character;
        }

        /// <summary>
        /// Перевод из стандартного класса персонажа в класс персонажа ИИ
        /// </summary>
        /// <param name="character">Стандартный класс персонажа</param>
        public AICharacter(Character character)
        {
            if (character == null) return;
            Name = character.Name;
            Description = character.Description;
            Traits = character.Traits;
            Relations = new Dictionary<string, double>();
            foreach (var relation in character.Relations)
            {
                Relations.Add(relation.Character.Name, relation.Value);
            }
            Locations = character.Locations.Select(l => l.Name).ToList();
            Items = character.Items.Select(i => i.Name).ToList();
            Events = character.Events.Select(e => e.Name).ToList();
        }

        /// <summary>
        /// Получение новых отношений для персонажа
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Список новых отношений</returns>
        public List<string> NewRelations(Plot plot)
        {
            List<string> newRelations = 
                Relations.Keys.Where(name => plot.Characters.FirstOrDefault(c => c.Name == name) == null)
                              .ToList();
            return newRelations;
        }

        /// <summary>
        /// Получение новых местоположений для персонажа
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Список новых локаций</returns>
        public List<string> NewLocations(Plot plot)
        {
            List<string> newLocations = 
                Locations.Where(name => plot.Locations.FirstOrDefault(l => l.Name == name) == null)
                         .ToList();
            return newLocations;
        }

        /// <summary>
        /// Получение новых вещей для персонажа
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Список новых вещей</returns>
        public List<string> NewItems(Plot plot)
        {
            List<string> newItems = 
                Items.Where(name => plot.Items.FirstOrDefault(i => i.Name == name) == null)
                     .ToList();
            return newItems;
        }

        /// <summary>
        /// Получение новых событий для персонажа
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Список новых событий</returns>
        public List<string> NewEvents(Plot plot)
        {
            List<string> newEvents = 
                Events.Where(name => plot.Events.FirstOrDefault(e => e.Name == name) == null)
                      .ToList();
            return newEvents;
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
        public string Name { get; set; } = "";

        /// <summary>
        /// Описание локации
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Персонажи, находящиеся в локации
        /// </summary>
        public List<string> Characters { get; set; } = new List<string>();

        /// <summary>
        /// Вещи, находящиеся в локации
        /// </summary>
        public List<string> Items { get; set; } = new List<string>();

        /// <summary>
        /// События, происходящие в локации
        /// </summary>
        public List<string> Events { get; set; } = new List<string>();

        /// <summary>
        /// Преобразование в стандартный класс локации
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Стандартный класс локации</returns>
        public Location ToLocation(Plot plot)
        {
            Location location = new Location();
            location.Name = Name;
            location.Description = Description;
            location.Characters = new List<Character>();
            location.Time = plot.Time++;
            if (plot.Characters != null && plot.Characters.Count != 0 && Characters != null)
            {
                foreach (var character in Characters)
                {
                    Character foundCharacter = plot.Characters.FirstOrDefault(c => c.Name == character);
                    if (foundCharacter == null) continue;
                    Binder.Bind(location, foundCharacter);
                }
            }
            location.Items = new List<Item>();
            if (plot.Items != null && plot.Items.Count != 0 && Items != null)
            {
                foreach (var item in Items)
                {
                    Item foundItem = plot.Items.FirstOrDefault(i => i.Name == item);
                    if (foundItem == null || foundItem.Location != null) continue;
                    Binder.Bind(location, foundItem);
                }
            }
            location.Events = new List<Event>();
            if (plot.Events != null && plot.Events.Count != 0 && Events != null)
            {
                foreach (var @event in Events)
                {
                    Event foundEvent = plot.Events.FirstOrDefault(e => e.Name == @event);
                    if (foundEvent == null) continue;
                    Binder.Bind(location, foundEvent);
                }
            }
            return location;
        }

        /// <summary>
        /// Перевод из стандартного класса локации в класс локации ИИ
        /// </summary>
        /// <param name="location">Стандартный класс локации</param>
        public AILocation(Location location)
        {
            if (location == null) return;
            Name = location.Name;
            Description = location.Description;
            Characters = location.Characters.Select(c => c.Name).ToList();
            Items = location.Items.Select(i => i.Name).ToList();
            Events = location.Events.Select(e => e.Name).ToList();
        }

        /// <summary>
        /// Получение новых персонажей для локации
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Список новых персонажей</returns>
        public List<string> NewCharacters(Plot plot)
        {
            List<string> newCharacters = 
                Characters.Where(name => plot.Characters.FirstOrDefault(c => c.Name == name) == null)
                          .ToList();
            return newCharacters;
        }

        /// <summary>
        /// Получение новых вещей для локации
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Список новых вещей</returns>
        public List<string> NewItems(Plot plot)
        {
            List<string> newItems = 
                Items.Where(name => plot.Items.FirstOrDefault(i => i.Name == name) == null)
                     .ToList();
            return newItems;
        }

        /// <summary>
        /// Получение новых событий для локации
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Список новых событий</returns>
        public List<string> NewEvents(Plot plot)
        {
            List<string> newEvents = 
                Events.Where(name => plot.Events.FirstOrDefault(e => e.Name == name) == null)
                      .ToList();
            return newEvents;
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
        public string Name { get; set; } = "";

        /// <summary>
        /// Описание предмета
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Местоположение предмета
        /// </summary>
        public string Location { get; set; } = "";

        /// <summary>
        /// Хозяин предмета
        /// </summary>
        public string Host { get; set; } = "";

        /// <summary>
        /// События, в которых участвует предмет
        /// </summary>
        public List<string> Events { get; set; } = new List<string>();

        /// <summary>
        /// Преобразование в стандартный класс предмета
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Стандартный класс предмета</returns>
        public Item ToItem(Plot plot)
        {
            Item item = new Item();
            item.Name = Name;
            item.Description = Description;
            item.Time = plot.Time++;
            Location foundLocation = plot.Locations.FirstOrDefault(l => l.Name == Location);
            if (foundLocation != null)
            {
                Binder.Bind(item, foundLocation);
            }
            else
            {
                item.Location = null;
            }
            Character foundHost = plot.Characters.FirstOrDefault(c => c.Name == Host);
            if (foundHost != null)
            {
                Binder.Bind(item, foundHost);
            }
            else
            {
                item.Host = null;
            }
            item.Events = new List<Event>();
            if (plot.Events != null && plot.Events.Count != 0 && Events != null)
            {
                foreach (var @event in Events)
                {
                    Event foundEvent = plot.Events.FirstOrDefault(e => e.Name == @event);
                    if (foundEvent == null) continue;
                    Binder.Bind(item, foundEvent);
                }
            }
            return item;
        }

        /// <summary>
        /// Перевод из стандартного класса предмета в класс предмета ИИ
        /// </summary>
        /// <param name="item">Стандартный класс предмета</param>
        public AIItem(Item item)
        {
            if (item == null) return;
            Name = item.Name;
            Description = item.Description;
            Location = item.Location?.Name;
            Host = item.Host?.Name;
            Events = item.Events.Select(e => e.Name).ToList();
        }

        /// <summary>
        /// Получение новый хозяин для предмета
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Новый хозяин</returns>
        public string NewHost(Plot plot)
        {
            if (plot.Characters.FirstOrDefault(c => c.Name == Host) == null)
            {
                return Host;
            }
            return null;
        }

        /// <summary>
        /// Получение новой локации для предмета
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Новая локация</returns>
        public string NewLocation(Plot plot)
        {
            if (plot.Locations.FirstOrDefault(l => l.Name == Location) == null)
            {
                return Location;
            }
            return null;
        }

        /// <summary>
        /// Получение новых событий для предмета
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Список новых событий</returns>
        public List<string> NewEvents(Plot plot)
        {
            List<string> newEvents = 
                Events.Where(name => plot.Events.FirstOrDefault(e => e.Name == name) == null)
                      .ToList();
            return newEvents;
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
        public string Name { get; set; } = "";

        /// <summary>
        /// Описание события
        /// </summary>
        public string Description { get; set; } = "";

        /// <summary>
        /// Персонажи, участвующие в событии
        /// </summary>
        public List<string> Characters { get; set; } = new List<string>();

        /// <summary>
        /// Локации, в которых происходит событие
        /// </summary>
        public List<string> Locations { get; set; } = new List<string>();

        /// <summary>
        /// Вещи, участвующие в событии
        /// </summary>
        public List<string> Items { get; set; } = new List<string>();

        /// <summary>
        /// Преобразование в стандартный класс события
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Стандартный класс события</returns>
        public Event ToEvent(Plot plot)
        {
            Event @event = new Event();
            @event.Name = Name;
            @event.Description = Description;
            @event.Characters = new List<Character>();
            @event.Time = plot.Time++;
            if (plot.Characters != null && plot.Characters.Count != 0 && Characters != null)
            {
                foreach (var character in Characters)
                {
                    Character foundCharacter = plot.Characters.FirstOrDefault(c => c.Name == character);
                    if (foundCharacter == null) continue;
                    Binder.Bind(@event, foundCharacter);
                }
            }
            @event.Locations = new List<Location>();
            if (plot.Locations != null && plot.Locations.Count != 0 && Locations != null)
            {
                foreach (var location in Locations)
                {
                    Location foundLocation = plot.Locations.FirstOrDefault(l => l.Name == location);
                    if (foundLocation == null) continue;
                    Binder.Bind(@event, foundLocation);
                }
            }
            @event.Items = new List<Item>();
            if (plot.Items != null && plot.Items.Count != 0 && Items != null)
            {
                foreach (var item in Items)
                {
                    Item foundItem = plot.Items.FirstOrDefault(i => i.Name == item);
                    if (foundItem == null) continue;
                    Binder.Bind(@event, foundItem);
                }
            }
            return @event;
        }

        /// <summary>
        /// Перевод из стандартного класса события в класс события ИИ
        /// </summary>
        /// <param name="event">Стандартный класс события</param>
        public AIEvent(Event @event)
        {
            if (@event == null) return;
            Name = @event.Name;
            Description = @event.Description;
            Characters = @event.Characters.Select(c => c.Name).ToList();
            Locations = @event.Locations.Select(l => l.Name).ToList();
            Items = @event.Items.Select(i => i.Name).ToList();
        }

        /// <summary>
        /// Получение новых персонажей для события
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Список новых персонажей</returns>
        public List<string> NewCharacters(Plot plot)
        {
            List<string> newCharacters = 
                Characters.Where(name => plot.Characters.FirstOrDefault(c => c.Name == name) == null)
                          .ToList();
            return newCharacters;
        }

        /// <summary>
        /// Получение новых локаций для события
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Список новых локаций</returns>
        public List<string> NewLocations(Plot plot)
        {
            List<string> newLocations = 
                Locations.Where(name => plot.Locations.FirstOrDefault(l => l.Name == name) == null)
                         .ToList();
            return newLocations;
        }

        /// <summary>
        /// Получение новых вещей для события
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Список новых вещей</returns>
        public List<string> NewItems(Plot plot)
        {
            List<string> newItems = 
                Items.Where(name => plot.Items.FirstOrDefault(i => i.Name == name) == null)
                     .ToList();
            return newItems;
        }
    }
}
