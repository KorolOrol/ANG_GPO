using System.Collections.Generic;
using BaseClasses.Interface;
using BaseClasses.Enum;
using BaseClasses.Services;
using System.Text.Json.Serialization;

namespace BaseClasses.Model
{
    /// <summary>
    /// История
    /// </summary>
    public class Plot
    {
        /// <summary>
        /// Элементы истории
        /// </summary>
        public List<IElement> Elements { get; set; } = new List<IElement>();

        /// <summary>
        /// Добавление элемента в историю
        /// </summary>
        /// <param name="element">Элемент</param>
        public void Add(IElement element) 
        {
            if (Elements.Contains(element)) return;
            if (element.Time == -1) element.Time = Time++;
            Elements.Add(element); 
        }
        
        /// <summary>
        /// Удаление элемента из истории
        /// </summary>
        /// <param name="element">Удаляемый элемент</param>
        public void Remove(IElement element)
        {
            if (!Elements.Contains(element)) return;
            foreach (var e in Elements)
            {
                Binder.Unbind(element, e);
            }
            Elements.Remove(element);
        }
            
        /// <summary>
        /// Персонажи
        /// </summary>
        [JsonIgnore]
        public List<IElement> Characters => Elements.FindAll(e => e.Type == ElemType.Character);

        /// <summary>
        /// Локации
        /// </summary>
        [JsonIgnore]
        public List<IElement> Locations => Elements.FindAll(e => e.Type == ElemType.Location);

        /// <summary>
        /// Предметы
        /// </summary>
        [JsonIgnore]
        public List<IElement> Items => Elements.FindAll(e => e.Type == ElemType.Item);

        /// <summary>
        /// События
        /// </summary>
        [JsonIgnore]
        public List<IElement> Events => Elements.FindAll(e => e.Type == ElemType.Event);

        /// <summary>
        /// Время
        /// </summary>
        public int Time { get; set; } = 0;

        /// <summary>
        /// Полная информация об истории
        /// </summary>
        /// <returns>Полная информация об истории</returns>
        public string FullInfo()
        {
            string info = "";
            foreach (var c in Characters)
            {
                info += c.FullInfo() + "\n";
            }
            foreach (var l in Locations)
            {
                info += l.FullInfo() + "\n";
            }
            foreach (var i in Items)
            {
                info += i.FullInfo() + "\n";
            }
            foreach (var e in Events)
            {
                info += e.FullInfo() + "\n";
            }
            return info;
        }
    }
}
