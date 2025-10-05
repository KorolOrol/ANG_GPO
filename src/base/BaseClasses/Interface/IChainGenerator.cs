using System.Collections.Generic;
using System.Threading.Tasks;
using BaseClasses.Model;

namespace BaseClasses.Interface
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
        public Task<IElement> GenerateChainAsync(Plot plot,
                                              IElement preparedElement,
                                              Queue<(IElement, IElement, int)> generationQueue = null,
                                              int recursion = 3);
    }
}
