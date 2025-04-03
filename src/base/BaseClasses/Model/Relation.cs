using BaseClasses.Interface;
using System.Text.Json.Serialization;

namespace BaseClasses.Model
{
    /// <summary>
    /// Отношение персонажа с другим персонажем
    /// </summary>
    [JsonDerivedType(typeof(Relation), typeDiscriminator: "Relation")]
    public class Relation : IEquatable<Relation>
    {
        /// <summary>
        /// Персонаж
        /// </summary>
        public required IElement Character { get; set; }

        /// <summary>
        /// Значение отношения
        /// </summary>
        public double Value { get; set; }

        public override string ToString()
        {
            return $"{{{Character.Name}, {Value}}}";
        }

        public bool Equals(Relation? other)
        {
            return true;
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (GetType() != other.GetType()) return false;
            return Character.Equals(other.Character) &&
                Value == other.Value;
        }
    }
}
