using BaseClasses_V2.Model;

namespace BaseClasses_V2.Interface
{
    /// <summary>
    /// Интерфейс генератора
    /// </summary>
    public interface IGenerator
    {
        /// <summary>
        /// Генерация элемента истории
        /// </summary>
        /// <param name="plot">История</param>
        /// <param name="preparedElement">Подготовленный элемент</param>
        /// <returns>Сгенерированный элемент</returns>
        public Task<Element> GenerateAsync(Plot plot, Element preparedElement);
    }
}
