using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Enum;
using BaseClasses.Services;

namespace BaseClasses.Tests.Services
{
    /// <summary>
    /// Тесты для класса Serializer
    /// </summary>
    public class SerializerTests
    {
        /// <summary>
        /// Тесты для класса Serializer. Сохранение и загрузка элементов истории без связей.
        /// </summary>
        public class SerializerSingleElementsTests
        {
            /// <summary>
            /// Сохранение персонажа без связей с другими элементами.
            /// Ожидание: файл с верными данными персонажа.
            /// </summary>
            [Fact]
            public void Serialize_CharacterWithoutBonds_CorrectFile()
            {
                // Arrange
                var character = new Element(ElemType.Character, "Name", "Description");
                character.Params.Add("Traits", new List<string> { "Trait1", "Trait2" });
                // Act
                Serializer.Serialize(character, "CharacterWithoutBonds.txt");
                // Assert
                var actual = File.ReadAllText("CharacterWithoutBonds.txt");
                var expected = @"{
  ""$id"": ""1"",
  ""$type"": ""Element"",
  ""Type"": 0,
  ""Name"": ""Name"",
  ""Description"": ""Description"",
  ""Params"": {
    ""$id"": ""2"",
    ""Traits"": {
      ""$id"": ""3"",
      ""$values"": [
        ""Trait1"",
        ""Trait2""
      ]
    }
  },
  ""Time"": -1
}";
                Assert.Equal(expected, actual);
                File.Delete("CharacterWithoutBonds.txt");
            }

            /// <summary>
            /// Десериализация персонажа без связей с другими элементами.
            /// Ожидание: объект персонажа с верными данными.
            /// </summary>
            [Fact]
            public void Deserialize_CharacterWithoutBonds_CorrectObject()
            {
                // Arrange
                Serialize_CharacterWithoutBonds_CorrectFile();
                var character = new Element(ElemType.Character, "Name", "Description");
                character.Params.Add("Traits", new List<string> { "Trait1", "Trait2" });
                Serializer.Serialize(character, "CharacterWithoutBonds.txt");
                // Act
                var actual = Serializer.Deserialize<Element>("CharacterWithoutBonds.txt");
                // Assert
                Assert.Equal(character.Type, actual.Type);
                Assert.Equal(character.Name, actual.Name);
                Assert.Equal(character.Description, actual.Description);
                Assert.Equal(character.Params, actual.Params);
                Assert.Equal(character.Time, actual.Time);
                File.Delete("CharacterWithoutBonds.txt");
            }

            /// <summary>
            /// Сохранение предмета без связей с другими элементами.
            /// Ожидание: файл с верными данными предмета.
            /// </summary>
            [Fact]
            public void Serialize_ItemWithoutBonds_CorrectFile()
            {
                // Arrange
                var item = new Element(ElemType.Item, "Name", "Description");
                item.Params.Add("Weight", 10);
                // Act
                Serializer.Serialize(item, "ItemWithoutBonds.txt");
                // Assert
                var actual = File.ReadAllText("ItemWithoutBonds.txt");
                var expected = @"{
  ""$id"": ""1"",
  ""$type"": ""Element"",
  ""Type"": 1,
  ""Name"": ""Name"",
  ""Description"": ""Description"",
  ""Params"": {
    ""$id"": ""2"",
    ""Weight"": 10
  },
  ""Time"": -1
}";
                Assert.Equal(expected, actual);
                File.Delete("ItemWithoutBonds.txt");
            }

            /// <summary>
            /// Десериализация предмета без связей с другими элементами.
            /// Ожидание: объект предмета с верными данными.
            /// </summary>
            [Fact]
            public void Deserialize_ItemWithoutBonds_CorrectObject()
            {
                // Arrange
                Serialize_ItemWithoutBonds_CorrectFile();
                var item = new Element(ElemType.Item, "Name", "Description");
                item.Params.Add("Weight", 10.0);
                Serializer.Serialize(item, "ItemWithoutBonds.txt");
                // Act
                var actual = Serializer.Deserialize<Element>("ItemWithoutBonds.txt");
                // Assert
                Assert.Equal(item.Type, actual.Type);
                Assert.Equal(item.Name, actual.Name);
                Assert.Equal(item.Description, actual.Description);
                Assert.Equal(item.Params, actual.Params);
                Assert.Equal(item.Time, actual.Time);
                File.Delete("ItemWithoutBonds.txt");
            }

