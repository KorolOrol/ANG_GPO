using MessagePack;

namespace BaseClasses_V2.Model
{
    /// <summary>
    /// Отношение персонажа с другим персонажем
    /// </summary>
    [MessagePackObject(keyAsPropertyName: true)]
    public class Relation : PlotEntity, IComparable<Relation>, IEquatable<Relation>
    {
        public virtual string RelationType { get; set; }
        
        public Element Source { get; set; }
        
        public Element Target { get; set; }
        
        public Relation()
        {
            Hash = Guid.NewGuid().ToString();
        }

        public Relation(string relationName, Dictionary<string, object> @params, HashSet<string> labels, Element sourceHash, Element targetHash)
        {
            Hash = Guid.NewGuid().ToString();
            RelationType = relationName;
            Params = @params;
            Labels = labels;
            Source = sourceHash;
            Target = targetHash;
        }
        
        public int CompareTo(Relation other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var relationNameComparison = string.Compare(RelationType, other.RelationType, StringComparison.Ordinal);
            if (relationNameComparison != 0) return relationNameComparison;
            var sourceHashComparison = string.Compare(Source.Hash, other.Source.Hash, StringComparison.Ordinal);
            if (sourceHashComparison != 0) return sourceHashComparison;
            return string.Compare(Target.Hash, other.Target.Hash, StringComparison.Ordinal);
        }

        public bool Equals(Relation other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return RelationType == other.RelationType && Equals(Params, other.Params) && Equals(Labels, other.Labels) && Source.Hash == other.Source.Hash && Target.Hash == other.Target.Hash;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Relation) obj);
        }
    }
}
