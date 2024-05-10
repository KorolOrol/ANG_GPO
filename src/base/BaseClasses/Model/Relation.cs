namespace BaseClasses.Model
{
    /// <summary>
    /// Отношение персонажа с другим персонажем
    /// </summary>
    public class Relation
    {
        /// <summary>
        /// Персонаж
        /// </summary>
        public Character Character { get; set; }

        /// <summary>
        /// Значение отношения
        /// </summary>
        public double Value { get; set; }
    }
}
