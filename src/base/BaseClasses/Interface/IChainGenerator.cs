using BaseClasses.Model;

namespace BaseClasses.Interface
{
    /// <summary>
    /// Интерфейс генератора цепочки частей истории
    /// </summary>
    public interface IChainGenerator
    {
        /// <summary>
        /// Генерация персонажа с цепочкой
        /// </summary>
        /// <param name="plot">История</param>
        /// <param name="recursion">Глубина рекурсии</param>
        /// <param name="name">Имя персонажа</param>
        /// <returns>Сгенерированный персонаж</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<Character> GenerateCharacterChainAsync(Plot plot, int recursion = 3, string name = "")
        {
            await Task.Run(() => { });
            throw new NotImplementedException();
        }

        /// <summary>
        /// Генерация локации с цепочкой
        /// </summary>
        /// <param name="plot">История</param>
        /// <param name="recursion">Глубина рекурсии</param>
        /// <param name="name">Название локации</param>
        /// <returns>Сгенерированная локация</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<Location> GenerateLocationChainAsync(Plot plot, int recursion = 3, string name = "")
        {
            await Task.Run(() => { });
            throw new NotImplementedException();
        }

        /// <summary>
        /// Генерация предмета с цепочкой
        /// </summary>
        /// <param name="plot">История</param>
        /// <param name="recursion">Глубина рекурсии</param>
        /// <param name="name">Название предмета</param>
        /// <returns>Сгененрированный предмет</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<Item> GenerateItemChainAsync(Plot plot, int recursion = 3, string name = "")
        {
            await Task.Run(() => { });
            throw new NotImplementedException();
        }

        /// <summary>
        /// Генерация события с цепочкой
        /// </summary>
        /// <param name="plot">История</param>
        /// <param name="recursion">Глубина рекурсии</param>
        /// <param name="name">Названия события</param>
        /// <returns>Сгенерированное событие</returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<Event> GenerateEventChainAsync(Plot plot, int recursion = 3, string name = "")
        {
            await Task.Run(() => { });
            throw new NotImplementedException();
        }
    }
}
