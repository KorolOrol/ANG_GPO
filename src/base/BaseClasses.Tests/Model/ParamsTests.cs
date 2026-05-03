using System;
using System.Collections.Generic;
using BaseClasses.Interface;
using BaseClasses.Model.Params;
using Xunit;

namespace BaseClasses.Tests.Model
{
    /// <summary>
    /// Тесты для классов, связанных с параметрами элементов истории, таких как ParamKey и ParamBag.
    /// </summary>
    public class ParamsTests
    {
        /// <summary>
        /// Тесты для класса ParamKey.
        /// </summary>
        public class ParamKeyTests
        {
            public static IEnumerable<object[]> IsCollectionCases()
            {
                yield return new object[] { new ParamKey<List<int>>("Ints"), true };
                yield return new object[] { new ParamKey<List<string>>("Strings"), true };
                yield return new object[] { new ParamKey<Dictionary<string, int>>("DictInt"), true };
                yield return new object[] { new ParamKey<Dictionary<string, string>>("DictString"), true };
                yield return new object[] { new ParamKey<int>("Int"), false };
                yield return new object[] { new ParamKey<string>("String"), false };
            }

            public static IEnumerable<object?[]> IsTypeMatchPositiveCases()
            {
                yield return new object?[] { new ParamKey<int>("Int"), 1 };
                yield return new object?[] { new ParamKey<double>("Double"), 1 };
                yield return new object?[] { new ParamKey<int>("Int"), 1.0d };
                yield return new object?[] { new ParamKey<List<int>>("Ints"), new[] { 1, 2, 3 } };
                yield return new object?[] { new ParamKey<string>("String"), null };
            }

            public static IEnumerable<object[]> IsTypeMatchNegativeCases()
            {
                yield return new object[] { new ParamKey<int>("Int"), "abc" };
                yield return new object[] { new ParamKey<List<int>>("Ints"), new[] { 1.5d } };
                yield return new object[] { new ParamKey<List<int>>("Ints"), "abc" };
            }

            public static IEnumerable<object[]> SimpleTypeSuccessfulCases()
            {
                yield return new object[] { new ParamKey<int>("Int"), 1 };
                yield return new object[] { new ParamKey<string>("String"), "string" };
                yield return new object[] { new ParamKey<bool>("Bool"), true };
            }

            [Theory]
            [MemberData(nameof(IsCollectionCases))]
            public void ParamKey_IsCollection_Correct(IParamKey key, bool expected)
            {
                Assert.Equal(expected, key.IsCollection);
            }

            [Theory]
            [MemberData(nameof(IsTypeMatchPositiveCases))]
            public void ParamKey_IsTypeMatch_ReturnsTrue_ForValidAndConvertibleValues(IParamKey key, object? value)
            {
                Assert.True(key.IsTypeMatch(value));
            }

            [Theory]
            [MemberData(nameof(IsTypeMatchNegativeCases))]
            public void ParamKey_IsTypeMatch_ReturnsFalse_ForInvalidValues(IParamKey key, object value)
            {
                Assert.False(key.IsTypeMatch(value));
            }

            [Theory]
            [MemberData(nameof(SimpleTypeSuccessfulCases))]
            public void ParamKey_TryConvertValue_SimpleTypes_Correct(IParamKey key, object value)
            {
                Assert.True(key.TryConvertValue(value, out var result));
                Assert.Equal(value, result);
            }

            [Fact]
            public void ParamKey_TryConvertValue_PrimitiveTypes_Correct()
            {
                var intPk = new ParamKey<int>("Int");
                var doublePk = new ParamKey<double>("Double");

                Assert.True(intPk.TryConvertValue(1, out var intResult));
                Assert.Equal(1, intResult);

                Assert.True(intPk.TryConvertValue(1.0d, out intResult));
                Assert.Equal(1, intResult);

                Assert.False(intPk.TryConvertValue(1.5d, out _));

                Assert.True(intPk.TryConvertValue(1.0d + 5e-10, out intResult));
                Assert.Equal(1, intResult);
                
                Assert.True(intPk.TryConvertValue(1.0d - 5e-10, out intResult));
                Assert.Equal(1, intResult);

                Assert.True(doublePk.TryConvertValue(1, out var doubleResult));
                Assert.Equal(1.0d, doubleResult);

                Assert.True(doublePk.TryConvertValue(1.5d, out doubleResult));
                Assert.Equal(1.5d, doubleResult);
            }

