using MessagePack;

namespace BaseClasses_V2.Model
{
    [Union(0, typeof(Element))]
    [Union(1, typeof(Relation))]
    public abstract class PlotEntity
    {
        /// <summary>
        /// Properties contained in an entity
        /// </summary>
        [Key(0)]
        public Dictionary<string, object> Params { get; set; } = new Dictionary<string, object>();
        
        /// <summary>
        /// Labels contained in an entity
        /// </summary>
        [Key(1)]

        public HashSet<string> Labels { get; set; } = new HashSet<string>();

        [Key(2)]
        public virtual string Hash { get; protected internal set; } = null!;
    }
}