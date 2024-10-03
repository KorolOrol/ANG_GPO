using BaseClasses.Model;

namespace BaseClasses.Interface
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
        public Task<IElement> GenerateAsync(Plot plot, IElement preparedElement);
    }
}