            [Fact]
            public void ParamKey_TryConvertValue_CollectionTypes_Correct()
            {
                var listIntPk = new ParamKey<List<int>>("Ints");

                Assert.True(listIntPk.TryConvertValue(new[] { 1, 2, 3 }, out var intArrayResult));
                Assert.Equal(new List<int> { 1, 2, 3 }, Assert.IsType<List<int>>(intArrayResult));

                Assert.True(listIntPk.TryConvertValue(new[] { 1.0d, 2.0d }, out var doubleArrayResult));
                Assert.Equal(new List<int> { 1, 2 }, Assert.IsType<List<int>>(doubleArrayResult));

                Assert.False(listIntPk.TryConvertValue(new[] { 1.5d }, out _));
                Assert.False(listIntPk.TryConvertValue("abc", out _));
                Assert.False(listIntPk.TryConvertValue(10, out _));
            }
        }

        /// <summary>
        /// Тесты для класса ParamBag.
        /// </summary>
        public class ParamBagTests
        {
            [Fact]
            public void ParamBag_Indexer_SetAndGet_Correct()
            {
                var bag = new ParamBag();
                var intKey = new ParamKey<int>("Int");

                bag[intKey] = 42;

                Assert.True(bag.ContainsKey(intKey));
                Assert.Equal(42, bag[intKey]);
            }

            [Fact]
            public void ParamBag_Indexer_GetMissingKey_Throws()
            {
                var bag = new ParamBag();
                var intKey = new ParamKey<int>("Int");

                Assert.Throws<KeyNotFoundException>(() => _ = bag[intKey]);
            }

            [Fact]
            public void ParamBag_Indexer_SetWrongType_Throws()
            {
                var bag = new ParamBag();
                var intKey = new ParamKey<int>("Int");

                Assert.Throws<InvalidOperationException>(() => bag[intKey] = "abc");
            }

            [Fact]
            public void ParamBag_Set_StoresAndOverwritesTypedValue()
            {
                var bag = new ParamBag();
                var intKey = new ParamKey<int>("Int");

                bag.Set(intKey, 1);
                bag.Set(intKey, 2);

                Assert.True(bag.TryGet(intKey, out var value));
                Assert.Equal(2, value);
            }

            [Fact]
            public void ParamBag_TryGet_WorksWithEquivalentKeyInstance()
            {
                var bag = new ParamBag();
                var key1 = new ParamKey<int>("Int");
                var key2 = new ParamKey<int>("Int");

                bag.Set(key1, 7);

                Assert.True(bag.TryGet(key2, out var value));
                Assert.Equal(7, value);
            }

            [Fact]
            public void ParamBag_TryGet_ReturnsFalse_WhenMissing()
            {
                var bag = new ParamBag();
                var intKey = new ParamKey<int>("Int");

                Assert.False(bag.TryGet(intKey, out _));
            }

            [Fact]
            public void ParamBag_TryGet_HandlesNull_ForReferenceAndValueTypes()
            {
                var bag = new ParamBag();
                var stringKey = new ParamKey<string>("String");
                var intKey = new ParamKey<int>("Int");

                bag[stringKey] = null;
                bag[intKey] = null;

                Assert.True(bag.TryGet(stringKey, out var stringValue));
                Assert.Null(stringValue);

                Assert.False(bag.TryGet(intKey, out _));
            }

            [Fact]
            public void ParamBag_TryGet_ConvertsPrimitive_WhenPossible()
            {
                var bag = new ParamBag();
                var doubleKey = new ParamKey<double>("Double");

                // Add хранит сырое значение, поэтому проверяем конвертацию в TryGet.
                bag.Add(doubleKey, 1);

                Assert.True(bag.TryGet(doubleKey, out var value));
                Assert.Equal(1.0d, value);
            }
        }
    }
}