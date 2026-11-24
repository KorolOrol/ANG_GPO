using System.Collections.Generic;
using BaseClasses.Enum;
using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Services.Binds;
using Xunit;

namespace BaseClasses.Tests.Services
{
    public class StandardBindingStrategiesTests
    {
        public static IEnumerable<object[]> RouteCases()
        {
            yield return new object[] { ElemType.Character, ElemType.Character, BaseRelationKeys.Relationship, BaseRelationKeys.Relationship };
            yield return new object[] { ElemType.Character, ElemType.Item, BaseRelationKeys.Owns, BaseRelationKeys.Owned };
            yield return new object[] { ElemType.Character, ElemType.Location, BaseRelationKeys.Located, BaseRelationKeys.Locates };
            yield return new object[] { ElemType.Character, ElemType.Event, BaseRelationKeys.Participates, BaseRelationKeys.Involves };
            yield return new object[] { ElemType.Item, ElemType.Character, BaseRelationKeys.Owned, BaseRelationKeys.Owns };
            yield return new object[] { ElemType.Item, ElemType.Location, BaseRelationKeys.Located, BaseRelationKeys.Locates };
            yield return new object[] { ElemType.Item, ElemType.Event, BaseRelationKeys.Used, BaseRelationKeys.Uses };
            yield return new object[] { ElemType.Location, ElemType.Character, BaseRelationKeys.Locates, BaseRelationKeys.Located };
            yield return new object[] { ElemType.Location, ElemType.Item, BaseRelationKeys.Locates, BaseRelationKeys.Located };
            yield return new object[] { ElemType.Location, ElemType.Event, BaseRelationKeys.Locates, BaseRelationKeys.Located };
            yield return new object[] { ElemType.Event, ElemType.Character, BaseRelationKeys.Involves, BaseRelationKeys.Participates };
            yield return new object[] { ElemType.Event, ElemType.Item, BaseRelationKeys.Uses, BaseRelationKeys.Used };
            yield return new object[] { ElemType.Event, ElemType.Location, BaseRelationKeys.Located, BaseRelationKeys.Locates };
        }

        [Theory]
        [MemberData(nameof(RouteCases))]
        public void Bind_AddsForwardAndReverseRelations_ForAllStandardRoutes(
            ElemType sourceType,
            ElemType targetType,
            IParamKey forwardKey,
            IParamKey reverseKey)
        {
            var plot = new Plot();
            var source = NewElement(sourceType, "S");
            var target = NewElement(targetType, "T");
            plot.Add(source);
            plot.Add(target);

            plot.Bind(source, target, forwardKey, ValueFor(forwardKey));

            Assert.True(HasRelation(plot, source, target, forwardKey));
            Assert.True(HasRelation(plot, target, source, reverseKey));
        }

        [Theory]
        [MemberData(nameof(RouteCases))]
        public void Unbind_RemovesForwardAndReverseRelations_ForAllStandardRoutes(
            ElemType sourceType,
            ElemType targetType,
            IParamKey forwardKey,
            IParamKey reverseKey)
        {
            var plot = new Plot();
            var source = NewElement(sourceType, "S");
            var target = NewElement(targetType, "T");
            plot.Add(source);
            plot.Add(target);
            plot.Bind(source, target, forwardKey, ValueFor(forwardKey));

            plot.Unbind(source, target, forwardKey);

            Assert.False(HasRelation(plot, source, target, forwardKey));
            Assert.False(HasRelation(plot, target, source, reverseKey));
        }

        [Fact]
        public void BindCharItem_Rebind_RemovesOldOwner()
        {
            var plot = new Plot();
            var oldOwner = new Element(ElemType.Character, "Old");
            var newOwner = new Element(ElemType.Character, "New");
            var item = new Element(ElemType.Item, "Sword");
            plot.Add(oldOwner);
            plot.Add(newOwner);
            plot.Add(item);

            plot.Bind(oldOwner, item, BaseRelationKeys.Owns, true);
            plot.Bind(newOwner, item, BaseRelationKeys.Owns, true);

            Assert.False(HasRelation(plot, oldOwner, item, BaseRelationKeys.Owns));
            Assert.False(HasRelation(plot, item, oldOwner, BaseRelationKeys.Owned));
            Assert.True(HasRelation(plot, newOwner, item, BaseRelationKeys.Owns));
            Assert.True(HasRelation(plot, item, newOwner, BaseRelationKeys.Owned));
        }

        [Fact]
        public void BindItemLoc_Rebind_RemovesOldLocation()
        {
            var plot = new Plot();
            var item = new Element(ElemType.Item, "Ring");
            var oldLocation = new Element(ElemType.Location, "Cave");
            var newLocation = new Element(ElemType.Location, "Castle");
            plot.Add(item);
            plot.Add(oldLocation);
            plot.Add(newLocation);

            plot.Bind(item, oldLocation, BaseRelationKeys.Located, true);
            plot.Bind(item, newLocation, BaseRelationKeys.Located, true);

            Assert.False(HasRelation(plot, item, oldLocation, BaseRelationKeys.Located));
            Assert.False(HasRelation(plot, oldLocation, item, BaseRelationKeys.Locates));
            Assert.True(HasRelation(plot, item, newLocation, BaseRelationKeys.Located));
            Assert.True(HasRelation(plot, newLocation, item, BaseRelationKeys.Locates));
        }

        private static Element NewElement(ElemType type, string name)
        {
            return new Element(type, name);
        }

        private static object ValueFor(IParamKey key)
        {
            return key.ValueType == typeof(double) ? (object)0.5d : true;
        }

        private static bool HasRelation(Plot plot, IElement source, IElement target, IParamKey key)
        {
            return plot.Relations.Contains(new Relation(source, target, key, null));
        }
    }
}