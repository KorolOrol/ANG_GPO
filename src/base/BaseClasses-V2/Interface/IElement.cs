using BaseClasses_V2.Enum;
using MessagePack;

namespace BaseClasses_V2.Interface
{
    /// <summary>
    /// Интерфейс элемента истории
    /// </summary>
    [Union(0, typeof(IElement))]
    public interface IElement : IPlotEntity, IEquatable<IElement>
    {
        /// <summary>
        /// Тип элемента
        /// </summary>
        [Key(0)]
        ElemType Type { get; }
        
        /// <summary>
        /// Название элемента
        /// </summary>
        [Key(1)]
        string Name { get; set; }

        /// <summary>
        /// Описание элемента
        /// </summary>
        [Key(2)]
        string Description { get; set; }

        /// <summary>
        /// Время создания элемента
        /// </summary>
        [Key(3)]
        int Time { get; set; }
        
        /// <summary>
        /// Полная информация об элементе
        /// </summary>
        string FullInfo();
        
        /// <summary>
        /// Проверка на пустоту элемента
        /// </summary>
        bool IsEmpty();
    }
}