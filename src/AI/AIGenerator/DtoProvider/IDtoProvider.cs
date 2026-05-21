using System.Collections.Generic;
using BaseClasses.Interface;
using BaseClasses.Model;

namespace AIGenerator.DtoProvider
{
    /// <summary>
    /// Интерфейс провайдера для преобразования элементов и сюжета в DTO-формат и обратно.
    /// </summary>
    public interface IDtoProvider
    {
        /// <summary>
        /// Преобразование сюжета в строку формата DTO.
        /// </summary>
        /// <param name="plot">Сюжет, который нужно преобразовать.</param>
        /// <returns>Строка, представляющая сюжет в формате DTO.</returns>
        public string ToDto(Plot plot);
        
        /// <summary>
        /// Преобразование элемента в строку формата DTO.
        /// </summary>
        /// <param name="element">Элемент, который нужно преобразовать.</param>
        /// <returns>Строка, представляющая элемент в формате DTO.</returns>
        public string ToDto(IElement element);
        
        /// <summary>
        /// Преобразование строки формата DTO обратно в элемент.
        /// Сюжет нужен для корректного восстановления связей между элементами.
        /// </summary>
        /// <param name="dto">Строка, представляющая элемент в формате DTO.</param>
        /// <param name="plot">Сюжет, в который будет добавлен восстановленный элемент.</param>
        /// <returns>Восстановленный элемент.</returns>
        public IElement FromDto(string dto, Plot plot);

        /// <summary>
        /// Получение схемы формата DTO.
        /// </summary>
        /// <returns>Строка, представляющая схему формата DTO.</returns>
        public string GetSchema();

        /// <summary>
        /// Получение новых элементов из строки формата DTO.
        /// </summary>
        /// <param name="dto">Строка, представляющая элементы в формате DTO.</param>
        /// <param name="plot">Сюжет, в который будут добавлены восстановленные элементы.</param>
        /// <returns></returns>
        public List<(IElement, IParamKey, object)> GetNewElements(string dto, Plot plot);
    }
}