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
        public HashSet<IElement> Elements { get; } = new HashSet<IElement>();
        
        /// <summary>
        /// Связи между элементами истории
        /// </summary>
        public HashSet<Relation> Relations { get; } = new HashSet<Relation>();

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
        public int Time { get; set; }
    }
}
