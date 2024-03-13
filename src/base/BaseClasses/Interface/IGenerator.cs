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
        /// <param name="plot">История</param>
        /// <returns>Сгенерированный персонаж</returns>
        public async Task<Character> GenerateCharacterAsync(Plot plot)
        {
            await Task.Run(() => { });
            throw new NotImplementedException();
        }

        /// <summary>
        /// Генерация локации
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Сгенерированная локация</returns>
        public async Task<Location> GenerateLocationAsync(Plot plot)
        {
            await Task.Run(() => { });
            throw new NotImplementedException();
        }

        /// <summary>
        /// Генерация предмета
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Сгенерированный предмет</returns>
        public async Task<Item> GenerateItemAsync(Plot plot)
        {
            await Task.Run(() => { });
            throw new NotImplementedException();
        }

        /// <summary>
        /// Генерация события
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Сгенерированное событие</returns>
        public async Task<Event> GenerateEventAsync(Plot plot)
        {
            await Task.Run(() => { });
            throw new NotImplementedException();
        }

    }
}