            /// <summary>
            /// Сохранение локации без связей с другими элементами.
            /// Ожидание: файл с верными данными локации.
            /// </summary>
            [Fact]
            public void Serialize_LocationWithoutBonds_CorrectFile()
            {
                // Arrange
                var location = new Element(ElemType.Location, "Name", "Description");
                location.Params.Add("Coordinates", new List<double> { 10.0, 20.0 });
                // Act
                Serializer.Serialize(location, "LocationWithoutBonds.txt");
                // Assert
                var actual = File.ReadAllText("LocationWithoutBonds.txt");
                var expected = @"{
  ""$id"": ""1"",
  ""$type"": ""Element"",
  ""Type"": 2,
  ""Name"": ""Name"",
  ""Description"": ""Description"",
  ""Params"": {
    ""$id"": ""2"",
    ""Coordinates"": {
      ""$id"": ""3"",
      ""$values"": [
        10,
        20
      ]
    }
  },
  ""Time"": -1
}";
                Assert.Equal(expected, actual);
                File.Delete("LocationWithoutBonds.txt");
            }

            /// <summary>
            /// Десериализация локации без связей с другими элементами.
            /// Ожидание: объект локации с верными данными.
            /// </summary>
            [Fact]
            public void Deserialize_LocationWithoutBonds_CorrectObject()
            {
                // Arrange
                Serialize_LocationWithoutBonds_CorrectFile();
                var location = new Element(ElemType.Location, "Name", "Description");
                location.Params.Add("Coordinates", new List<double> { 10.5, 20.5 });
                Serializer.Serialize(location, "LocationWithoutBonds.txt");
                // Act
                var actual = Serializer.Deserialize<Element>("LocationWithoutBonds.txt");
                // Assert
                Assert.Equal(location.Type, actual.Type);
                Assert.Equal(location.Name, actual.Name);
                Assert.Equal(location.Description, actual.Description);
                Assert.Equal(location.Params, actual.Params);
                Assert.Equal(location.Time, actual.Time);
                File.Delete("LocationWithoutBonds.txt");
            }

            /// <summary>
            /// Сохранение события без связей с другими элементами.
            /// Ожидание: файл с верными данными события.
            /// </summary>
            [Fact]
            public void Serialize_EventWithoutBonds_CorrectFile()
            {
                // Arrange
                var @event = new Element(ElemType.Event, "Name", "Description");
                @event.Params.Add("Date", new DateTime(2021, 10, 10));
                // Act
                Serializer.Serialize(@event, "EventWithoutBonds.txt");
                // Assert
                var actual = File.ReadAllText("EventWithoutBonds.txt");
                var expected = @"{
  ""$id"": ""1"",
  ""$type"": ""Element"",
  ""Type"": 3,
  ""Name"": ""Name"",
  ""Description"": ""Description"",
  ""Params"": {
    ""$id"": ""2"",
    ""Date"": ""2021-10-10T00:00:00""
  },
  ""Time"": -1
}";
                Assert.Equal(expected, actual);
                File.Delete("EventWithoutBonds.txt");
            }

            /// <summary>
            /// Десериализация события без связей с другими элементами.
            /// Ожидание: объект события с верными данными.
            /// </summary>
            [Fact]
            public void Deserialize_EventWithoutBonds_CorrectObject()
            {
                // Arrange
                Serialize_EventWithoutBonds_CorrectFile();
                var @event = new Element(ElemType.Event, "Name", "Description");
                @event.Params.Add("Date", new DateTime(2021, 10, 10));
                Serializer.Serialize(@event, "EventWithoutBonds.txt");
                // Act
                var actual = Serializer.Deserialize<Element>("EventWithoutBonds.txt");
                // Assert
                Assert.Equal(@event.Type, actual.Type);
                Assert.Equal(@event.Name, actual.Name);
                Assert.Equal(@event.Description, actual.Description);
                Assert.Equal(@event.Params, actual.Params);
                Assert.Equal(@event.Time, actual.Time);
                File.Delete("EventWithoutBonds.txt");
            }
        }
    }
}
