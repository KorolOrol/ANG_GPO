using BaseClasses.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseClasses.Services
{
    /// <summary>
    /// Сервис для слияния историй
    /// </summary>
    public static class Binder
    {
        /// <summary>
        /// Связывание двух персонажей
        /// </summary>
        /// <param name="character1">Первый персонаж</param>
        /// <param name="character2">Второй персонаж</param>
        /// <param name="relations">Отношения между персонажами</param>
        public static void Bind(Character character1, Character character2, double relations)
        {
            if (character1.Relations.FirstOrDefault(rel => rel.Character == character2) != null)
            {
                character1.Relations.FirstOrDefault(rel => rel.Character == character2).Value = relations;
            }
            else
            {
                character1.Relations.Add(new Relation { Character = character2, Value = relations });
            }
            if (character2.Relations.FirstOrDefault(rel => rel.Character == character1) != null)
            {
                character2.Relations.FirstOrDefault(rel => rel.Character == character1).Value = relations;
            }
            else
            {
                character2.Relations.Add(new Relation { Character = character1, Value = relations });
            }
        }

        /// <summary>
        /// Разъединение двух персонажей
        /// </summary>
        /// <param name="character1">Первый персонаж</param>
        /// <param name="character2">Второй персонаж</param>
        public static void Unbind(Character character1, Character character2)
        {
            Relation rel1 = character1.Relations.FirstOrDefault(rel => rel.Character == character2);
            Relation rel2 = character2.Relations.FirstOrDefault(rel => rel.Character == character1);
            if (rel1 != null)
            {
                character1.Relations.Remove(rel1);
            }
            if (rel2 != null)
            {
                character2.Relations.Remove(rel2);
            }
        }

        /// <summary>
        /// Связывание персонажа с локацией
        /// </summary>
        /// <param name="character">Персонаж</param>
        /// <param name="location">Локация</param>
        public static void Bind(Character character, Location location)
        {
            if (!character.Locations.Contains(location))
            {
                character.Locations.Add(location);
            }
            if (!location.Characters.Contains(character))
            {
                location.Characters.Add(character);
            }
        }

        /// <summary>
        /// Разъединение персонажа с локацией
        /// </summary>
        /// <param name="character">Персонаж</param>
        /// <param name="location">Локация</param>
        public static void Unbind(Character character, Location location)
        {
            if (character.Locations.Contains(location))
            {
                character.Locations.Remove(location);
            }
            if (location.Characters.Contains(character))
            {
                location.Characters.Remove(character);
            }
        }

        /// <summary>
        /// Связывание персонажа с предметом
        /// </summary>
        /// <param name="character">Персонаж</param>
        /// <param name="item">Предмет</param>
        public static void Bind(Character character, Item item)
        {
            if (!character.Items.Contains(item))
            {
                character.Items.Add(item);
            }
            if (item.Host != null)
            {
                item.Host.Items.Remove(item);
            }
            item.Host = character;
        }

        /// <summary>
        /// Разъединение персонажа с предметом
        /// </summary>
        /// <param name="character">Персонаж</param>
        /// <param name="item">Предмет</param>
        public static void Unbind(Character character, Item item)
        {
            if (character.Items.Contains(item))
            {
                character.Items.Remove(item);
            }
            if (item.Host == character)
            {
                item.Host = null;
            }
        }

        /// <summary>
        /// Связывание персонажа с событием
        /// </summary>
        /// <param name="character">Персонаж</param>
        /// <param name="event">Событие</param>
        public static void Bind(Character character, Event @event)
        {
            if (!character.Events.Contains(@event))
            {
                character.Events.Add(@event);
            }
            if (!@event.Characters.Contains(character))
            {
                @event.Characters.Add(character);
            }
        }

        /// <summary>
        /// Разъединение персонажа с событием
        /// </summary>
        /// <param name="character">Персонаж</param>
        /// <param name="event">Событие</param>
        public static void Unbind(Character character, Event @event)
        {
            if (character.Events.Contains(@event))
            {
                character.Events.Remove(@event);
            }
            if (@event.Characters.Contains(character))
            {
                @event.Characters.Remove(character);
            }
        }

        /// <summary>
        /// Связывание локации с персонажем
        /// </summary>
        /// <param name="location">Локация</param>
        /// <param name="character">Персонаж</param>
        public static void Bind(Location location, Character character)
        {
            Bind(character, location);
        }

        /// <summary>
        /// Разъединение локации с персонажем
        /// </summary>
        /// <param name="location">Локация</param>
        /// <param name="character">Персонаж</param>
        public static void Unbind(Location location, Character character)
        {
            Unbind(character, location);
        }

        /// <summary>
        /// Связывание локации с событием
        /// </summary>
        /// <param name="location">Локация</param>
        /// <param name="item">Событие</param>
        public static void Bind(Location location, Item item)
        {
            if (!location.Items.Contains(item))
            {
                location.Items.Add(item);
            }
            if (item.Location != null)
            {
                item.Location.Items.Remove(item);
            }
            item.Location = location;
        }

        /// <summary>
        /// Разъединение локации с предметом
        /// </summary>
        /// <param name="location">Локация</param>
        /// <param name="item">Предмет</param>
        public static void Unbind(Location location, Item item)
        {
            if (location.Items.Contains(item))
            {
                location.Items.Remove(item);
            }
            if (item.Location == location)
            {
                item.Location = null;
            }
        }

        /// <summary>
        /// Связывание локации с событием
        /// </summary>
        /// <param name="location">Локация</param>
        /// <param name="event">Событие</param>
        public static void Bind(Location location, Event @event)
        {
            if (!location.Events.Contains(@event))
            {
                location.Events.Add(@event);
            }
            if (!@event.Locations.Contains(location))
            {
                @event.Locations.Add(location);
            }
        }

        /// <summary>
        /// Разъединение локации с событием
        /// </summary>
        /// <param name="location">Локация</param>
        /// <param name="event">Событие</param>
        public static void Unbind(Location location, Event @event)
        {
            if (location.Events.Contains(@event))
            {
                location.Events.Remove(@event);
            }
            if (@event.Locations.Contains(location))
            {
                @event.Locations.Remove(location);
            }
        }

        /// <summary>
        /// Связывание предмета с персонажем
        /// </summary>
        /// <param name="item">Предмет</param>
        /// <param name="character">Персонаж</param>
        public static void Bind(Item item, Character character)
        {
            Bind(character, item);
        }

        /// <summary>
        /// Разъединение предмета с персонажем
        /// </summary>
        /// <param name="item">Предмет</param>
        /// <param name="character">Персонаж</param>
        public static void Unbind(Item item, Character character)
        {
            Unbind(character, item);
        }

        /// <summary>
        /// Связывание предмета с локацией
        /// </summary>
        /// <param name="item">Предмет</param>
        /// <param name="location">Локация</param>
        public static void Bind(Item item, Location location)
        {
            Bind(location, item);
        }

        /// <summary>
        /// Разъединение предмета с локацией
        /// </summary>
        /// <param name="item">Предмет</param>
        /// <param name="location">Локация</param>
        public static void Unbind(Item item, Location location)
        {
            Unbind(location, item);
        }

        /// <summary>
        /// Связывание предмета с событием
        /// </summary>
        /// <param name="item">Предмет</param>
        /// <param name="event">Событие</param>
        public static void Bind(Item item, Event @event)
        {
            if (!item.Events.Contains(@event))
            {
                item.Events.Add(@event);
            }
            if (!@event.Items.Contains(item))
            {
                @event.Items.Add(item);
            }
        }

        /// <summary>
        /// Разъединение предмета с событием
        /// </summary>
        /// <param name="item">Предмет</param>
        /// <param name="event">Событие</param>
        public static void Unbind(Item item, Event @event)
        {
            if (item.Events.Contains(@event))
            {
                item.Events.Remove(@event);
            }
            if (@event.Items.Contains(item))
            {
                @event.Items.Remove(item);
            }
        }

        /// <summary>
        /// Связывание события с персонажем
        /// </summary>
        /// <param name="event"></param>
        /// <param name="character"></param>
        public static void Bind(Event @event, Character character)
        {
            Bind(character, @event);
        }
        
        /// <summary>
        /// Разъединение события с персонажем
        /// </summary>
        /// <param name="event">Событие</param>
        /// <param name="character">Персонаж</param>
        public static void Unbind(Event @event, Character character)
        {
            Unbind(character, @event);
        }

        /// <summary>
        /// Связывание события с локацией
        /// </summary>
        /// <param name="event">Событие</param>
        /// <param name="location">Локация</param>
        public static void Bind(Event @event, Location location)
        {
            Bind(location, @event);
        }

        /// <summary>
        /// Разъединение события с локацией
        /// </summary>
        /// <param name="event">Событие</param>
        /// <param name="location">Локация</param>
        public static void Unbind(Event @event, Location location)
        {
            Unbind(location, @event);
        }

        /// <summary>
        /// Связывание события с предметом
        /// </summary>
        /// <param name="event">Событие</param>
        /// <param name="item">Предмет</param>
        public static void Bind(Event @event, Item item)
        {
            Bind(item, @event);
        }

        /// <summary>
        /// Разъединение события с предметом
        /// </summary>
        /// <param name="event">Событие</param>
        /// <param name="item">Предмет</param>
        public static void Unbind(Event @event, Item item)
        {
            Unbind(item, @event);
        }
    }
}
