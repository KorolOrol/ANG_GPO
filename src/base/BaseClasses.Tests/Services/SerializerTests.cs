using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BaseClasses.Enum;
using BaseClasses.Model;
using BaseClasses.Model.Params;
using BaseClasses.Services;
using BaseClasses.Services.Binds;
using Xunit;

namespace BaseClasses.Tests.Services
{
    /// <summary>
    /// Тесты для класса Serializer.
    /// </summary>
    public class SerializerTests
    {
        private static string NewTempFilePath()
        {
            return Path.Combine(Path.GetTempPath(), $"baseclasses_{Guid.NewGuid():N}.json");
        }

        private static readonly ParamKey<string> _TraitsKey = new ParamKey<string>("Traits");
        private static readonly ParamKey<int> _WeightKey = new ParamKey<int>("Weight");

        [Fact]
        public void SerializeToString_Element_ContainsCoreFields()
        {
            var element = new Element(ElemType.Character, "Hero", "Main", time: 5);
            element.Params.Set(_TraitsKey, "Brave");

            var json = Serializer.SerializeToString(element);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.True(root.TryGetProperty("Id", out var idProp));
            Assert.Equal(JsonValueKind.String, idProp.ValueKind);
            Assert.False(string.IsNullOrWhiteSpace(idProp.GetString()));

            Assert.Equal((int)ElemType.Character, root.GetProperty("Type").GetInt32());
            Assert.Equal("Hero", root.GetProperty("Name").GetString());
            Assert.Equal("Main", root.GetProperty("Description").GetString());
            Assert.Equal(5, root.GetProperty("Time").GetInt32());

            var paramsProp = root.GetProperty("Params");
            Assert.Equal(JsonValueKind.Array, paramsProp.ValueKind);
            Assert.Single(paramsProp.EnumerateArray());
            var item = paramsProp.EnumerateArray().First();
            Assert.Equal("Traits", item.GetProperty("Name").GetString());
            Assert.Equal("Brave", item.GetProperty("Value").GetString());
        }

        [Fact]
        public void SerializeToString_Plot_ContainsElementsAndRelationsDtoShape()
        {
            var plot = new Plot();
            var character = new Element(ElemType.Character, "Hero", "Main");
            var item = new Element(ElemType.Item, "Sword", "Steel");
            plot.Add(character);
            plot.Add(item);
            plot.Bind(character, item, BaseRelationKeys.Owns, true);
            plot.Time = 99;

            var json = Serializer.SerializeToString(plot);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            Assert.Equal(99, root.GetProperty("Time").GetInt32());

            var elements = root.GetProperty("Elements").EnumerateArray().ToList();
            Assert.Equal(2, elements.Count);
            Assert.All(elements, e =>
            {
                Assert.True(e.TryGetProperty("Id", out var id));
                Assert.Equal(JsonValueKind.String, id.ValueKind);
                Assert.True(e.TryGetProperty("Type", out _));
                Assert.True(e.TryGetProperty("Name", out _));
                Assert.True(e.TryGetProperty("Description", out _));
                Assert.True(e.TryGetProperty("Params", out _));
                Assert.True(e.TryGetProperty("Time", out _));
            });

            var relations = root.GetProperty("Relations").EnumerateArray().ToList();
            Assert.Equal(2, relations.Count);
            Assert.All(relations, r =>
            {
                Assert.Equal(JsonValueKind.String, r.GetProperty("Source").ValueKind);
                Assert.Equal(JsonValueKind.String, r.GetProperty("Target").ValueKind);
                Assert.Equal("Base", r.GetProperty("Namespace").GetString());
                Assert.Equal(JsonValueKind.String, r.GetProperty("Name").ValueKind);
                Assert.Equal(JsonValueKind.String, r.GetProperty("Type").ValueKind);
                Assert.Equal(JsonValueKind.True, r.GetProperty("Value").ValueKind);
            });
            Assert.Contains(relations, r => r.GetProperty("Name").GetString() == "Owns");
            Assert.Contains(relations, r => r.GetProperty("Name").GetString() == "Owned");
        }

