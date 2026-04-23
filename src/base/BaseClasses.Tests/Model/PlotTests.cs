using System;
using System.Linq;
using BaseClasses.Enum;
using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Services.Binds;
using Xunit;

namespace BaseClasses.Tests.Model
{
    /// <summary>
    /// Тесты для класса Plot.
    /// </summary>
    public class PlotTests
    {
        /// <summary>
        /// Проверяет, что метод Bind использует зарегистрированную стратегию связывания из Binder.
        /// </summary>
        [Fact]
        public void Plot_Bind_UsesBinderMockStrategy()
        {
            var binderMock = new Binder();
            var bindCalls = 0;
            binderMock.Register(
                ElemType.Character,
                ElemType.Character,
                BaseRelationKeys.Relationship,
                (source, target, key, value, plot) =>
                {
                    bindCalls++;
                    plot.Relations.Add(new Relation(source, target, key, value));
                },
                (source, target, key, value, plot) => { });

            var plot = new Plot(binderMock);
            var source = new Element(ElemType.Character, "A");
            var target = new Element(ElemType.Character, "B");
            plot.Add(source);
            plot.Add(target);

            plot.Bind(source, target, BaseRelationKeys.Relationship, 0.8d);

            Assert.Equal(1, bindCalls);
            Assert.Single(plot.Relations);
            Assert.Equal(0.8d, plot.Relations.First().Value);
        }

        /// <summary>
        /// Проверяет, что метод Unbind использует зарегистрированную стратегию отвязывания из Binder
        /// и удаляет соответствующие отношения из истории.
        /// </summary>
        [Fact]
        public void Plot_Unbind_UsesBinderMockStrategy()
        {
            var binderMock = new Binder();
            var unbindCalls = 0;
            binderMock.Register(
                ElemType.Character,
                ElemType.Character,
                BaseRelationKeys.Relationship,
                (source, target, key, value, plot) =>
                {
                    plot.Relations.Add(new Relation(source, target, key, value));
                },
                (source, target, key, value, plot) =>
                {
                    unbindCalls++;
                    plot.Relations.RemoveWhere(r =>
                        r.Source.Equals(source) && r.Target.Equals(target) && r.Param.Equals(key));
                });

            var plot = new Plot(binderMock);
            var source = new Element(ElemType.Character, "A");
            var target = new Element(ElemType.Character, "B");
            plot.Add(source);
            plot.Add(target);
            plot.Bind(source, target, BaseRelationKeys.Relationship, 1.0d);

            plot.Unbind(source, target, BaseRelationKeys.Relationship);

            Assert.Equal(1, unbindCalls);
            Assert.Empty(plot.Relations);
        }

        /// <summary>
        /// Проверяет, что при удалении элемента из истории через метод Remove,
        /// все отношения, связанные с этим элементом, удаляются с помощью стратегии отвязывания из Binder,
        /// и элемент удаляется из списка элементов истории.
        /// </summary>
        [Fact]
        public void Plot_Remove_UnbindsAllRelationsViaBinderMock()
        {
            var binderMock = new Binder();
            var unbindCalls = 0;
            binderMock.Register(
                ElemType.Character,
                ElemType.Character,
                BaseRelationKeys.Relationship,
                (source, target, key, value, plot) =>
                {
                    plot.Relations.Add(new Relation(source, target, key, value));
                },
                (source, target, key, value, plot) =>
                {
                    unbindCalls++;
                    plot.Relations.RemoveWhere(r =>
                        r.Source.Equals(source) && r.Target.Equals(target) && r.Param.Equals(key));
                });

            var plot = new Plot(binderMock);
            var a = new Element(ElemType.Character, "A");
            var b = new Element(ElemType.Character, "B");
            var c = new Element(ElemType.Character, "C");
            plot.Add(a);
            plot.Add(b);
            plot.Add(c);

            plot.Bind(a, b, BaseRelationKeys.Relationship, 0.1d);
            plot.Bind(c, a, BaseRelationKeys.Relationship, 0.2d);

            plot.Remove(a);

            Assert.Equal(2, unbindCalls);
            Assert.DoesNotContain(a, plot.Elements);
            Assert.DoesNotContain(plot.Relations, r => r.Source.Equals(a) || r.Target.Equals(a));
        }

        /// <summary>
        /// Проверяет, что метод Merge использует предоставленную пользовательскую логику объединения (mergerMock),
        /// и что после объединения базовый элемент остается в истории, а целевой элемент удаляется.
        /// </summary>
        [Fact]
        public void Plot_Merge_UsesMergerMock_AndRemovesTarget()
        {
            var mergerCalls = 0;
            var bothElementsAreInPlotOnMergeCall = false;
            Action<IElement, IElement, Plot, bool> mergerMock = (baseElement, targetElement, plot, basePriority) =>
            {
                mergerCalls++;
                bothElementsAreInPlotOnMergeCall =
                    plot.Elements.Contains(baseElement) && plot.Elements.Contains(targetElement) && !basePriority;
            };

            var plot = new Plot(mergeAction: mergerMock);
            var baseElement = new Element(ElemType.Character, "Base");
            var targetElement = new Element(ElemType.Character, "Target");

            plot.Merge(baseElement, targetElement, false);

            Assert.Equal(1, mergerCalls);
            Assert.True(bothElementsAreInPlotOnMergeCall);
            Assert.Contains(baseElement, plot.Elements);
            Assert.DoesNotContain(targetElement, plot.Elements);
        }
    }
}