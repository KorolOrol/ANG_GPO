using BaseClasses.Model;

namespace BaseClasses.Interface
{
    /// <summary>
    /// Интерфейс генератора
    /// </summary>
    public interface IGenerator
    {
        /// <summary>
        /// Генерация персонажа
        /// </summary>
        /// <param name="characters">Персонажи, уже существующие в истории</param>
        /// <param name="locations">Локации, уже существующие в истории</param>
        /// <param name="items">Предметы, уже существующие в истории</param>
        /// <param name="events">События, уже существующие в истории</param>
        /// <returns>Сгенерированный персонаж</returns>
        public async Task<Character> GenerateCharacter(List<Character>? characters = null,
                                                       List<Location>? locations = null,
                                                       List<Item>? items = null,
                                                       List<Event>? events = null)
        {
            await Task.Run(() => { });
            throw new NotImplementedException();
        }

        /// <summary>
        /// Генерация локации
        /// </summary>
        /// <param name="characters">Персонажи, уже существующие в истории</param>
        /// <param name="locations">Локации, уже существующие в истории</param>
        /// <param name="items">Предметы, уже существующие в истории</param>
        /// <param name="events">События, уже существующие в истории</param>
        /// <returns>Сгенерированная локация</returns>
        public async Task<Location> GenerateLocation(List<Character>? characters = null,
                                                     List<Location>? locations = null,
                                                     List<Item>? items = null,
                                                     List<Event>? events = null)
        {
            await Task.Run(() => { });
            throw new NotImplementedException();
        }

        /// <summary>
        /// Генерация предмета
        /// </summary>
        /// <param name="characters">Персонажи, уже существующие в истории</param>
        /// <param name="locations">Локации, уже существующие в истории</param>
        /// <param name="items">Предметы, уже существующие в истории</param>
        /// <param name="events">События, уже существующие в истории</param>
        /// <returns>Сгенерированный предмет</returns>
        public async Task<Item> GenerateItem(List<Character>? characters = null,
                                             List<Location>? locations = null,
                                             List<Item>? items = null,
                                             List<Event>? events = null)
        {
            await Task.Run(() => { });
            throw new NotImplementedException();
        }

        /// <summary>
        /// Генерация события
        /// </summary>
        /// <param name="characters">Персонажи, уже существующие в истории</param>
        /// <param name="locations">Локации, уже существующие в истории</param>
        /// <param name="items">Предметы, уже существующие в истории</param>
        /// <param name="events">События, уже существующие в истории</param>
        /// <returns>Сгенерированное событие</returns>
        public async Task<Event> GenerateEvent(List<Character>? characters = null,
                                               List<Location>? locations = null,
                                               List<Item>? items = null,
                                               List<Event>? events = null)
        {
            await Task.Run(() => { });
            throw new NotImplementedException();
        }

    }
}
