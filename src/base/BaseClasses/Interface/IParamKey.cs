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
    }
}