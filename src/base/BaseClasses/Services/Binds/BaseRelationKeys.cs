using BaseClasses.Model.Params;

namespace BaseClasses.Services.Binds
{
    /// <summary>
    /// Ключи параметров для стандартных видов связей.
    /// </summary>
    public static class BaseRelationKeys
    {
        /// <summary>
        /// Отношение одного персонажа к другому.
        /// </summary>
        public static readonly ParamKey<double> Relationship = new ParamKey<double>("Relationship");
        
        /// <summary>
        /// Элемент находится на локации.
        /// </summary>
        public static readonly ParamKey<bool> Located = new ParamKey<bool>("Located");
        
        /// <summary>
        /// Локация содержит элемент.
        /// </summary>
        public static readonly ParamKey<bool> Locates = new ParamKey<bool>("Locates");
        
        /// <summary>
        /// Персонаж обладает предметом.
        /// </summary>
        public static readonly ParamKey<bool> Owns = new ParamKey<bool>("Owns");
        
        /// <summary>
        /// Предмет принадлежит персонажу.
        /// </summary>
        public static readonly ParamKey<bool> Owned = new ParamKey<bool>("Owned");
        
        /// <summary>
        /// Персонаж принимает участие в событии.
        /// </summary>
        public static readonly ParamKey<bool> Participates = new ParamKey<bool>("Participates");
        
        /// <summary>
        /// Событие включает персонажа.
        /// </summary>
        public static readonly ParamKey<bool> Involves = new ParamKey<bool>("Involves");
        
        /// <summary>
        /// Событие использует предмет.
        /// </summary>
        public static readonly ParamKey<bool> Uses = new ParamKey<bool>("Uses");
        
        /// <summary>
        /// Предмет используется в событии.
        /// </summary>
        public static readonly ParamKey<bool> Used = new ParamKey<bool>("Used");
    }
}