using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
