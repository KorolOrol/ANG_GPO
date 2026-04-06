using System;
using System.Collections.Generic;
using BaseClasses.Enum;
using BaseClasses.Model;
using BaseClasses.Model.Params;

namespace BaseClasses.Interface
{
    /// <summary>
    /// Интерфейс элемента истории
    /// </summary>
    public interface IElement
    {
        /// <summary>
        /// Тип элемента
        /// </summary>
        public ElemType Type { get; }

        /// <summary>
        /// Название элемента
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Описание элемента
        /// </summary>
        public string Description { get; set; }
        
        public ParamBag TypedParams { get; }

        /// <summary>
        /// Параметры элемента
        /// </summary>
        [Obsolete("Используйте TypedParams вместо Params для новых разработок.")]
        public IDictionary<string, object> Params { get; set; }

        /// <summary>
        /// Время создания элемента
        /// </summary>
        public int Time { get; set; }

        /// <summary>
        /// Полная информация об элементе
        /// </summary>
        /// <returns>Полная информация об элементе</returns>
        public string FullInfo();

        /// <summary>
        /// Проверка на пустоту
        /// </summary>
        /// <returns>Ture, если элемент пуст, иначе False</returns>
        public bool IsEmpty();
    }
}
