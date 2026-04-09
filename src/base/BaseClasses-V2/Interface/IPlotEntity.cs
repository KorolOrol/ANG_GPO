using MessagePack;
using BaseClasses_V2.Model;

namespace BaseClasses_V2.Interface
{
    [Union(0, typeof(IElement))]
    [Union(1, typeof(Relation))]
    public interface IPlotEntity
    {
        /// <summary>
        /// Properties contained in an entity
        /// </summary>
        [Key(0)]
        public Dictionary<string, object> Params { get; set; }
        
        /// <summary>
        /// Labels contained in an entity
        /// </summary>
        [Key(1)]

        public HashSet<string> Labels { get; set; }

        [Key(2)]
        public string Hash { get; protected internal set; }
    }
}