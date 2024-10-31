using BaseClasses.Interface;
using System.Text.Json.Serialization;

namespace BaseClasses.Model
{
    /// <summary>
    /// Отношение персонажа с другим персонажем
    /// </summary>
    [JsonDerivedType(typeof(Relation), typeDiscriminator: "Relation")]
    public class Relation
    {
        /// <summary>
        /// Персонаж
        /// </summary>
        public IElement Character { get; set; }

        /// <summary>
        /// Значение отношения
        /// </summary>
        public double Value { get; set; }

        public override string ToString()
        {
            return $"{{{Character.Name}, {Value}}}";
        }
    }
}
