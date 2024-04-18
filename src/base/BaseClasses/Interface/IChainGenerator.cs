using BaseClasses.Model;

namespace BaseClasses.Interface
{
    /// <summary>
    /// Интерфейс генератора цепочки частей истории
    /// </summary>
    public interface IChainGenerator
    {
        /// <summary>
        /// Генерация цепочки частей истории
        /// </summary>
        /// <typeparam name="T">Тип компонента</typeparam>
        /// <param name="plot">История</param>
        /// <param name="preparedPart">Подготовленная часть</param>
        /// <param name="generationQueue">Очередь генерации</param>
        /// <param name="recursion">Глубина рекурсии</param>
        /// <returns>Сгенерированная цепочка частей</returns>
        public Task<T> GenerateChainAsync<T>(Plot plot,
                                             T preparedPart,
                                             Queue<object> generationQueue = null,
                                             int recursion = 3);
    }
}
