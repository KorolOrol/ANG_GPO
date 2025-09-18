using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Enum;
namespace BaseClasses.Services
{
    /// <summary>
    /// Сервис для связывания элементов истории
    /// </summary>
    public static class Binder
    {
        /// <summary>
        /// Связывание двух частей истории
        /// </summary>
        /// <param name="element1">Первая часть истории</param>
        /// <param name="element2">Вторая часть истории</param>
        /// <param name="param">Параметр отношений (если связываются два персонажа)</param>
        public static void Bind(IElement element1, IElement element2, double param = 0) 
        { 
            switch (element1.Type)
            {
                case ElemType.Character:
                    switch (element2.Type)
                    {
                        case ElemType.Character:
                            BindCharacters(element1, element2, param);
                            break;
                        case ElemType.Location:
                            BindCharLoc(element1, element2);
                            break;
                        case ElemType.Item:
                            BindCharItem(element1, element2);
                            break;
                        case ElemType.Event:
                            BindCharEvent(element1, element2);
                            break;
                    }
                    break;
                case ElemType.Location:
                    switch (element2.Type)
                    {
                        case ElemType.Character:
                            BindLocChar(element1, element2);
                            break;
                        case ElemType.Item:
                            BindLocItem(element1, element2);
                            break;
                        case ElemType.Event:
                            BindLocEvent(element1, element2);
                            break;
                    }
                    break;
                case ElemType.Item:
                    switch (element2.Type)
                    {
                        case ElemType.Character:
                            BindItemChar(element1, element2);
                            break;
                        case ElemType.Location:
                            BindItemLoc(element1, element2);
                            break;
                        case ElemType.Event:
                            BindItemEvent(element1, element2);
                            break;
                    }
                    break;
                case ElemType.Event:
                    switch (element2.Type)
                    {
                        case ElemType.Character:
                            BindEventChar(element1, element2);
                            break;
                        case ElemType.Location:
                            BindEventLoc(element1, element2);
                            break;
                        case ElemType.Item:
                            BindEventItem(element1, element2);
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// Разъединение двух частей истории
        /// </summary>
        /// <param name="element1">Первая часть истории</param>
        /// <param name="element2">Вторая часть истории</param>
        public static void Unbind(IElement element1, IElement element2) 
        { 
            switch (element1.Type)
            {
                case ElemType.Character:
                    switch (element2.Type)
                    {
                        case ElemType.Character:
                            UnbindCharacters(element1, element2);
                            break;
                        case ElemType.Location:
                            UnbindCharLoc(element1, element2);
                            break;
                        case ElemType.Item:
                            UnbindCharItem(element1, element2);
                            break;
                        case ElemType.Event:
                            UnbindCharEvent(element1, element2);
                            break;
                    }
                    break;
                case ElemType.Location:
                    switch (element2.Type)
                    {
                        case ElemType.Character:
                            UnbindLocChar(element1, element2);
                            break;
                        case ElemType.Item:
                            UnbindLocItem(element1, element2);
                            break;
                        case ElemType.Event:
                            UnbindLocEvent(element1, element2);
                            break;
                    }
                    break;
                case ElemType.Item:
                    switch (element2.Type)
                    {
                        case ElemType.Character:
                            UnbindItemChar(element1, element2);
                            break;
                        case ElemType.Location:
                            UnbindItemLoc(element1, element2);
                            break;
                        case ElemType.Event:
                            UnbindItemEvent(element1, element2);
                            break;
                    }
                    break;
                case ElemType.Event:
                    switch (element2.Type)
                    {
                        case ElemType.Character:
                            UnbindEventChar(element1, element2);
                            break;
                        case ElemType.Location:
                            UnbindEventLoc(element1, element2);
                            break;
                        case ElemType.Item:
                            UnbindEventItem(element1, element2);
                            break;
                    }
                    break;
            }
        }

        /// <summary>
        /// Вспомогательный метод для проверки валидности связывания элементов
        /// </summary>
        private static bool IsValidBinding(IElement element1, ElemType type1, IElement element2, ElemType type2)
        {
            return element1 != null && element2 != null && element1 != element2 &&
                   element1.Type == type1 && element2.Type == type2;
        }

        /// <summary>
        /// Связывание двух персонажей
        /// </summary>
        /// <param name="character1">Первый персонаж</param>
        /// <param name="character2">Второй персонаж</param>
        /// <param name="relations">Отношения между персонажами</param>
        private static void BindCharacters(IElement character1, IElement character2, double relations)
        {
            if (!IsValidBinding(character1, ElemType.Character, character2, ElemType.Character) || relations == 0)
            {
                return;
            }

            List<Relation> relations1;
            if (!character1.Params.ContainsKey("Relations"))
            {
                relations1 = new List<Relation>();
                character1.Params.Add("Relations", relations1);
            }
            else
            {
                relations1 = (List<Relation>)character1.Params["Relations"];
            }

            List<Relation> relations2;
            if (!character2.Params.ContainsKey("Relations"))
            {
                relations2 = new List<Relation>();
                character2.Params.Add("Relations", relations2);
            }
            else
            {
                relations2 = (List<Relation>)character2.Params["Relations"];
            }

            var rel1 = relations1.FirstOrDefault(rel => rel.Character == character2);
            if (rel1 != null)
            {
                rel1.Value = relations;
            }
            else
            {
                relations1.Add(new Relation { Character = character2, Value = relations });
            }

            var rel2 = relations2.FirstOrDefault(rel => rel.Character == character1);
            if (rel2 != null)
            {
                rel2.Value = relations;
            }
            else
            {
                relations2.Add(new Relation { Character = character1, Value = relations });
            }
        }

        /// <summary>
        /// Разъединение двух персонажей
        /// </summary>
        /// <param name="character1">Первый персонаж</param>
        /// <param name="character2">Второй персонаж</param>
        private static void UnbindCharacters(IElement character1, IElement character2)
        {
            if (!IsValidBinding(character1, ElemType.Character, character2, ElemType.Character))
            {
                return;
            }

            if (character1.Params.ContainsKey("Relations"))
            {
                Relation rel1 = ((List<Relation>)character1.Params["Relations"]).
                                FirstOrDefault(rel => rel.Character == character2);
                if (rel1 != null)
                {
                    ((List<Relation>)character1.Params["Relations"]).Remove(rel1);
                }
            }

            if (character2.Params.ContainsKey("Relations"))
            {
                Relation rel2 = ((List<Relation>)character2.Params["Relations"]).
                                FirstOrDefault(rel => rel.Character == character1);
                if (rel2 != null)
                {
                    ((List<Relation>)character2.Params["Relations"]).Remove(rel2);
                }
            }
        }

        /// <summary>
        /// Связывание персонажа с локацией
        /// </summary>
        /// <param name="character">Персонаж</param>
        /// <param name="location">Локация</param>
        private static void BindCharLoc(IElement character, IElement location)
        {
            if (!IsValidBinding(character, ElemType.Character, location, ElemType.Location))
            {
                return;
            }

            List<IElement> locs;
            if (!character.Params.ContainsKey("Locations"))
            {
                locs = new List<IElement>();
                character.Params.Add("Locations", locs);
            }
            else
            {
                locs = (List<IElement>)character.Params["Locations"];
            }

            List<IElement> chars;
            if (!location.Params.ContainsKey("Characters"))
            {
                chars = new List<IElement>();
                location.Params.Add("Characters", chars);
            }
            else
            {
                chars = (List<IElement>)location.Params["Characters"];
            }

            if (!locs.Contains(location))
            {
                locs.Add(location);
            }

            if (!chars.Contains(character))
            {
                chars.Add(character);
            }
        }

        /// <summary>
        /// Разъединение персонажа с локацией
        /// </summary>
        /// <param name="character">Персонаж</param>
        /// <param name="location">Локация</param>
        private static void UnbindCharLoc(IElement character, IElement location)
        {
            if (!IsValidBinding(character, ElemType.Character, location, ElemType.Location))
            {
                return;
            }

            if (character.Params.ContainsKey("Locations") &&
                ((List<IElement>)character.Params["Locations"]).Contains(location))
            {
                ((List<IElement>)character.Params["Locations"]).Remove(location);
            }

            if (location.Params.ContainsKey("Characters") &&
                ((List<IElement>)location.Params["Characters"]).Contains(character))
            {
                ((List<IElement>)location.Params["Characters"]).Remove(character);
            }
        }

        /// <summary>
        /// Связывание персонажа с предметом
        /// </summary>
        /// <param name="character">Персонаж</param>
        /// <param name="item">Предмет</param>
        private static void BindCharItem(IElement character, IElement item)
        {
            if (!IsValidBinding(character, ElemType.Character, item, ElemType.Item))
            {
                return;
            }

            List<IElement> items;
            if (!character.Params.ContainsKey("Items"))
            {
                items = new List<IElement>();
                character.Params.Add("Items", items);
            }
            else
            {
                items = (List<IElement>)character.Params["Items"];
            }

            if (!items.Contains(item))
            {
                items.Add(item);
            }

            if (!item.Params.ContainsKey("Host"))
            {
                item.Params.Add("Host", character);
            }
            else
            {
                if ((IElement)item.Params["Host"] != character)
                {
                    UnbindCharItem((IElement)item.Params["Host"], item);
                    item.Params["Host"] = character;
                }
            }
        }

        /// <summary>
        /// Разъединение персонажа с предметом
        /// </summary>
        /// <param name="character">Персонаж</param>
        /// <param name="item">Предмет</param>
        private static void UnbindCharItem(IElement character, IElement item)
        {
            if (!IsValidBinding(character, ElemType.Character, item, ElemType.Item))
            {
                return;
            }

            if (character.Params.ContainsKey("Items") &&
                ((List<IElement>)character.Params["Items"]).Contains(item))
            {
                ((List<IElement>)character.Params["Items"]).Remove(item);
            }

            if (item.Params.ContainsKey("Host") &&
                ((IElement)item.Params["Host"]) == character)
            {
                item.Params["Host"] = null;
            }
        }

        /// <summary>
        /// Связывание персонажа с событием
        /// </summary>
        /// <param name="character">Персонаж</param>
        /// <param name="event">Событие</param>
        private static void BindCharEvent(IElement character, IElement @event)
        {
            if (!IsValidBinding(character, ElemType.Character, @event, ElemType.Event))
            {
                return;
            }

            List<IElement> events;
            if (!character.Params.ContainsKey("Events"))
            {
                events = new List<IElement>();
                character.Params.Add("Events", events);
            }
            else
            {
                events = (List<IElement>)character.Params["Events"];
            }

            List<IElement> chars;
            if (!@event.Params.ContainsKey("Characters"))
            {
                chars = new List<IElement>();
                @event.Params.Add("Characters", chars);
            }
            else
            {
                chars = (List<IElement>)@event.Params["Characters"];
            }

            if (!events.Contains(@event))
            {
                events.Add(@event);
            }

            if (!chars.Contains(character))
            {
                chars.Add(character);
            }
        }

        /// <summary>
        /// Разъединение персонажа с событием
        /// </summary>
        /// <param name="character">Персонаж</param>
        /// <param name="event">Событие</param>
        private static void UnbindCharEvent(IElement character, IElement @event)
        {
            if (!IsValidBinding(character, ElemType.Character, @event, ElemType.Event))
            {
                return;
            }

            if (character.Params.ContainsKey("Events") &&
                ((List<IElement>)character.Params["Events"]).Contains(@event))
            {
                ((List<IElement>)character.Params["Events"]).Remove(@event);
            }

            if (@event.Params.ContainsKey("Characters") &&
                ((List<IElement>)@event.Params["Characters"]).Contains(character))
            {
                ((List<IElement>)@event.Params["Characters"]).Remove(character);
            }
        }

        /// <summary>
        /// Связывание локации с персонажем
        /// </summary>
        /// <param name="location">Локация</param>
        /// <param name="character">Персонаж</param>
        private static void BindLocChar(IElement location, IElement character)
        {
            BindCharLoc(character, location);
        }

        /// <summary>
        /// Разъединение локации с персонажем
        /// </summary>
        /// <param name="location">Локация</param>
        /// <param name="character">Персонаж</param>
        private static void UnbindLocChar(IElement location, IElement character)
        {
            UnbindCharLoc(character, location);
        }

        /// <summary>
        /// Связывание локации с предметом
        /// </summary>
        /// <param name="location">Локация</param>
        /// <param name="item">Предмет</param>
        private static void BindLocItem(IElement location, IElement item)
        {
            if (!IsValidBinding(location, ElemType.Location, item, ElemType.Item))
            {
                return;
            }

            List<IElement> items;
            if (!location.Params.ContainsKey("Items"))
            {
                items = new List<IElement>();
                location.Params.Add("Items", items);
            }
            else
            {
                items = (List<IElement>)location.Params["Items"];
            }

            if (!items.Contains(item))
            {
                items.Add(item);
            }

            if (!item.Params.ContainsKey("Location"))
            {
                item.Params.Add("Location", location);
            }
            else
            {
                if ((IElement)item.Params["Location"] != location)
                {
                    UnbindLocItem((IElement)item.Params["Location"], item);
                    item.Params["Location"] = location;
                }
            }
        }

        /// <summary>
        /// Разъединение локации с предметом
        /// </summary>
        /// <param name="location">Локация</param>
        /// <param name="item">Предмет</param>
        private static void UnbindLocItem(IElement location, IElement item)
        {
            if (!IsValidBinding(location, ElemType.Location, item, ElemType.Item))
            {
                return;
            }

            if (location.Params.ContainsKey("Items") &&
                ((List<IElement>)location.Params["Items"]).Contains(item))
            {
                ((List<IElement>)location.Params["Items"]).Remove(item);
            }

            if (item.Params.ContainsKey("Location") &&
                ((IElement)item.Params["Location"]) == location)
            {
                item.Params["Location"] = null;
            }
        }

        /// <summary>
        /// Связывание локации с событием
        /// </summary>
        /// <param name="location">Локация</param>
        /// <param name="event">Событие</param>
        private static void BindLocEvent(IElement location, IElement @event)
        {
            if (!IsValidBinding(location, ElemType.Location, @event, ElemType.Event))
            {
                return;
            }

            List<IElement> events;
            if (!location.Params.ContainsKey("Events"))
            {
                events = new List<IElement>();
                location.Params.Add("Events", events);
            }
            else
            {
                events = (List<IElement>)location.Params["Events"];
            }

            List<IElement> locs;
            if (!@event.Params.ContainsKey("Locations"))
            {
                locs = new List<IElement>();
                @event.Params.Add("Locations", locs);
            }
            else
            {
                locs = (List<IElement>)@event.Params["Locations"];
            }

            if (!events.Contains(@event))
            {
                events.Add(@event);
            }

            if (!locs.Contains(location))
            {
                locs.Add(location);
            }
        }

        /// <summary>
        /// Разъединение локации с событием
        /// </summary>
        /// <param name="location">Локация</param>
        /// <param name="event">Событие</param>
        private static void UnbindLocEvent(IElement location, IElement @event)
        {
            if (!IsValidBinding(location, ElemType.Location, @event, ElemType.Event))
            {
                return;
            }

            if (location.Params.ContainsKey("Events") &&
                ((List<IElement>)location.Params["Events"]).Contains(@event))
            {
                ((List<IElement>)location.Params["Events"]).Remove(@event);
            }

            if (@event.Params.ContainsKey("Locations") &&
                ((List<IElement>)@event.Params["Locations"]).Contains(location))
            {
                ((List<IElement>)@event.Params["Locations"]).Remove(location);
            }
        }

        /// <summary>
        /// Связывание предмета с персонажем
        /// </summary>
        /// <param name="item">Предмет</param>
        /// <param name="character">Персонаж</param>
        private static void BindItemChar(IElement item, IElement character)
        {
            BindCharItem(character, item);
        }

        /// <summary>
        /// Разъединение предмета с персонажем
        /// </summary>
        /// <param name="item">Предмет</param>
        /// <param name="character">Персонаж</param>
        private static void UnbindItemChar(IElement item, IElement character)
        {
            UnbindCharItem(character, item);
        }

        /// <summary>
        /// Связывание предмета с локацией
        /// </summary>
        /// <param name="item">Предмет</param>
        /// <param name="location">Локация</param>
        private static void BindItemLoc(IElement item, IElement location)
        {
            BindLocItem(location, item);
        }

        /// <summary>
        /// Разъединение предмета с локацией
        /// </summary>
        /// <param name="item">Предмет</param>
        /// <param name="location">Локация</param>
        private static void UnbindItemLoc(IElement item, IElement location)
        {
            UnbindLocItem(location, item);
        }

        /// <summary>
        /// Связывание предмета с событием
        /// </summary>
        /// <param name="item">Предмет</param>
        /// <param name="event">Событие</param>
        private static void BindItemEvent(IElement item, IElement @event)
        {
            if (!IsValidBinding(item, ElemType.Item, @event, ElemType.Event))
            {
                return;
            }

            List<IElement> events;
            if (!item.Params.ContainsKey("Events"))
            {
                events = new List<IElement>();
                item.Params.Add("Events", events);
            }
            else
            {
                events = (List<IElement>)item.Params["Events"];
            }

            List<IElement> items;
            if (!@event.Params.ContainsKey("Items"))
            {
                items = new List<IElement>();
                @event.Params.Add("Items", items);
            }
            else
            {
                items = (List<IElement>)@event.Params["Items"];
            }

            if (!events.Contains(@event))
            {
                events.Add(@event);
            }

            if (!items.Contains(item))
            {
                items.Add(item);
            }
        }

        /// <summary>
        /// Разъединение предмета с событием
        /// </summary>
        /// <param name="item">Предмет</param>
        /// <param name="event">Событие</param>
        private static void UnbindItemEvent(IElement item, IElement @event)
        {
            if (!IsValidBinding(item, ElemType.Item, @event, ElemType.Event))
            {
                return;
            }

            if (item.Params.ContainsKey("Events") &&
                ((List<IElement>)item.Params["Events"]).Contains(@event))
            {
                ((List<IElement>)item.Params["Events"]).Remove(@event);
            }

            if (@event.Params.ContainsKey("Items") &&
                ((List<IElement>)@event.Params["Items"]).Contains(item))
            {
                ((List<IElement>)@event.Params["Items"]).Remove(item);
            }
        }

        /// <summary>
        /// Связывание события с персонажем
        /// </summary>
        /// <param name="event"></param>
        /// <param name="character"></param>
        private static void BindEventChar(IElement @event, IElement character)
        {
            BindCharEvent(character, @event);
        }
        
        /// <summary>
        /// Разъединение события с персонажем
        /// </summary>
        /// <param name="event">Событие</param>
        /// <param name="character">Персонаж</param>
        private static void UnbindEventChar(IElement @event, IElement character)
        {
            UnbindCharEvent(character, @event);
        }

        /// <summary>
        /// Связывание события с локацией
        /// </summary>
        /// <param name="event">Событие</param>
        /// <param name="location">Локация</param>
        private static void BindEventLoc(IElement @event, IElement location)
        {
            BindLocEvent(location, @event);
        }

        /// <summary>
        /// Разъединение события с локацией
        /// </summary>
        /// <param name="event">Событие</param>
        /// <param name="location">Локация</param>
        private static void UnbindEventLoc(IElement @event, IElement location)
        {
            UnbindLocEvent(location, @event);
        }

        /// <summary>
        /// Связывание события с предметом
        /// </summary>
        /// <param name="event">Событие</param>
        /// <param name="item">Предмет</param>
        private static void BindEventItem(IElement @event, IElement item)
        {
            BindItemEvent(item, @event);
        }

        /// <summary>
        /// Разъединение события с предметом
        /// </summary>
        /// <param name="event">Событие</param>
        /// <param name="item">Предмет</param>
        private static void UnbindEventItem(IElement @event, IElement item)
        {
            UnbindItemEvent(item, @event);
        }
    }
}
