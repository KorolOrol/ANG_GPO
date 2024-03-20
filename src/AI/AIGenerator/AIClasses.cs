﻿using BaseClasses.Model;

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
        /// События, в которых участвует персонаж
        /// </summary>
        public List<string> Events { get; set; }

        /// <summary>
        /// Преобразование в стандартный класс персонажа
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Стандартный класс персонажа</returns>
        /// <exception cref="ArgumentNullException">ИИ вернул пустой ответ</exception>
        public Character ToCharacter(Plot plot)
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
            if (plot.Characters != null && plot.Characters.Count != 0 && Relations != null)
            {
                foreach (var relation in Relations)
                {
                    try
                    {
                        Character rel = plot.Characters.First(c => c.Name == relation.Key);
                        character.Relations.Add(rel, relation.Value);
                        rel.Relations.Add(character, relation.Value);
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }
                }
            }
            character.Locations = new List<Location>();
            if (plot.Locations != null && plot.Locations.Count != 0 && Locations != null)
            {
                foreach (var location in Locations)
                {
                    try
                    {
                        Location loc = plot.Locations.First(l => l.Name == location);
                        character.Locations.Add(loc);
                        loc.Characters.Add(character);
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }
                }
            }
            character.Items = new List<Item>();
            if (plot.Items != null && plot.Items.Count != 0 && Items != null)
            {
                foreach (var item in Items)
                {
                    try
                    {
                        Item foundItem = plot.Items.First(i => i.Name == item);
                        if (foundItem.Host != null) continue;
                        character.Items.Add(foundItem);
                        foundItem.Host = character;
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }
                }
            }
            character.Events = new List<Event>();
            if (plot.Events != null && plot.Events.Count != 0 && Events != null)
            {
                foreach (var @event in Events)
                {
                    try
                    {
                        Event foundEvent = plot.Events.First(e => e.Name == @event);
                        character.Events.Add(foundEvent);
                        foundEvent.Characters.Add(character);
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }
                }
            }
            return character;
        }

        /// <summary>
        /// Получение новых отношений для персонажа
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Список новых отношений</returns>
        public List<string> NewRelations(Plot plot)
        {
            List<string> newRelations = new List<string>();
            foreach (string name in Relations.Keys)
            {
                if (plot.Characters.FirstOrDefault(c => c.Name == name) == null)
                {
                    newRelations.Add(name);
                }
            }
            return newRelations;
        }

        /// <summary>
        /// Получение новых местоположений для персонажа
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns></returns>
        public List<string> NewLocations(Plot plot)
        {
            List<string> newLocations = new List<string>();
            foreach (string name in Locations)
            {
                if (plot.Locations.FirstOrDefault(l => l.Name == name) == null)
                {
                    newLocations.Add(name);
                }
            }
            return newLocations;
        }

        /// <summary>
        /// Получение новых вещей для персонажа
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Список новых вещей</returns>
        public List<string> NewItems(Plot plot)
        {
            List<string> newItems = new List<string>();
            foreach (string name in Items)
            {
                if (plot.Items.FirstOrDefault(i => i.Name == name) == null)
                {
                    newItems.Add(name);
                }
            }
            return newItems;
        }

        /// <summary>
        /// Получение новых событий для персонажа
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Список новых событий</returns>
        public List<string> NewEvents(Plot plot)
        {
            List<string> newEvents = new List<string>();
            foreach (string name in Events)
            {
                if (plot.Events.FirstOrDefault(e => e.Name == name) == null)
                {
                    newEvents.Add(name);
                }
            }
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
        /// События, происходящие в локации
        /// </summary>
        public List<string> Events { get; set; }

        /// <summary>
        /// Преобразование в стандартный класс локации
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Стандартный класс локации</returns>
        /// <exception cref="ArgumentNullException">ИИ вернул пустой ответ</exception>
        public Location ToLocation(Plot plot)
        {
            if (this == null)
            {
                throw new ArgumentNullException("ИИ вернул пустой ответ");
            }
            Location location = new Location();
            location.Name = Name;
            location.Description = Description;
            location.Characters = new List<Character>();
            if (plot.Characters != null && plot.Characters.Count != 0 && Characters != null)
            {
                foreach (var character in Characters)
                {
                    try
                    {
                        Character charac = plot.Characters.First(c => c.Name == character);
                        location.Characters.Add(charac);
                        charac.Locations.Add(location);
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }
                }
            }
            location.Items = new List<Item>();
            if (plot.Items != null && plot.Items.Count != 0 && Items != null)
            {
                foreach (var item in Items)
                {
                    try
                    {
                        Item foundItem = plot.Items.First(i => i.Name == item);
                        location.Items.Add(foundItem);
                        foundItem.Location = location;
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }
                }
            }
            location.Events = new List<Event>();
            if (plot.Events != null && plot.Events.Count != 0 && Events != null)
            {
                foreach (var @event in Events)
                {
                    try
                    {
                        Event foundEvent = plot.Events.First(e => e.Name == @event);
                        location.Events.Add(foundEvent);
                        foundEvent.Locations.Add(location);
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }
                }
            }
            return location;
        }

        /// <summary>
        /// Получение новых персонажей для локации
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Список новых персонажей</returns>
        public List<string> NewCharacters(Plot plot)
        {
            List<string> newCharacters = new List<string>();
            foreach (string name in Characters)
            {
                if (plot.Characters.FirstOrDefault(c => c.Name == name) == null)
                {
                    newCharacters.Add(name);
                }
            }
            return newCharacters;
        }

        /// <summary>
        /// Получение новых вещей для локации
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Список новых вещей</returns>
        public List<string> NewItems(Plot plot)
        {
            List<string> newItems = new List<string>();
            foreach (string name in Items)
            {
                if (plot.Items.FirstOrDefault(i => i.Name == name) == null)
                {
                    newItems.Add(name);
                }
            }
            return newItems;
        }

        /// <summary>
        /// Получение новых событий для локации
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Список новых событий</returns>
        public List<string> NewEvents(Plot plot)
        {
            List<string> newEvents = new List<string>();
            foreach (string name in Events)
            {
                if (plot.Events.FirstOrDefault(e => e.Name == name) == null)
                {
                    newEvents.Add(name);
                }
            }
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
        /// События, в которых участвует предмет
        /// </summary>
        public List<string> Events { get; set; }

        /// <summary>
        /// Преобразование в стандартный класс предмета
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Стандартный класс предмета</returns>
        /// <exception cref="ArgumentNullException">ИИ вернул пустой ответ</exception>
        public Item ToItem(Plot plot)
        {
            if (this == null)
            {
                throw new ArgumentNullException("ИИ вернул пустой ответ");
            }
            Item item = new Item();
            item.Name = Name;
            item.Description = Description;
            try
            {
                item.Location = plot.Locations.First(l => l.Name == Location);
                item.Location.Items.Add(item);
            }
            catch (InvalidOperationException)
            {
                item.Location = null;
            }
            try
            {
                item.Host = plot.Characters.First(c => c.Name == Host);
                item.Host.Items.Add(item);
            }
            catch (InvalidOperationException)
            {
                item.Host = null;
            }
            item.Events = new List<Event>();
            if (plot.Events != null && plot.Events.Count != 0 && Events != null)
            {
                foreach (var @event in Events)
                {
                    try
                    {
                        Event foundEvent = plot.Events.First(e => e.Name == @event);
                        item.Events.Add(foundEvent);
                        foundEvent.Items.Add(item);
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }
                }
            }
            return item;
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
            List<string> newEvents = new List<string>();
            foreach (string name in Events)
            {
                if (plot.Events.FirstOrDefault(e => e.Name == name) == null)
                {
                    newEvents.Add(name);
                }
            }
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
        /// <param name="plot">История</param>
        /// <returns>Стандартный класс события</returns>
        /// <exception cref="ArgumentNullException">ИИ вернул пустой ответ</exception>
        public Event ToEvent(Plot plot)
        {
            if (this == null)
            {
                throw new ArgumentNullException("ИИ вернул пустой ответ");
            }
            Event @event = new Event();
            @event.Name = Name;
            @event.Description = Description;
            @event.Characters = new List<Character>();
            if (plot.Characters != null && plot.Characters.Count != 0 && Characters != null)
            {
                foreach (var character in Characters)
                {
                    try
                    {
                        Character charac = plot.Characters.First(c => c.Name == character);
                        @event.Characters.Add(charac);
                        charac.Events.Add(@event);
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }
                }
            }
            @event.Locations = new List<Location>();
            if (plot.Locations != null && plot.Locations.Count != 0 && Locations != null)
            {
                foreach (var location in Locations)
                {
                    try
                    {
                        Location loc = plot.Locations.First(l => l.Name == location);
                        @event.Locations.Add(loc);
                        loc.Events.Add(@event);
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }
                }
            }
            @event.Items = new List<Item>();
            if (plot.Items != null && plot.Items.Count != 0 && Items != null)
            {
                foreach (var item in Items)
                {
                    try
                    {
                        Item foundItem = plot.Items.First(i => i.Name == item);
                        @event.Items.Add(foundItem);
                        foundItem.Events.Add(@event);
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }
                }
            }
            return @event;
        }

        /// <summary>
        /// Получение новых персонажей для события
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Список новых персонажей</returns>
        public List<string> NewCharacters(Plot plot)
        {
            List<string> newCharacters = new List<string>();
            foreach (string name in Characters)
            {
                if (plot.Characters.FirstOrDefault(c => c.Name == name) == null)
                {
                    newCharacters.Add(name);
                }
            }
            return newCharacters;
        }

        /// <summary>
        /// Получение новых локаций для события
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Список новых локаций</returns>
        public List<string> NewLocations(Plot plot)
        {
            List<string> newLocations = new List<string>();
            foreach (string name in Locations)
            {
                if (plot.Locations.FirstOrDefault(l => l.Name == name) == null)
                {
                    newLocations.Add(name);
                }
            }
            return newLocations;
        }

        /// <summary>
        /// Получение новых вещей для события
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Список новых вещей</returns>
        public List<string> NewItems(Plot plot)
        {
            List<string> newItems = new List<string>();
            foreach (string name in Items)
            {
                if (plot.Items.FirstOrDefault(i => i.Name == name) == null)
                {
                    newItems.Add(name);
                }
            }
            return newItems;
        }
    }
}