        [Fact]
        public void SerializeAndDeserialize_Element_RoundTripPreservesData()
        {
            var path = NewTempFilePath();
            try
            {
                var source = new Element(ElemType.Item, "Sword", "Steel", time: 11);
                source.Params.Set(_WeightKey, 3);

                Serializer.Serialize(source, path);
                var actual = Serializer.Deserialize<Element>(path);

                Assert.Equal(source.Type, actual.Type);
                Assert.Equal(source.Name, actual.Name);
                Assert.Equal(source.Description, actual.Description);
                Assert.Equal(source.Time, actual.Time);
                Assert.True(actual.Params.TryGet(_WeightKey, out var weight));
                Assert.Equal(3, weight);
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void SerializeAndDeserialize_Plot_RoundTripPreservesElementsRelationsAndTime()
        {
            var path = NewTempFilePath();
            try
            {
                var plot = new Plot();
                var character = new Element(ElemType.Character, "Hero", "Main");
                var item = new Element(ElemType.Item, "Sword", "Steel");
                var location = new Element(ElemType.Location, "Castle", "Stone");

                plot.Add(character);
                plot.Add(item);
                plot.Add(location);

                plot.Bind(character, item, BaseRelationKeys.Owns, true);
                plot.Bind(item, location, BaseRelationKeys.Located, true);
                plot.Time = 42;

                Serializer.Serialize(plot, path);
                var actual = Serializer.Deserialize<Plot>(path);

                Assert.Equal(3, actual.Elements.Count);
                Assert.Equal(42, actual.Time);
                Assert.Equal(4, actual.Relations.Count);

                Assert.Contains(actual.Relations, r =>
                    r.Source.Type == ElemType.Character &&
                    r.Target.Type == ElemType.Item &&
                    r.Param.Equals(BaseRelationKeys.Owns) &&
                    r.Value is true);

                Assert.Contains(actual.Relations, r =>
                    r.Source.Type == ElemType.Item &&
                    r.Target.Type == ElemType.Character &&
                    r.Param.Equals(BaseRelationKeys.Owned) &&
                    r.Value is true);

                Assert.Contains(actual.Relations, r =>
                    r.Source.Type == ElemType.Item &&
                    r.Target.Type == ElemType.Location &&
                    r.Param.Equals(BaseRelationKeys.Located));

                Assert.Contains(actual.Relations, r =>
                    r.Source.Type == ElemType.Location &&
                    r.Target.Type == ElemType.Item &&
                    r.Param.Equals(BaseRelationKeys.Locates));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void Deserialize_Plot_WithMissingRelationSource_ThrowsJsonException()
        {
            var path = NewTempFilePath();
            try
            {
                // Source/Target не существуют в коллекции Elements.
                var json = @"{
  ""Time"": 0,
  ""Elements"": [],
  ""Relations"": [
    {
      ""Source"": ""11111111-1111-1111-1111-111111111111"",
      ""Target"": ""22222222-2222-2222-2222-222222222222"",
      ""Namespace"": ""Base"",
      ""Name"": ""Owns"",
      ""Type"": ""System.Boolean"",
      ""Value"": true
    }
  ]
}";
                File.WriteAllText(path, json);

                Assert.Throws<JsonException>(() => Serializer.Deserialize<Plot>(path));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void Deserialize_UnsupportedType_ThrowsArgumentException()
        {
            var path = NewTempFilePath();
            try
            {
                File.WriteAllText(path, "{}");

                Assert.Throws<ArgumentException>(() => Serializer.Deserialize<Dictionary<string, string>>(path));
            }
            finally
            {
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Fact]
        public void PrintToString_Element_ReturnsReadableText()
        {
            var element = new Element(ElemType.Event, "Battle", "Finale");

            var actual = Serializer.PrintToString(element);

            Assert.Contains("Event: Battle", actual);
            Assert.Contains("Finale", actual);
        }

        [Fact]
        public void PrintToString_Plot_ContainsTimeElementsAndRelationsSections()
        {
            var plot = new Plot();
            var character = new Element(ElemType.Character, "Hero", "Main");
            var item = new Element(ElemType.Item, "Sword", "Steel");
            plot.Add(character);
            plot.Add(item);
            plot.Bind(character, item, BaseRelationKeys.Owns, true);

            var actual = Serializer.PrintToString(plot);

            Assert.Contains("Time:", actual);
            Assert.Contains("Elements:", actual);
            Assert.Contains("Relations:", actual);
            Assert.Contains("Character: Hero", actual);
            Assert.Contains("Item: Sword", actual);
        }
    }
}
