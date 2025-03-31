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
                Serializer.Serialize(character, "CharacterWithoutBonds.json");
                // Assert
                var expected = File.ReadAllText("CharacterWithoutBonds.json");
                var actual = "";
                Assert.Equal(expected, actual);
            }
        }
    }
}
