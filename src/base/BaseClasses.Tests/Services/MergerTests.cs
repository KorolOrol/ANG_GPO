using System;
using System.Linq;
using BaseClasses.Enum;
using BaseClasses.Model;
using BaseClasses.Model.Params;
using BaseClasses.Services;
using Xunit;

namespace BaseClasses.Tests.Services
{
    /// <summary>
    /// Тесты для класса Merger с mock/stub Plot (кастомный Binder-спай).
    /// </summary>
    public class MergerTests
    {
        private static readonly ParamKey<bool> _LinkKey = new ParamKey<bool>("Link");

        private static Plot CreatePlot() => new Plot();

        [Fact]
        public void Merge_NullBase_ThrowsArgumentNullException()
        {
            var plot = CreatePlot();
            var merged = new Element(ElemType.Character);

            Assert.Throws<ArgumentNullException>(() => Merger.Merge(null!, merged, plot));
        }

        [Fact]
        public void Merge_NullMerged_ThrowsArgumentNullException()
        {
            var plot = CreatePlot();
            var baseElement = new Element(ElemType.Character);

            Assert.Throws<ArgumentNullException>(() => Merger.Merge(baseElement, null!, plot));
        }

        [Fact]
        public void Merge_DifferentTypes_ThrowsArgumentException()
        {
            var plot = CreatePlot();
            var character = new Element(ElemType.Character);
            var item = new Element(ElemType.Item);

            Assert.Throws<ArgumentException>(() => Merger.Merge(character, item, plot));
        }

        [Fact]
        public void Merge_WithBasePriority_KeepsBaseTextFields_AndSetsMaxTime()
        {
            var plot = CreatePlot();
            var baseElement = new Element(ElemType.Character, "BaseName", "BaseDescription", time: 2);
            var mergedElement = new Element(ElemType.Character, "MergedName", "MergedDescription", time: 7);

            Merger.Merge(baseElement, mergedElement, plot, basePriority: true);

            Assert.Equal("BaseName", baseElement.Name);
            Assert.Equal("BaseDescription", baseElement.Description);
            Assert.Equal(7, baseElement.Time);
        }

        [Fact]
        public void Merge_WithoutBasePriority_OverridesTextFields()
        {
            var plot = CreatePlot();
            var baseElement = new Element(ElemType.Character, "BaseName", "BaseDescription", time: 3);
            var mergedElement = new Element(ElemType.Character, "MergedName", "MergedDescription", time: 1);

            Merger.Merge(baseElement, mergedElement, plot, basePriority: false);

            Assert.Equal("MergedName", baseElement.Name);
            Assert.Equal("MergedDescription", baseElement.Description);
            Assert.Equal(3, baseElement.Time);
        }

        [Fact]
        public void Merge_TransfersIncomingAndOutgoingRelations_FromMergedToBase()
        {
            var plot = CreatePlot();
            var baseElement = new Element(ElemType.Character, "Base");
            var mergedElement = new Element(ElemType.Character, "Merged");
            var incomingOther = new Element(ElemType.Character, "Incoming");
            var outgoingOther = new Element(ElemType.Character, "Outgoing");

            plot.Add(baseElement);
            plot.Add(mergedElement);
            plot.Add(incomingOther);
            plot.Add(outgoingOther);

            plot.Relations.Add(new Relation(mergedElement, outgoingOther, _LinkKey, true));
            plot.Relations.Add(new Relation(incomingOther, mergedElement, _LinkKey, true));

            Merger.Merge(baseElement, mergedElement, plot);

            Assert.DoesNotContain(plot.Relations, r =>
                r.Source.Equals(mergedElement) || r.Target.Equals(mergedElement));
            Assert.Contains(plot.Relations, r =>
                r.Source.Equals(baseElement) && r.Target.Equals(outgoingOther) && r.Param.Equals(_LinkKey));
            Assert.Contains(plot.Relations, r =>
                r.Source.Equals(incomingOther) && r.Target.Equals(baseElement) && r.Param.Equals(_LinkKey));
        }

        [Fact]
        public void Merge_WithBasePriority_SkipsConflictingRelationForBase()
        {
            var plot = CreatePlot();
            var baseElement = new Element(ElemType.Character, "Base");
            var mergedElement = new Element(ElemType.Character, "Merged");
            var other = new Element(ElemType.Character, "Other");

            plot.Add(baseElement);
            plot.Add(mergedElement);
            plot.Add(other);

            plot.Relations.Add(new Relation(baseElement, other, _LinkKey, false));
            plot.Relations.Add(new Relation(mergedElement, other, _LinkKey, true));

            Merger.Merge(baseElement, mergedElement, plot, basePriority: true);

            var relation = Assert.Single(plot.Relations, r =>
                r.Source.Equals(baseElement) && r.Target.Equals(other) && r.Param.Equals(_LinkKey));
            Assert.Equal(false, relation.Value);
            Assert.DoesNotContain(plot.Relations, r =>
                r.Source.Equals(mergedElement) || r.Target.Equals(mergedElement));
        }

        [Fact]
        public void Merge_WithoutBasePriority_RebindsConflictingRelation()
        {
            var plot = CreatePlot();
            var baseElement = new Element(ElemType.Character, "Base");
            var mergedElement = new Element(ElemType.Character, "Merged");
            var other = new Element(ElemType.Character, "Other");

            plot.Add(baseElement);
            plot.Add(mergedElement);
            plot.Add(other);

            plot.Relations.Add(new Relation(baseElement, other, _LinkKey, false));
            plot.Relations.Add(new Relation(mergedElement, other, _LinkKey, true));

            Merger.Merge(baseElement, mergedElement, plot, basePriority: false);

            var relation = Assert.Single(plot.Relations, r =>
                r.Source.Equals(baseElement) && r.Target.Equals(other) && r.Param.Equals(_LinkKey));
            Assert.Equal(true, relation.Value);
        }
    }
}
