using System;

namespace BaseClasses.Interface
{
    /// <summary>
    /// Типизированный ключ параметра.
    /// </summary>
    public interface IParamKey
    {
        /// <summary>
        /// Название ключа параметра.
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// Тип значения, связанного с этим ключом параметра.
        /// </summary>
        Type ValueType { get; }
        
        /// <summary>
        /// Является ли тип значения коллекцией.
        /// </summary>
        bool IsCollection { get; }
        
        /// <summary>
        /// Пространство имен для организации ключей параметров.
        /// </summary>
        string Namespace { get; }

        /// <summary>
        /// Проверяет, соответствует ли тип переданного значения типу, определенному в этом ключе параметра.
        /// </summary>
        /// <param name="value">Значение, тип которого нужно проверить на соответствие.</param>
        /// <returns>True, если тип значения соответствует типу,
        /// определенному в этом ключе параметра, иначе False.</returns>
        bool IsTypeMatch(object? value);

        /// <summary>
        /// Пытается преобразовать переданное значение к типу, определенному в этом ключе параметра.
        /// </summary>
        /// <param name="value">Значение, которое нужно попытаться преобразовать.</param>
        /// <param name="result">Параметр, в который будет записан результат преобразования, если оно успешно.</param>
        /// <returns>True, если преобразование было успешным, иначе False.</returns>
        public bool TryConvertValue(object? value, out object? result);
    }
}