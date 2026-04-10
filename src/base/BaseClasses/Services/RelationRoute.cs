using System;
using BaseClasses.Enum;
using BaseClasses.Interface;

namespace BaseClasses.Services
{
    /// <summary>
    /// Структура для определения маршрута связи между элементами истории, включающая типы элементов и ключ параметра.
    /// </summary>
    public readonly struct RelationRoute : IEquatable<RelationRoute>
    {
        /// <summary>
        /// Тип элемента-источника.
        /// </summary>
        public ElemType SourceType { get; }
        
        /// <summary>
        /// Тип элемента-цели.
        /// </summary>
        public ElemType TargetType { get; }
        
        /// <summary>
        /// Ключ параметра, определяющий тип связи между элементами.
        /// </summary>
        public IParamKey ParamKey { get; }

        /// <summary>
        /// Конструктор для создания маршрута связи между элементами истории.
        /// </summary>
        /// <param name="sourceType">Тип элемента-источника.</param>
        /// <param name="targetType">Тип элемента-цели.</param>
        /// <param name="paramKey">Ключ параметра, определяющий тип связи между элементами.</param>
        public RelationRoute(ElemType sourceType, ElemType targetType, IParamKey paramKey)
        {
            SourceType = sourceType;
            TargetType = targetType;
            ParamKey = paramKey;
        }
        
        public bool Equals(RelationRoute other)
        {
            return SourceType == other.SourceType && TargetType == other.TargetType && ParamKey.Equals(other.ParamKey);
        }

        public override bool Equals(object? obj)
        {
            return obj is RelationRoute other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)SourceType, (int)TargetType, ParamKey);
        }
    }
}