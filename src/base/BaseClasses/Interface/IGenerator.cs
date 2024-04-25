using BaseClasses.Model;

namespace BaseClasses.Interface
{
    /// <summary>
    /// Интерфейс генератора
    /// </summary>
    public interface IGenerator
    {
        /// <summary>
        /// Генерация компонента истории
        /// </summary>
        /// <param name="plot">История</param>
        /// <param name="preparedPart">Подготовленная часть</param>
        /// <returns>Сгенерированный компонент</returns>
        public Task<IPart> GenerateAsync(Plot plot, IPart preparedPart);
    }
}
