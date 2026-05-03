using System;
using BaseClasses.Enum;
using BaseClasses.Model;
using BaseClasses.Model.Params;
using BaseClasses.Services.Binds;
using Xunit;

namespace BaseClasses.Tests.Services
{
    /// <summary>
    /// Тесты для класса Binder.
    /// </summary>
    public class BinderTests
    {
        /// <summary>
        /// Проверяет, что регистрация стратегии связывания с null-значением выбрасывает ArgumentNullException.
        /// </summary>
        [Fact]
        public void Register_NullBindStrategy_Throws()
        {
            var binder = new Binder();
            var route = new RelationRoute(ElemType.Character, ElemType.Item, new ParamKey<bool>("Custom"));

            Assert.Throws<ArgumentNullException>(() =>
                binder.Register(route, null!, 
                    (s, t, p, v, pl) => { }));
        }

        /// <summary>
        /// Проверяет, что регистрация стратегии отвязывания с null-значением выбрасывает ArgumentNullException.
        /// </summary>
        [Fact]
        public void Register_NullUnbindStrategy_Throws()
        {
            var binder = new Binder();
            var route = new RelationRoute(ElemType.Character, ElemType.Item, new ParamKey<bool>("Custom"));

            Assert.Throws<ArgumentNullException>(() =>
                binder.Register(route, (s, t, p, v, pl) => { }, 
                    null!));
        }

        /// <summary>
        /// Проверяет, что попытка связать элементы без зарегистрированной стратегии выбрасывает ArgumentException.
        /// </summary>
        [Fact]
        public void Bind_NoStrategy_Throws()
        {
            var binder = new Binder();
            var plot = new Plot(binder);
            var source = new Element(ElemType.Character, "S");
            var target = new Element(ElemType.Item, "T");
            var key = new ParamKey<bool>("Unregistered");

            Assert.Throws<ArgumentException>(() => binder.Bind(source, target, key, true, plot));
        }

        /// <summary>
        /// Проверяет, что попытка связать элементы с null-источником выбрасывает ArgumentNullException.
        /// </summary>
        [Fact]
        public void Bind_NullSource_Throws()
        {
            var binder = new Binder();
            var plot = new Plot(binder);
            var target = new Element(ElemType.Item, "T");

            Assert.Throws<ArgumentNullException>(() =>
                binder.Bind(null!, target, BaseRelationKeys.Owns, true, plot));
        }

        /// <summary>
        /// Проверяет, что попытка связать элементы с null-целью выбрасывает ArgumentNullException.
        /// </summary>
        [Fact]
        public void Bind_NullTarget_Throws()
        {
            var binder = new Binder();
            var plot = new Plot(binder);
            var source = new Element(ElemType.Character, "S");

            Assert.Throws<ArgumentNullException>(() =>
                binder.Bind(source, null!, BaseRelationKeys.Owns, true, plot));
        }

        /// <summary>
        /// Проверяет, что попытка связать элементы с несовместимым типом значения выбрасывает ArgumentException.
        /// </summary>
        [Fact]
        public void Bind_InvalidValueType_Throws()
        {
            var binder = new Binder();
            var plot = new Plot(binder);
            var source = new Element(ElemType.Character, "S");
            var target = new Element(ElemType.Character, "T");

            Assert.Throws<ArgumentException>(() =>
                binder.Bind(source, target, BaseRelationKeys.Relationship, "bad", plot));
        }

        /// <summary>
        /// Проверяет, что метод Bind вызывает зарегистрированную стратегию связывания и добавляет отношение в историю.
        /// </summary>
        [Fact]
        public void Bind_RegisteredStrategy_InvokesStrategy()
        {
            var binder = new Binder();
            var plot = new Plot(binder);
            var source = new Element(ElemType.Character, "S");
            var target = new Element(ElemType.Item, "T");
            var key = new ParamKey<bool>("Custom");
            var bindCalls = 0;

            binder.Register(
                ElemType.Character,
                ElemType.Item,
                key,
                (s, t, k, v, p) =>
                {
                    bindCalls++;
                    p.Relations.Add(new Relation(s, t, k, v));
                },
                (s, t, p, v, pl) => { });

            binder.Bind(source, target, key, true, plot);

            Assert.Equal(1, bindCalls);
            Assert.Single(plot.Relations);
        }

        /// <summary>
        /// Проверяет, что метод Unbind вызывает зарегистрированную стратегию отвязывания
        /// и удаляет соответствующее отношение из истории.
        /// </summary>
        [Fact]
        public void Unbind_RegisteredStrategy_InvokesStrategy()
        {
            var binder = new Binder();
            var plot = new Plot(binder);
            var source = new Element(ElemType.Character, "S");
            var target = new Element(ElemType.Item, "T");
            var key = new ParamKey<bool>("Custom");
            var unbindCalls = 0;

            binder.Register(
                ElemType.Character,
                ElemType.Item,
                key,
                (s, t, k, v, p) => 
                    p.Relations.Add(new Relation(s, t, k, v)),
                (s, t, k, _, p) =>
                {
                    unbindCalls++;
                    p.Relations.RemoveWhere(r =>
                        r.Source.Equals(s) && r.Target.Equals(t) && r.Param.Equals(k));
                });

            binder.Bind(source, target, key, true, plot);
            binder.Unbind(source, target, key, plot);

            Assert.Equal(1, unbindCalls);
            Assert.Empty(plot.Relations);
        }

        /// <summary>
        /// Проверяет, что попытка отвязать элементы без зарегистрированной стратегии не выбрасывает исключений
        /// и не изменяет историю.
        /// </summary>
        [Fact]
        public void Unbind_NoStrategy_DoesNothing()
        {
            var binder = new Binder();
            var plot = new Plot(binder);
            var source = new Element(ElemType.Character, "S");
            var target = new Element(ElemType.Item, "T");
            var key = new ParamKey<bool>("Custom");

            var ex = Record.Exception(() => binder.Unbind(source, target, key, plot));

            Assert.Null(ex);
        }

        /// <summary>
        /// Проверяет, что метод Unregister удаляет зарегистрированную стратегию связывания.
        /// </summary>
        [Fact]
        public void Unregister_RemovesRoute()
        {
            var binder = new Binder();
            var plot = new Plot(binder);
            var source = new Element(ElemType.Character, "S");
            var target = new Element(ElemType.Item, "T");
            var key = new ParamKey<bool>("Custom");

            binder.Register(ElemType.Character, ElemType.Item, key, 
                (s, t, p, v, pl) => { }, 
                (s, t, p, v, pl) => { });
            Assert.True(binder.Unregister(ElemType.Character, ElemType.Item, key));

            Assert.Throws<ArgumentException>(() => binder.Bind(source, target, key, true, plot));
        }
    }
}
