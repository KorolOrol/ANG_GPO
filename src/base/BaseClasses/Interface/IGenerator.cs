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
        /// <typeparam name="T">Тип компонента</typeparam>
        /// <param name="plot">История</param>
        /// <param name="preparedPart">Подготовленная часть</param>
        /// <returns>Сгенерированный компонент</returns>
        public Task<T> GenerateAsync<T>(Plot plot, T preparedPart);
    }
}
