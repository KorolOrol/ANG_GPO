using BaseClasses_V2.Model;

namespace BaseClasses_V2.Interface
{
    /// <summary>
    /// Интерфейс генератора цепочки элементов истории
    /// </summary>
    public interface IChainGenerator
    {
        /// <summary>
        /// Генерация цепочки элементов истории
        /// </summary>
        /// <param name="plot">История</param>
        /// <param name="preparedElement">Подготовленный элемент</param>
        /// <param name="generationQueue">Очередь генерации</param>
        /// <param name="recursion">Глубина рекурсии</param>
        /// <returns>Сгенерированная цепочка элементов</returns>
        public Task<Element> GenerateChainAsync(Plot plot,
                                              Element preparedElement,
                                              Queue<(Element, Element, int)> generationQueue = null,
                                              int recursion = 3);
    }
}
