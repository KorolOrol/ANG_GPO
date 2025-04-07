using BaseClasses.Model;
using BaseClasses.Enum;
using BaseClasses.Services;
using System.Collections;
using System.Collections.Generic;

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

        /// <summary>
        /// Тесты для класса Serializer. Сохранение и загрузка элементов истории со связями.
        /// </summary>
        public class SerializerElementsWithBondsTests
        {
            private bool CompareElements(Element element1, Element element2, List<Element> refHandler = null)
            {
                if (refHandler == null) refHandler = new List<Element>();
                if (element1.Type != element2.Type) return false;
                if (element1.Name != element2.Name) return false;
                if (element1.Description != element2.Description) return false;
                if (element1.Time != element2.Time) return false;
                if (element1.Params.Count != element2.Params.Count) return false;
                foreach (var key in element1.Params.Keys)
                {
                    if (!element2.Params.ContainsKey(key)) return false;
                    if (element1.Params[key] is IEnumerable)
                    {
                        if (element2.Params[key] is not IEnumerable) return false;
                        var list1 = (IEnumerable)element1.Params[key];
                        var list2 = (IEnumerable)element2.Params[key];
                        var enumerator1 = list1.GetEnumerator();
                        while (enumerator1.MoveNext())
                        {
                            var current1 = enumerator1.Current;
                            if (current1 is Element element)
                            {
                                if (refHandler.Contains(element)) continue;
                                refHandler.Add(element);
                                var found = false;
                                foreach (var item in list2)
                                {
                                    if (item is Element foundElement &&
                                        CompareElements(element, foundElement, refHandler))
                                    {
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found) return false;
                            }
                        }
                    }
                    else
                    {
                        if (!CompareElements((Element)element1.Params[key], 
                                             (Element)element2.Params[key], 
                                             refHandler)) return false;
                    }
                }
                return true;
            }

            private bool CompareRelations(Relation relation1, Relation relation2)
            {
                if (relation1.Value != relation2.Value) return false;
                return CompareElements((Element)relation1.Character, (Element)relation2.Character);
            }

            private bool Compare<T>(T element1, T element2)
            {
                if (element1 is IEnumerable enum1 && element2 is IEnumerable enum2)
                {
                    var enumerator1 = enum1.GetEnumerator();
                    while (enumerator1.MoveNext())
                    {
                        bool found = false;
                        var enumerator2 = enum2.GetEnumerator();
                        while (enumerator2.MoveNext())
                        {
                            if (Compare(enumerator1.Current, enumerator2.Current))
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found) return false;
                    }
                    return true;
                }
                if (element1 is Element e1 && element2 is Element e2)
                {
                    return CompareElements(e1, e2);
                }
                else if (element1 is Relation r1 && element2 is Relation r2)
                {
                    return CompareRelations(r1, r2);
                }
                return false;
            }

            /// <summary>
            /// Сохранение персонажа со связями с другими элементами.
            /// Ожидание: файл с верными данными персонажа.
            /// </summary>
            [Fact]
            public void Serialize_CharacterWithBonds_CorrectFile()
            {
                // Arrange
                var character = new Element(ElemType.Character, "Name1", "Description1");
                var character2 = new Element(ElemType.Character, "Name2", "Description2");
                var item = new Element(ElemType.Item, "Name3", "Description3");
                var location = new Element(ElemType.Location, "Name4", "Description4");
                var @event = new Element(ElemType.Event, "Name5", "Description5");
                Binder.Bind(character, character2, 10);
                Binder.Bind(character, item);
                Binder.Bind(character, location);
                Binder.Bind(character, @event);
                // Act
                Serializer.Serialize(character, "CharacterWithBonds.txt");
                // Assert
                var actual = File.ReadAllText("CharacterWithBonds.txt");
                var expected = @"{
  ""$id"": ""1"",
  ""$type"": ""Element"",
  ""Type"": 0,
  ""Name"": ""Name1"",
  ""Description"": ""Description1"",
  ""Params"": {
    ""$id"": ""2"",
    ""Relations"": {
      ""$id"": ""3"",
      ""$values"": [
        {
          ""$id"": ""4"",
          ""$type"": ""Relation"",
          ""Character"": {
            ""$id"": ""5"",
            ""$type"": ""Element"",
            ""Type"": 0,
            ""Name"": ""Name2"",
            ""Description"": ""Description2"",
            ""Params"": {
              ""$id"": ""6"",
              ""Relations"": {
                ""$id"": ""7"",
                ""$values"": [
                  {
                    ""$id"": ""8"",
                    ""$type"": ""Relation"",
                    ""Character"": {
                      ""$ref"": ""1""
                    },
                    ""Value"": 10
                  }
                ]
              }
            },
            ""Time"": -1
          },
          ""Value"": 10
        }
      ]
    },
    ""Items"": {
      ""$id"": ""9"",
      ""$values"": [
        {
          ""$id"": ""10"",
          ""$type"": ""Element"",
          ""Type"": 1,
          ""Name"": ""Name3"",
          ""Description"": ""Description3"",
          ""Params"": {
            ""$id"": ""11"",
            ""Host"": {
              ""$ref"": ""1""
            }
          },
          ""Time"": -1
        }
      ]
    },
    ""Locations"": {
      ""$id"": ""12"",
      ""$values"": [
        {
          ""$id"": ""13"",
          ""$type"": ""Element"",
          ""Type"": 2,
          ""Name"": ""Name4"",
          ""Description"": ""Description4"",
          ""Params"": {
            ""$id"": ""14"",
            ""Characters"": {
              ""$id"": ""15"",
              ""$values"": [
                {
                  ""$ref"": ""1""
                }
              ]
            }
          },
          ""Time"": -1
        }
      ]
    },
    ""Events"": {
      ""$id"": ""16"",
      ""$values"": [
        {
          ""$id"": ""17"",
          ""$type"": ""Element"",
          ""Type"": 3,
          ""Name"": ""Name5"",
          ""Description"": ""Description5"",
          ""Params"": {
            ""$id"": ""18"",
            ""Characters"": {
              ""$id"": ""19"",
              ""$values"": [
                {
                  ""$ref"": ""1""
                }
              ]
            }
          },
          ""Time"": -1
        }
      ]
    }
  },
  ""Time"": -1
}";
                Assert.Equal(expected, actual);
                File.Delete("CharacterWithBonds.txt");
            }

            /// <summary>
            /// Десериализация персонажа со связями с другими элементами.
            /// Ожидание: объект персонажа с верными данными.
            /// </summary>
            [Fact]
            public void Deserialize_CharacterWithBonds_CorrectObject()
            {
                // Arrange
                Serialize_CharacterWithBonds_CorrectFile();
                var character = new Element(ElemType.Character, "Name1", "Description1");
                var character2 = new Element(ElemType.Character, "Name2", "Description2");
                var item = new Element(ElemType.Item, "Name3", "Description3");
                var location = new Element(ElemType.Location, "Name4", "Description4");
                var @event = new Element(ElemType.Event, "Name5", "Description5");
                Binder.Bind(character, character2, 10);
                Binder.Bind(character, item);
                Binder.Bind(character, location);
                Binder.Bind(character, @event);
                Serializer.Serialize(character, "CharacterWithBonds.txt");
                // Act
                var actual = Serializer.Deserialize<Element>("CharacterWithBonds.txt");
                // Assert
                Assert.Equal(character.Type, actual.Type);
                Assert.Equal(character.Name, actual.Name);
                Assert.Equal(character.Description, actual.Description);
                Assert.Equal(character.Params.Count, actual.Params.Count);
                foreach (var key in character.Params.Keys)
                {
                    Assert.True(actual.Params.ContainsKey(key));
                    Assert.Equal(character.Params[key], actual.Params[key], Compare);

                }
                Assert.Equal(character.Time, actual.Time);
                File.Delete("CharacterWithBonds.txt");
            }

            /// <summary>
            /// Сохранение предмета со связями с другими элементами.
            /// Ожидание: файл с верными данными предмета.
            /// </summary>
            [Fact]
            public void Serialize_ItemWithBonds_CorrectFile()
            {
                // Arrange
                var item = new Element(ElemType.Item, "Name1", "Description1");
                var character = new Element(ElemType.Character, "Name2", "Description2");
                var location = new Element(ElemType.Location, "Name3", "Description3");
                var @event = new Element(ElemType.Event, "Name4", "Description4");
                Binder.Bind(item, character);
                Binder.Bind(item, location);
                Binder.Bind(item, @event);
                // Act
                Serializer.Serialize(item, "ItemWithBonds.txt");
                // Assert
                var actual = File.ReadAllText("ItemWithBonds.txt");
                var expected = @"{
  ""$id"": ""1"",
  ""$type"": ""Element"",
  ""Type"": 1,
  ""Name"": ""Name1"",
  ""Description"": ""Description1"",
  ""Params"": {
    ""$id"": ""2"",
    ""Host"": {
      ""$id"": ""3"",
      ""$type"": ""Element"",
      ""Type"": 0,
      ""Name"": ""Name2"",
      ""Description"": ""Description2"",
      ""Params"": {
        ""$id"": ""4"",
        ""Items"": {
          ""$id"": ""5"",
          ""$values"": [
            {
              ""$ref"": ""1""
            }
          ]
        }
      },
      ""Time"": -1
    },
    ""Location"": {
      ""$id"": ""6"",
      ""$type"": ""Element"",
      ""Type"": 2,
      ""Name"": ""Name3"",
      ""Description"": ""Description3"",
      ""Params"": {
        ""$id"": ""7"",
        ""Items"": {
          ""$id"": ""8"",
          ""$values"": [
            {
              ""$ref"": ""1""
            }
          ]
        }
      },
      ""Time"": -1
    },
    ""Events"": {
      ""$id"": ""9"",
      ""$values"": [
        {
          ""$id"": ""10"",
          ""$type"": ""Element"",
          ""Type"": 3,
          ""Name"": ""Name4"",
          ""Description"": ""Description4"",
          ""Params"": {
            ""$id"": ""11"",
            ""Items"": {
              ""$id"": ""12"",
              ""$values"": [
                {
                  ""$ref"": ""1""
                }
              ]
            }
          },
          ""Time"": -1
        }
      ]
    }
  },
  ""Time"": -1
}";
                Assert.Equal(expected, actual);
                File.Delete("ItemWithBonds.txt");
            }

            /// <summary>
            /// Десериализация предмета со связями с другими элементами.
            /// Ожидание: объект предмета с верными данными.
            /// </summary>
            [Fact]
            public void Deserialize_ItemWithBonds_CorrectObject()
            {
                // Arrange
                Serialize_ItemWithBonds_CorrectFile();
                var item = new Element(ElemType.Item, "Name1", "Description1");
                var character = new Element(ElemType.Character, "Name2", "Description2");
                var location = new Element(ElemType.Location, "Name3", "Description3");
                var @event = new Element(ElemType.Event, "Name4", "Description4");
                Binder.Bind(item, character);
                Binder.Bind(item, location);
                Binder.Bind(item, @event);
                Serializer.Serialize(item, "ItemWithBonds.txt");
                // Act
                var actual = Serializer.Deserialize<Element>("ItemWithBonds.txt");
                // Assert
                Assert.Equal(item.Type, actual.Type);
                Assert.Equal(item.Name, actual.Name);
                Assert.Equal(item.Description, actual.Description);
                Assert.Equal(item.Params.Count, actual.Params.Count);
                foreach (var key in item.Params.Keys)
                {
                    Assert.True(actual.Params.ContainsKey(key));
                    Assert.Equal(item.Params[key], actual.Params[key], Compare);
                }
                Assert.Equal(item.Time, actual.Time);
                File.Delete("ItemWithBonds.txt");
            }
        }
    }
}
