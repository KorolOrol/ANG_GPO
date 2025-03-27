using BaseClasses.Model;
using BaseClasses.Enum;
using BaseClasses.Services;
using BaseClasses.Interface;

namespace BaseClasses.Tests.Services
{
    /// <summary>
    /// Тесты для класса Binder
    /// </summary>
    public class BinderTests
    {
        /// <summary>
        /// Тесты для класса Binder. Связывание и разрыв связей персонажей.
        /// </summary>
        public class BinderCharactersTests
        {
            /// <summary>
            /// Связывание двух пустых персонажей с ненулевым значением отношения.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_EmptyCharactersWithNonZeroRelation_SuccessfulBinding()
            {
                // Arrange
                var character1 = new Element(ElemType.Character);
                var character2 = new Element(ElemType.Character);
                // Act
                Binder.Bind(character1, character2, 1);
                // Assert
                Assert.True(character1.Params.ContainsKey("Relations"));
                Assert.True(character2.Params.ContainsKey("Relations"));
                Assert.True(character1.Params["Relations"] is List<Relation>);
                Assert.True(character2.Params["Relations"] is List<Relation>);
                Assert.Single((List<Relation>)character1.Params["Relations"]);
                Assert.Single((List<Relation>)character2.Params["Relations"]);
                Assert.Equal(1, ((List<Relation>)character1.Params["Relations"])[0].Value);
                Assert.Equal(1, ((List<Relation>)character2.Params["Relations"])[0].Value);
                Assert.Equal(character1, ((List<Relation>)character2.Params["Relations"])[0].Character);
                Assert.Equal(character2, ((List<Relation>)character1.Params["Relations"])[0].Character);
            }

            /// <summary>
            /// Связывание двух пустых персонажей с нулевым значением отношения.
            /// Ожидание: никаких изменений.
            /// </summary>
            [Fact]
            public void Bind_EmptyCharactersWithZeroRelation_NoChanges()
            {
                // Arrange
                var character1 = new Element(ElemType.Character);
                var character2 = new Element(ElemType.Character);
                // Act
                Binder.Bind(character1, character2, 0);
                // Assert
                Assert.False(character1.Params.ContainsKey("Relations"));
                Assert.False(character2.Params.ContainsKey("Relations"));
            }

            /// <summary>
            /// Связывание персонажа с самим собой.
            /// Ожидание: никаких изменений.
            /// </summary>
            [Fact]
            public void Bind_CharacterWithItself_NoChanges()
            {
                // Arrange
                var character = new Element(ElemType.Character);
                // Act
                Binder.Bind(character, character, 1);
                // Assert
                Assert.False(character.Params.ContainsKey("Relations"));
            }

            /// <summary>
            /// Связывание уже связанных персонажей.
            /// Ожидание: обновление значения отношения.
            /// </summary>
            [Fact]
            public void Bind_BoundCharacters_UpdateRelationValue()
            {
                // Arrange
                Bind_EmptyCharactersWithNonZeroRelation_SuccessfulBinding();
                var character1 = new Element(ElemType.Character);
                var character2 = new Element(ElemType.Character);
                Binder.Bind(character1, character2, 1);
                // Act
                Binder.Bind(character1, character2, 2);
                // Assert
                Assert.Single((List<Relation>)character1.Params["Relations"]);
                Assert.Single((List<Relation>)character2.Params["Relations"]);
                Assert.Equal(2, ((List<Relation>)character1.Params["Relations"])[0].Value);
                Assert.Equal(2, ((List<Relation>)character2.Params["Relations"])[0].Value);
            }

            /// <summary>
            /// Связывание двух персонажей, один из которых уже связан с другим.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_OneBoundCharacter_SuccessfulBinding()
            {
                // Arrange
                var character1 = new Element(ElemType.Character);
                var character2 = new Element(ElemType.Character, @params:
                    new Dictionary<string, object> { { "Relations", new List<Relation>
                { new Relation() { Character = character1, Value = 2 } } } });
                // Act
                Binder.Bind(character1, character2, 1);
                // Assert
                Assert.True(character1.Params.ContainsKey("Relations"));
                Assert.True(character2.Params.ContainsKey("Relations"));
                Assert.True(character1.Params["Relations"] is List<Relation>);
                Assert.True(character2.Params["Relations"] is List<Relation>);
                Assert.Single((List<Relation>)character1.Params["Relations"]);
                Assert.Single((List<Relation>)character2.Params["Relations"]);
                Assert.Equal(1, ((List<Relation>)character1.Params["Relations"])[0].Value);
                Assert.Equal(1, ((List<Relation>)character2.Params["Relations"])[0].Value);
                Assert.Equal(character1, ((List<Relation>)character2.Params["Relations"])[0].Character);
                Assert.Equal(character2, ((List<Relation>)character1.Params["Relations"])[0].Character);
            }

            /// <summary>
            /// Разрыв связи между двумя связанными персонажами.
            /// Ожидание: успешное разрывание связи.
            /// </summary>
            [Fact]
            public void Unbind_BoundCharacters_SuccessfulUnbinding()
            {
                // Arrange
                Bind_EmptyCharactersWithNonZeroRelation_SuccessfulBinding();
                var character1 = new Element(ElemType.Character);
                var character2 = new Element(ElemType.Character);
                Binder.Bind(character1, character2, 1);
                // Act
                Binder.Unbind(character1, character2);
                // Assert
                Assert.Empty((List<Relation>)character1.Params["Relations"]);
                Assert.Empty((List<Relation>)character2.Params["Relations"]);
            }

            /// <summary>
            /// Разрыв связи между двумя несвязанными персонажами. Персонажи не содержат параметр отношений.
            /// Ожидание: никаких изменений.
            /// </summary>
            [Fact]
            public void Unbind_UnboundCharacters_NoChanges()
            {
                // Arrange
                var character1 = new Element(ElemType.Character);
                var character2 = new Element(ElemType.Character);
                // Act
                Binder.Unbind(character1, character2);
                // Assert
                Assert.False(character1.Params.ContainsKey("Relations"));
                Assert.False(character2.Params.ContainsKey("Relations"));
            }

            /// <summary>
            /// Разрыв связи между двумя несвязанными персонажами. Персонажи содержат параметр отношений.
            /// Ожидание: никаких изменений.
            /// </summary>
            [Fact]
            public void Unbind_UnboundCharactersWithRelations_NoChanges()
            {
                // Arrange
                var character1 = new Element(ElemType.Character, @params:
                    new Dictionary<string, object> { { "Relations", new List<Relation>() } });
                var character2 = new Element(ElemType.Character, @params:
                    new Dictionary<string, object> { { "Relations", new List<Relation>() } });
                // Act
                Binder.Unbind(character1, character2);
                // Assert
                Assert.True(character1.Params.ContainsKey("Relations"));
                Assert.True(character2.Params.ContainsKey("Relations"));
                Assert.True(character1.Params["Relations"] is List<Relation>);
                Assert.True(character2.Params["Relations"] is List<Relation>);
                Assert.Empty((List<Relation>)character1.Params["Relations"]);
                Assert.Empty((List<Relation>)character2.Params["Relations"]);
            }

            /// <summary>
            /// Разрыв связи между персонажем и самим собой.
            /// Ожидание: никаких изменений.
            /// </summary>
            [Fact]
            public void Unbind_CharacterWithItself_NoChanges()
            {
                // Arrange
                var character = new Element(ElemType.Character);
                // Act
                Binder.Unbind(character, character);
                // Assert
                Assert.False(character.Params.ContainsKey("Relations"));
            }

            /// <summary>
            /// Разрыв связи между персонажем и другим персонажем, один из которых не связан с другим.
            /// Ожидание: разрыв существующих связей.
            /// </summary>
            [Fact]
            public void Unbind_OneUnboundCharacter_SuccessfulUnbinding()
            {
                // Arrange
                var character1 = new Element(ElemType.Character, @params:
                    new Dictionary<string, object> { { "Relations", new List<Relation>() } });
                var character2 = new Element(ElemType.Character, @params:
                    new Dictionary<string, object> { { "Relations", new List<Relation>
                { new Relation() { Character = character1, Value = 2 } } } });
                // Act
                Binder.Unbind(character1, character2);
                // Assert
                Assert.Empty((List<Relation>)character1.Params["Relations"]);
                Assert.Empty((List<Relation>)character2.Params["Relations"]);
            }
        }

        /// <summary>
        /// Тесты для класса Binder. Связывание и разрыв связей персонажей и предметов.
        /// </summary>
        public class BinderCharItemTests
        {
            /// <summary>
            /// Связывание персонажа и предмета.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_CharacterAndItem_SuccessfulBinding()
            {
                // Arrange
                var character = new Element(ElemType.Character);
                var item = new Element(ElemType.Item);
                // Act
                Binder.Bind(character, item);
                // Assert
                Assert.True(character.Params.ContainsKey("Items"));
                Assert.True(item.Params.ContainsKey("Host"));
                Assert.True(character.Params["Items"] is List<IElement>);
                Assert.True(item.Params["Host"] is IElement);
                Assert.Single((List<IElement>)character.Params["Items"]);
                Assert.NotNull(item.Params["Host"]);
                Assert.Equal(item, ((List<IElement>)character.Params["Items"])[0]);
                Assert.Equal(character, (IElement)item.Params["Host"]);
            }

            /// <summary>
            /// Связывание персонажа и предмета с ненулевым значением отношения.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_CharacterAndItemWithNonZeroRelation_SuccessfulBinding()
            {
                // Arrange
                var character = new Element(ElemType.Character);
                var item = new Element(ElemType.Item);
                // Act
                Binder.Bind(character, item, 1);
                // Assert
                Assert.True(character.Params.ContainsKey("Items"));
                Assert.True(item.Params.ContainsKey("Host"));
                Assert.True(character.Params["Items"] is List<IElement>);
                Assert.True(item.Params["Host"] is IElement);
                Assert.Single((List<IElement>)character.Params["Items"]);
                Assert.NotNull(item.Params["Host"]);
                Assert.Equal(item, ((List<IElement>)character.Params["Items"])[0]);
                Assert.Equal(character, (IElement)item.Params["Host"]);
            }

            /// <summary>
            /// Связывание персонажа и предмета, когда персонаж уже содержит предмет.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_CharacterWithItem_SuccessfulBinding()
            {
                // Arrange
                var item = new Element(ElemType.Item);
                var character = new Element(ElemType.Character, @params:
                    new Dictionary<string, object> { { "Items", new List<IElement> { item } } });
                // Act
                Binder.Bind(character, item);
                // Assert
                Assert.True(character.Params.ContainsKey("Items"));
                Assert.True(item.Params.ContainsKey("Host"));
                Assert.True(character.Params["Items"] is List<IElement>);
                Assert.True(item.Params["Host"] is IElement);
                Assert.Single((List<IElement>)character.Params["Items"]);
                Assert.NotNull(item.Params["Host"]);
                Assert.Equal(item, ((List<IElement>)character.Params["Items"])[0]);
                Assert.Equal(character, (IElement)item.Params["Host"]);
            }

            /// <summary>
            /// Связывание персонажа и предмета, когда предмет уже содержит владельца.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_ItemWithCharacter_SuccessfulBinding()
            {
                // Arrange
                var character = new Element(ElemType.Character);
                var item = new Element(ElemType.Item, @params: new Dictionary<string, object> { { "Host", character } });
                // Act
                Binder.Bind(character, item);
                // Assert
                Assert.True(character.Params.ContainsKey("Items"));
                Assert.True(item.Params.ContainsKey("Host"));
                Assert.True(character.Params["Items"] is List<IElement>);
                Assert.True(item.Params["Host"] is IElement);
                Assert.Single((List<IElement>)character.Params["Items"]);
                Assert.NotNull(item.Params["Host"]);
                Assert.Equal(item, ((List<IElement>)character.Params["Items"])[0]);
                Assert.Equal(character, (IElement)item.Params["Host"]);
            }

            /// <summary>
            /// Связывание персонажа и предмета, которые уже связаны.
            /// Ожидаение: никаких изменений.
            /// </summary>
            [Fact]
            public void Bind_BoundCharacterAndItem_NoChanges()
            {
                // Arrange
                Bind_CharacterAndItem_SuccessfulBinding();
                var character = new Element(ElemType.Character);
                var item = new Element(ElemType.Item);
                Binder.Bind(character, item);
                // Act
                Binder.Bind(character, item);
                // Assert
                Assert.True(character.Params.ContainsKey("Items"));
                Assert.True(item.Params.ContainsKey("Host"));
                Assert.True(character.Params["Items"] is List<IElement>);
                Assert.True(item.Params["Host"] is IElement);
                Assert.Single((List<IElement>)character.Params["Items"]);
                Assert.NotNull(item.Params["Host"]);
                Assert.Equal(item, ((List<IElement>)character.Params["Items"])[0]);
                Assert.Equal(character, (IElement)item.Params["Host"]);
            }

            /// <summary>
            /// Разрыв связи между персонажем и предметом.
            /// Ожидание: успешное разрывание связи.
            /// </summary>
            [Fact]
            public void Unbind_CharacterAndItem_SuccessfulUnbinding()
            {
                // Arrange
                Bind_CharacterAndItem_SuccessfulBinding();
                var character = new Element(ElemType.Character);
                var item = new Element(ElemType.Item);
                Binder.Bind(character, item);
                // Act
                Binder.Unbind(character, item);
                // Assert
                Assert.True(character.Params.ContainsKey("Items"));
                Assert.True(item.Params.ContainsKey("Host"));
                Assert.True(character.Params["Items"] is List<IElement>);
                Assert.Empty((List<IElement>)character.Params["Items"]);
                Assert.Null(item.Params["Host"]);
            }

            /// <summary>
            /// Разрыв связи между персонажем и предметом, когда персонаж не содержит предмета.
            /// Ожидание: разрыв существующей связи.
            /// </summary>
            [Fact]
            public void Unbind_CharacterWithoutItem_SuccessfulUnbinding()
            {
                // Arrange
                var character = new Element(ElemType.Character, @params: new Dictionary<string, object> {
                    { "Items", new List<IElement>() } });
                var item = new Element(ElemType.Item, @params: new Dictionary<string, object> {
                    { "Host", character } });
                // Act
                Binder.Unbind(character, item);
                // Assert
                Assert.True(character.Params.ContainsKey("Items"));
                Assert.True(item.Params.ContainsKey("Host"));
                Assert.True(character.Params["Items"] is List<IElement>);
                Assert.Empty((List<IElement>)character.Params["Items"]);
                Assert.Null(item.Params["Host"]);
            }

            /// <summary>
            /// Разрыв связи между персонажем и предметом, когда предмет не содержит владельца.
            /// Ожидаение: разрыв существующей связи.
            /// </summary>
            [Fact]
            public void Unbind_ItemWithoutCharacter_SuccessfulUnbinding()
            {
                // Arrange
                var item = new Element(ElemType.Item, @params: new Dictionary<string, object> {
                    { "Host", null } });
                var character = new Element(ElemType.Character, @params: new Dictionary<string, object> {
                    { "Items", new List<IElement> { item } } });
                // Act
                Binder.Unbind(character, item);
                // Assert
                Assert.True(character.Params.ContainsKey("Items"));
                Assert.True(item.Params.ContainsKey("Host"));
                Assert.True(character.Params["Items"] is List<IElement>);
                Assert.Empty((List<IElement>)character.Params["Items"]);
                Assert.Null(item.Params["Host"]);
            }

            /// <summary>
            /// Разрыв связи между персонажем и предметом, когда персонаж и предмет не связаны.
            /// Ожидание: никаких изменений.
            /// </summary>
            [Fact]
            public void Unbind_UnboundCharacterAndItem_NoChanges()
            {
                // Arrange
                var character = new Element(ElemType.Character);
                var item = new Element(ElemType.Item);
                // Act
                Binder.Unbind(character, item);
                // Assert
                Assert.False(character.Params.ContainsKey("Items"));
                Assert.False(item.Params.ContainsKey("Host"));
            }

            /// <summary>
            /// Разрыв связи между персонажем и предметом, которые имеют пустые параметры для хранения связей.
            /// Ожидание: никаких изменений.
            /// </summary>
            [Fact]
            public void Unbind_EmptyCharacterAndItem_NoChanges()
            {
                // Arrange
                var character = new Element(ElemType.Character, @params: new Dictionary<string, object> {
                    { "Items", new List<IElement>() } });
                var item = new Element(ElemType.Item, @params: new Dictionary<string, object> {
                    { "Host", null } });
                // Act
                Binder.Unbind(character, item);
                // Assert
                Assert.True(character.Params.ContainsKey("Items"));
                Assert.True(item.Params.ContainsKey("Host"));
                Assert.True(character.Params["Items"] is List<IElement>);
                Assert.Empty((List<IElement>)character.Params["Items"]);
                Assert.Null(item.Params["Host"]);
            }

            /// <summary>
            /// Переприязка предмета к другому персонажу.
            /// Ожидание: разрыв существующей связи и успешное связывание с новым персонажем.
            /// </summary>
            [Fact]
            public void Rebind_ItemToAnotherCharacter_SuccessfulRebinding()
            {
                // Arrange
                Bind_CharacterAndItem_SuccessfulBinding();
                Unbind_CharacterAndItem_SuccessfulUnbinding();
                var character1 = new Element(ElemType.Character);
                var character2 = new Element(ElemType.Character);
                var item = new Element(ElemType.Item);
                Binder.Bind(character1, item);
                // Act
                Binder.Bind(character2, item);
                // Assert
                Assert.True(character1.Params.ContainsKey("Items"));
                Assert.True(character2.Params.ContainsKey("Items"));
                Assert.True(item.Params.ContainsKey("Host"));
                Assert.True(character1.Params["Items"] is List<IElement>);
                Assert.True(character2.Params["Items"] is List<IElement>);
                Assert.NotNull(item.Params["Host"]);
                Assert.Empty((List<IElement>)character1.Params["Items"]);
                Assert.Single((List<IElement>)character2.Params["Items"]);
                Assert.Equal(item, ((List<IElement>)character2.Params["Items"])[0]);
                Assert.Equal(character2, (IElement)item.Params["Host"]);
            }
        }

        /// <summary>
        /// Тесты для класса Binder. Связывание и разрыв связей персонажей и локаций.
        /// </summary>
        public class BinderCharLocTests
        {
            /// <summary>
            /// Связывание персонажа и локации.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_CharacterAndLocation_SuccessfulBinding()
            {
                // Arrange
                var character = new Element(ElemType.Character);
                var location = new Element(ElemType.Location);
                // Act
                Binder.Bind(character, location);
                // Assert
                Assert.True(character.Params.ContainsKey("Locations"));
                Assert.True(location.Params.ContainsKey("Characters"));
                Assert.True(character.Params["Locations"] is List<IElement>);
                Assert.True(location.Params["Characters"] is List<IElement>);
                Assert.Single((List<IElement>)character.Params["Locations"]);
                Assert.Single((List<IElement>)location.Params["Characters"]);
                Assert.Equal(location, ((List<IElement>)character.Params["Locations"])[0]);
                Assert.Equal(character, ((List<IElement>)location.Params["Characters"])[0]);
            }

            /// <summary>
            /// Связывание персонажа и локации с ненулевым значением отношения.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_CharacterAndLocationWithNonZeroRelation_SuccessfulBinding()
            {
                // Arrange
                var character = new Element(ElemType.Character);
                var location = new Element(ElemType.Location);
                // Act
                Binder.Bind(character, location, 1);
                // Assert
                Assert.True(character.Params.ContainsKey("Locations"));
                Assert.True(location.Params.ContainsKey("Characters"));
                Assert.True(character.Params["Locations"] is List<IElement>);
                Assert.True(location.Params["Characters"] is List<IElement>);
                Assert.Single((List<IElement>)character.Params["Locations"]);
                Assert.Single((List<IElement>)location.Params["Characters"]);
                Assert.Equal(location, ((List<IElement>)character.Params["Locations"])[0]);
                Assert.Equal(character, ((List<IElement>)location.Params["Characters"])[0]);
            }

            /// <summary>
            /// Связывание персонажа и локации, когда персонаж уже содержит локацию.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_CharacterWithLocation_SuccessfulBinding()
            {
                // Arrange
                var location = new Element(ElemType.Location);
                var character = new Element(ElemType.Character, @params:
                    new Dictionary<string, object> { { "Locations", new List<IElement> { location } } });
                // Act
                Binder.Bind(character, location);
                // Assert
                Assert.True(character.Params.ContainsKey("Locations"));
                Assert.True(location.Params.ContainsKey("Characters"));
                Assert.True(character.Params["Locations"] is List<IElement>);
                Assert.True(location.Params["Characters"] is List<IElement>);
                Assert.Single((List<IElement>)character.Params["Locations"]);
                Assert.Single((List<IElement>)location.Params["Characters"]);
                Assert.Equal(location, ((List<IElement>)character.Params["Locations"])[0]);
                Assert.Equal(character, ((List<IElement>)location.Params["Characters"])[0]);
            }

            /// <summary>
            /// Связывание персонажа и локации, когда локация уже содержит персонажа.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_LocationWithCharacter_SuccessfulBinding()
            {
                // Arrange
                var character = new Element(ElemType.Character);
                var location = new Element(ElemType.Location, @params: new Dictionary<string, object> {
                    { "Characters", new List<IElement> { character } } });
                // Act
                Binder.Bind(character, location);
                // Assert
                Assert.True(character.Params.ContainsKey("Locations"));
                Assert.True(location.Params.ContainsKey("Characters"));
                Assert.True(character.Params["Locations"] is List<IElement>);
                Assert.True(location.Params["Characters"] is List<IElement>);
                Assert.Single((List<IElement>)character.Params["Locations"]);
                Assert.Single((List<IElement>)location.Params["Characters"]);
                Assert.Equal(location, ((List<IElement>)character.Params["Locations"])[0]);
                Assert.Equal(character, ((List<IElement>)location.Params["Characters"])[0]);
            }

            /// <summary>
            /// Связывание персонажа и локации, которые уже связаны.
            /// Ожидание: никаких изменений.
            /// </summary>
            [Fact]
            public void Bind_BoundCharacterAndLocation_NoChanges()
            {
                // Arrange
                Bind_CharacterAndLocation_SuccessfulBinding();
                var character = new Element(ElemType.Character);
                var location = new Element(ElemType.Location);
                Binder.Bind(character, location);
                // Act
                Binder.Bind(character, location);
                // Assert
                Assert.True(character.Params.ContainsKey("Locations"));
                Assert.True(location.Params.ContainsKey("Characters"));
                Assert.True(character.Params["Locations"] is List<IElement>);
                Assert.True(location.Params["Characters"] is List<IElement>);
                Assert.Single((List<IElement>)character.Params["Locations"]);
                Assert.Single((List<IElement>)location.Params["Characters"]);
                Assert.Equal(location, ((List<IElement>)character.Params["Locations"])[0]);
                Assert.Equal(character, ((List<IElement>)location.Params["Characters"])[0]);
            }

            /// <summary>
            /// Разрыв связи между персонажем и локацией.
            /// Ожидание: успешное разрывание связи.
            /// </summary>
            [Fact]
            public void Unbind_CharacterAndLocation_SuccessfulUnbinding()
            {
                // Arrange
                Bind_CharacterAndLocation_SuccessfulBinding();
                var character = new Element(ElemType.Character);
                var location = new Element(ElemType.Location);
                Binder.Bind(character, location);
                // Act
                Binder.Unbind(character, location);
                // Assert
                Assert.True(character.Params.ContainsKey("Locations"));
                Assert.True(location.Params.ContainsKey("Characters"));
                Assert.True(character.Params["Locations"] is List<IElement>);
                Assert.True(location.Params["Characters"] is List<IElement>);
                Assert.Empty((List<IElement>)character.Params["Locations"]);
                Assert.Empty((List<IElement>)location.Params["Characters"]);
            }

            /// <summary>
            /// Разрыв связи между персонажем и локацией, когда персонаж не содержит локации.
            /// Ожидание: разрыв существующей связи.
            /// </summary>
            [Fact]
            public void Unbind_CharacterWithoutLocation_SuccessfulUnbinding()
            {
                // Arrange
                var character = new Element(ElemType.Character, @params: new Dictionary<string, object> {
                    { "Locations", new List<IElement>() } });
                var location = new Element(ElemType.Location, @params: new Dictionary<string, object> {
                    { "Characters", new List<IElement> { character } } });
                // Act
                Binder.Unbind(character, location);
                // Assert
                Assert.True(character.Params.ContainsKey("Locations"));
                Assert.True(location.Params.ContainsKey("Characters"));
                Assert.True(character.Params["Locations"] is List<IElement>);
                Assert.True(location.Params["Characters"] is List<IElement>);
                Assert.Empty((List<IElement>)character.Params["Locations"]);
                Assert.Empty((List<IElement>)location.Params["Characters"]);
            }

            /// <summary>
            /// Разрыв связи между персонажем и локацией, когда локация не содержит персонажа.
            /// Ожидание: разрыв существующей связи.
            /// </summary>
            [Fact]
            public void Unbind_LocationWithoutCharacter_SuccessfulUnbinding()
            {
                // Arrange
                var location = new Element(ElemType.Location, @params: new Dictionary<string, object> {
                    { "Characters", new List<IElement>() } });
                var character = new Element(ElemType.Character, @params: new Dictionary<string, object> {
                    { "Locations", new List<IElement> { location } } });
                // Act
                Binder.Unbind(character, location);
                // Assert
                Assert.True(character.Params.ContainsKey("Locations"));
                Assert.True(location.Params.ContainsKey("Characters"));
                Assert.True(character.Params["Locations"] is List<IElement>);
                Assert.True(location.Params["Characters"] is List<IElement>);
                Assert.Empty((List<IElement>)character.Params["Locations"]);
                Assert.Empty((List<IElement>)location.Params["Characters"]);
            }

            /// <summary>
            /// Разрыв связи между персонажем и локацией, когда персонаж и локация не связаны.
            /// Ожидание: никаких изменений.
            /// </summary>
            [Fact]
            public void Unbind_UnboundCharacterAndLocation_NoChanges()
            {
                // Arrange
                var character = new Element(ElemType.Character);
                var location = new Element(ElemType.Location);
                // Act
                Binder.Unbind(character, location);
                // Assert
                Assert.False(character.Params.ContainsKey("Locations"));
                Assert.False(location.Params.ContainsKey("Characters"));
            }

            /// <summary>
            /// Разрыв связи между персонажем и локацией, которые имеют пустые параметры для хранения связей.
            /// Ожидание: никаких изменений.
            /// </summary>
            [Fact]
            public void Unbind_EmptyCharacterAndLocation_NoChanges()
            {
                // Arrange
                var character = new Element(ElemType.Character, @params: new Dictionary<string, object> {
                    { "Locations", new List<IElement>() } });
                var location = new Element(ElemType.Location, @params: new Dictionary<string, object> {
                    { "Characters", new List<IElement>() } });
                // Act
                Binder.Unbind(character, location);
                // Assert
                Assert.True(character.Params.ContainsKey("Locations"));
                Assert.True(location.Params.ContainsKey("Characters"));
                Assert.True(character.Params["Locations"] is List<IElement>);
                Assert.True(location.Params["Characters"] is List<IElement>);
                Assert.Empty((List<IElement>)character.Params["Locations"]);
                Assert.Empty((List<IElement>)location.Params["Characters"]);
            }
        }

        /// <summary>
        /// Тесты для класса Binder. Связывание и разрыв связей персонажей и событий.
        /// </summary>
        public class BinderCharEventTests
        {
            /// <summary>
            /// Связывание персонажа и события.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_CharacterAndEvent_SuccessfulBinding()
            {
                // Arrange
                var character = new Element(ElemType.Character);
                var @event = new Element(ElemType.Event);
                // Act
                Binder.Bind(character, @event);
                // Assert
                Assert.True(character.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Characters"));
                Assert.True(character.Params["Events"] is List<IElement>);
                Assert.True(@event.Params["Characters"] is List<IElement>);
                Assert.Single((List<IElement>)character.Params["Events"]);
                Assert.Single((List<IElement>)@event.Params["Characters"]);
                Assert.Equal(@event, ((List<IElement>)character.Params["Events"])[0]);
                Assert.Equal(character, ((List<IElement>)@event.Params["Characters"])[0]);
            }

            /// <summary>
            /// Связывание персонажа и события с ненулевым значением отношения.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_CharacterAndEventWithNonZeroRelation_SuccessfulBinding()
            {
                // Arrange
                var character = new Element(ElemType.Character);
                var @event = new Element(ElemType.Event);
                // Act
                Binder.Bind(character, @event, 1);
                // Assert
                Assert.True(character.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Characters"));
                Assert.True(character.Params["Events"] is List<IElement>);
                Assert.True(@event.Params["Characters"] is List<IElement>);
                Assert.Single((List<IElement>)character.Params["Events"]);
                Assert.Single((List<IElement>)@event.Params["Characters"]);
                Assert.Equal(@event, ((List<IElement>)character.Params["Events"])[0]);
                Assert.Equal(character, ((List<IElement>)@event.Params["Characters"])[0]);
            }

            /// <summary>
            /// Связывание персонажа и события, когда персонаж уже содержит событие.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_CharacterWithEvent_SuccessfulBinding()
            {
                // Arrange
                var @event = new Element(ElemType.Event);
                var character = new Element(ElemType.Character, @params:
                    new Dictionary<string, object> { { "Events", new List<IElement> { @event } } });
                // Act
                Binder.Bind(character, @event);
                // Assert
                Assert.True(character.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Characters"));
                Assert.True(character.Params["Events"] is List<IElement>);
                Assert.True(@event.Params["Characters"] is List<IElement>);
                Assert.Single((List<IElement>)character.Params["Events"]);
                Assert.Single((List<IElement>)@event.Params["Characters"]);
                Assert.Equal(@event, ((List<IElement>)character.Params["Events"])[0]);
                Assert.Equal(character, ((List<IElement>)@event.Params["Characters"])[0]);
            }

            /// <summary>
            /// Связывание персонажа и события, когда событие уже содержит персонажа.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_EventWithCharacter_SuccessfulBinding()
            {
                // Arrange
                var character = new Element(ElemType.Character);
                var @event = new Element(ElemType.Event, @params: new Dictionary<string, object> {
                    { "Characters", new List<IElement> { character } } });
                // Act
                Binder.Bind(character, @event);
                // Assert
                Assert.True(character.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Characters"));
                Assert.True(character.Params["Events"] is List<IElement>);
                Assert.True(@event.Params["Characters"] is List<IElement>);
                Assert.Single((List<IElement>)character.Params["Events"]);
                Assert.Single((List<IElement>)@event.Params["Characters"]);
                Assert.Equal(@event, ((List<IElement>)character.Params["Events"])[0]);
                Assert.Equal(character, ((List<IElement>)@event.Params["Characters"])[0]);
            }

            /// <summary>
            /// Связывание персонажа и события, которые уже связаны.
            /// Ожидание: никаких изменений.
            /// </summary>
            [Fact]
            public void Bind_BoundCharacterAndEvent_NoChanges()
            {
                // Arrange
                Bind_CharacterAndEvent_SuccessfulBinding();
                var character = new Element(ElemType.Character);
                var @event = new Element(ElemType.Event);
                Binder.Bind(character, @event);
                // Act
                Binder.Bind(character, @event);
                // Assert
                Assert.True(character.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Characters"));
                Assert.True(character.Params["Events"] is List<IElement>);
                Assert.True(@event.Params["Characters"] is List<IElement>);
                Assert.Single((List<IElement>)character.Params["Events"]);
                Assert.Single((List<IElement>)@event.Params["Characters"]);
                Assert.Equal(@event, ((List<IElement>)character.Params["Events"])[0]);
                Assert.Equal(character, ((List<IElement>)@event.Params["Characters"])[0]);
            }

            /// <summary>
            /// Разрыв связи между персонажем и событием.
            /// Ожидание: успешное разрывание связи.
            /// </summary>
            [Fact]
            public void Unbind_CharacterAndEvent_SuccessfulUnbinding()
            {
                // Arrange
                Bind_CharacterAndEvent_SuccessfulBinding();
                var character = new Element(ElemType.Character);
                var @event = new Element(ElemType.Event);
                Binder.Bind(character, @event);
                // Act
                Binder.Unbind(character, @event);
                // Assert
                Assert.True(character.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Characters"));
                Assert.True(character.Params["Events"] is List<IElement>);
                Assert.True(@event.Params["Characters"] is List<IElement>);
                Assert.Empty((List<IElement>)character.Params["Events"]);
                Assert.Empty((List<IElement>)@event.Params["Characters"]);
            }

            /// <summary>
            /// Разрыв связи между персонажем и событием, когда персонаж не содержит события.
            /// Ожидание: разрыв существующей связи.
            /// </summary>
            [Fact]
            public void Unbind_CharacterWithoutEvent_SuccessfulUnbinding()
            {
                // Arrange
                var character = new Element(ElemType.Character, @params: new Dictionary<string, object> {
                    { "Events", new List<IElement>() } });
                var @event = new Element(ElemType.Event, @params: new Dictionary<string, object> {
                    { "Characters", new List<IElement> { character } } });
                // Act
                Binder.Unbind(character, @event);
                // Assert
                Assert.True(character.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Characters"));
                Assert.True(character.Params["Events"] is List<IElement>);
                Assert.True(@event.Params["Characters"] is List<IElement>);
                Assert.Empty((List<IElement>)character.Params["Events"]);
                Assert.Empty((List<IElement>)@event.Params["Characters"]);
            }

            /// <summary>
            /// Разрыв связи между персонажем и событием, когда событие не содержит персонажа.
            /// Ожидание: разрыв существующей связи.
            /// </summary>
            [Fact]
            public void Unbind_EventWithoutCharacter_SuccessfulUnbinding()
            {
                // Arrange
                var @event = new Element(ElemType.Event, @params: new Dictionary<string, object> {
                    { "Characters", new List<IElement>() } });
                var character = new Element(ElemType.Character, @params: new Dictionary<string, object> {
                    { "Events", new List<IElement> { @event } } });
                // Act
                Binder.Unbind(character, @event);
                // Assert
                Assert.True(character.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Characters"));
                Assert.True(character.Params["Events"] is List<IElement>);
                Assert.True(@event.Params["Characters"] is List<IElement>);
                Assert.Empty((List<IElement>)character.Params["Events"]);
                Assert.Empty((List<IElement>)@event.Params["Characters"]);
            }

            /// <summary>
            /// Разрыв связи между персонажем и событием, когда персонаж и событие не связаны.
            /// Ожидание: никаких изменений.
            /// </summary>
            [Fact]
            public void Unbind_UnboundCharacterAndEvent_NoChanges()
            {
                // Arrange
                var character = new Element(ElemType.Character);
                var @event = new Element(ElemType.Event);
                // Act
                Binder.Unbind(character, @event);
                // Assert
                Assert.False(character.Params.ContainsKey("Events"));
                Assert.False(@event.Params.ContainsKey("Characters"));
            }

            /// <summary>
            /// Разрыв связи между персонажем и событием, которые имеют пустые параметры для хранения связей.
            /// Ожидание: никаких изменений.
            /// </summary>
            [Fact]
            public void Unbind_EmptyCharacterAndEvent_NoChanges()
            {
                // Arrange
                var character = new Element(ElemType.Character, @params: new Dictionary<string, object> {
                    { "Events", new List<IElement>() } });
                var @event = new Element(ElemType.Event, @params: new Dictionary<string, object> {
                    { "Characters", new List<IElement>() } });
                // Act
                Binder.Unbind(character, @event);
                // Assert
                Assert.True(character.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Characters"));
                Assert.True(character.Params["Events"] is List<IElement>);
                Assert.True(@event.Params["Characters"] is List<IElement>);
                Assert.Empty((List<IElement>)character.Params["Events"]);
                Assert.Empty((List<IElement>)@event.Params["Characters"]);
            }
        }

        /// <summary>
        /// Тесты для класса Binder. Связывание и разрыв связей предметов и локаций.
        /// </summary>
        public class BinderItemLocTests
        {
            /// <summary>
            /// Связывание предмета и локации.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_ItemAndLocation_SuccessfulBinding()
            {
                // Arrange
                var item = new Element(ElemType.Item);
                var location = new Element(ElemType.Location);
                // Act
                Binder.Bind(item, location);
                // Assert
                Assert.True(item.Params.ContainsKey("Location"));
                Assert.True(location.Params.ContainsKey("Items"));
                Assert.True(item.Params["Location"] is IElement);
                Assert.True(location.Params["Items"] is List<IElement>);
                Assert.NotNull((IElement)item.Params["Location"]);
                Assert.Single((List<IElement>)location.Params["Items"]);
                Assert.Equal(location, (IElement)item.Params["Location"]);
                Assert.Equal(item, ((List<IElement>)location.Params["Items"])[0]);
            }

            /// <summary>
            /// Связывание предмета и локации с ненулевым значением отношения.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_ItemAndLocationWithNonZeroRelation_SuccessfulBinding()
            {
                // Arrange
                var item = new Element(ElemType.Item);
                var location = new Element(ElemType.Location);
                // Act
                Binder.Bind(item, location, 1);
                // Assert
                Assert.True(item.Params.ContainsKey("Location"));
                Assert.True(location.Params.ContainsKey("Items"));
                Assert.True(item.Params["Location"] is IElement);
                Assert.True(location.Params["Items"] is List<IElement>);
                Assert.NotNull((IElement)item.Params["Location"]);
                Assert.Single((List<IElement>)location.Params["Items"]);
                Assert.Equal(location, (IElement)item.Params["Location"]);
                Assert.Equal(item, ((List<IElement>)location.Params["Items"])[0]);
            }

            /// <summary>
            /// Связывание предмета и локации, когда предмет уже содержит локацию.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_ItemWithLocation_SuccessfulBinding()
            {
                // Arrange
                var location = new Element(ElemType.Location);
                var item = new Element(ElemType.Item, @params: new Dictionary<string, object> { 
                    { "Location", location } });
                // Act
                Binder.Bind(item, location);
                // Assert
                Assert.True(item.Params.ContainsKey("Location"));
                Assert.True(location.Params.ContainsKey("Items"));
                Assert.True(item.Params["Location"] is IElement);
                Assert.True(location.Params["Items"] is List<IElement>);
                Assert.NotNull((IElement)item.Params["Location"]);
                Assert.Single((List<IElement>)location.Params["Items"]);
                Assert.Equal(location, (IElement)item.Params["Location"]);
                Assert.Equal(item, ((List<IElement>)location.Params["Items"])[0]);
            }

            /// <summary>
            /// Связывание предмета и локации, когда локация уже содержит предмет.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_LocationWithItem_SuccessfulBinding()
            {
                // Arrange
                var item = new Element(ElemType.Item);
                var location = new Element(ElemType.Location, @params: new Dictionary<string, object> {
                    { "Items", new List<IElement> { item } } });
                // Act
                Binder.Bind(item, location);
                // Assert
                Assert.True(item.Params.ContainsKey("Location"));
                Assert.True(location.Params.ContainsKey("Items"));
                Assert.True(item.Params["Location"] is IElement);
                Assert.True(location.Params["Items"] is List<IElement>);
                Assert.NotNull((IElement)item.Params["Location"]);
                Assert.Single((List<IElement>)location.Params["Items"]);
                Assert.Equal(location, (IElement)item.Params["Location"]);
                Assert.Equal(item, ((List<IElement>)location.Params["Items"])[0]);
            }

            /// <summary>
            /// Связывание предмета и локации, которые уже связаны.
            /// Ожидание: никаких изменений.
            /// </summary>
            [Fact]
            public void Bind_BoundItemAndLocation_NoChanges()
            {
                // Arrange
                Bind_ItemAndLocation_SuccessfulBinding();
                var item = new Element(ElemType.Item);
                var location = new Element(ElemType.Location);
                Binder.Bind(item, location);
                // Act
                Binder.Bind(item, location);
                // Assert
                Assert.True(item.Params.ContainsKey("Location"));
                Assert.True(location.Params.ContainsKey("Items"));
                Assert.True(item.Params["Location"] is IElement);
                Assert.True(location.Params["Items"] is List<IElement>);
                Assert.NotNull((IElement)item.Params["Location"]);
                Assert.Single((List<IElement>)location.Params["Items"]);
                Assert.Equal(location, (IElement)item.Params["Location"]);
                Assert.Equal(item, ((List<IElement>)location.Params["Items"])[0]);
            }

            /// <summary>
            /// Разрыв связи между предметом и локацией.
            /// Ожидание: успешное разрывание связи.
            /// </summary>
            [Fact]
            public void Unbind_ItemAndLocation_SuccessfulUnbinding()
            {
                // Arrange
                Bind_ItemAndLocation_SuccessfulBinding();
                var item = new Element(ElemType.Item);
                var location = new Element(ElemType.Location);
                Binder.Bind(item, location);
                // Act
                Binder.Unbind(item, location);
                // Assert
                Assert.True(item.Params.ContainsKey("Location"));
                Assert.True(location.Params.ContainsKey("Items"));
                Assert.True(location.Params["Items"] is List<IElement>);
                Assert.Null((IElement)item.Params["Location"]);
                Assert.Empty((List<IElement>)location.Params["Items"]);
            }

            /// <summary>
            /// Разрыв связи между предметом и локацией, когда предмет не содержит локации.
            /// Ожидание: разрыв существующей связи.
            /// </summary>
            [Fact]
            public void Unbind_ItemWithoutLocation_SuccessfulUnbinding()
            {
                // Arrange
                var item = new Element(ElemType.Item, @params: new Dictionary<string, object> {
                    { "Location", null } });
                var location = new Element(ElemType.Location, @params: new Dictionary<string, object> {
                    { "Items", new List<IElement>() } });
                // Act
                Binder.Unbind(item, location);
                // Assert
                Assert.True(item.Params.ContainsKey("Location"));
                Assert.True(location.Params.ContainsKey("Items"));
                Assert.True(location.Params["Items"] is List<IElement>);
                Assert.Null((IElement)item.Params["Location"]);
                Assert.Empty((List<IElement>)location.Params["Items"]);
            }

            /// <summary>
            /// Разрыв связи между предметом и локацией, когда локация не содержит предмета.
            /// Ожидание: разрыв существующей связи.
            /// </summary>
            [Fact]
            public void Unbind_LocationWithoutItem_SuccessfulUnbinding()
            {
                // Arrange
                var location = new Element(ElemType.Location, @params: new Dictionary<string, object> {
                    { "Items", new List<IElement>() } });
                var item = new Element(ElemType.Item, @params: new Dictionary<string, object> {
                    { "Location", location } });
                // Act
                Binder.Unbind(item, location);
                // Assert
                Assert.True(item.Params.ContainsKey("Location"));
                Assert.True(location.Params.ContainsKey("Items"));
                Assert.True(location.Params["Items"] is List<IElement>);
                Assert.Null((IElement)item.Params["Location"]);
                Assert.Empty((List<IElement>)location.Params["Items"]);
            }

            /// <summary>
            /// Разрыв связи между предметом и локацией, когда предмет и локация не связаны.
            /// Ожидание: никаких изменений.
            /// </summary>
            [Fact]
            public void Unbind_UnboundItemAndLocation_NoChanges()
            {
                // Arrange
                var item = new Element(ElemType.Item);
                var location = new Element(ElemType.Location);
                // Act
                Binder.Unbind(item, location);
                // Assert
                Assert.False(item.Params.ContainsKey("Location"));
                Assert.False(location.Params.ContainsKey("Items"));
            }

            /// <summary>
            /// Разрыв связи между предметом и локацией, которые имеют пустые параметры для хранения связей.
            /// Ожидание: никаких изменений.
            /// </summary>
            [Fact]
            public void Unbind_EmptyItemAndLocation_NoChanges()
            {
                // Arrange
                var item = new Element(ElemType.Item, @params: new Dictionary<string, object> {
                    { "Location", null } });
                var location = new Element(ElemType.Location, @params: new Dictionary<string, object> {
                    { "Items", new List<IElement>() } });
                // Act
                Binder.Unbind(item, location);
                // Assert
                Assert.True(item.Params.ContainsKey("Location"));
                Assert.True(location.Params.ContainsKey("Items"));
                Assert.True(location.Params["Items"] is List<IElement>);
                Assert.Null((IElement)item.Params["Location"]);
                Assert.Empty((List<IElement>)location.Params["Items"]);
            }

            /// <summary>
            /// Переприязка предмета к другой локации.
            /// Ожидание: разрыв существующей связи и успешное связывание с новой локацией.
            /// </summary>
            [Fact]
            public void Rebind_ItemToAnotherLocation_SuccessfulRebinding()
            {
                // Arrange
                Bind_ItemAndLocation_SuccessfulBinding();
                Unbind_ItemAndLocation_SuccessfulUnbinding();
                var location1 = new Element(ElemType.Location);
                var location2 = new Element(ElemType.Location);
                var item = new Element(ElemType.Item);
                Binder.Bind(item, location1);
                // Act
                Binder.Bind(item, location2);
                // Assert
                Assert.True(item.Params.ContainsKey("Location"));
                Assert.True(location1.Params.ContainsKey("Items"));
                Assert.True(location2.Params.ContainsKey("Items"));
                Assert.True(item.Params["Location"] is IElement);
                Assert.True(location1.Params["Items"] is List<IElement>);
                Assert.True(location2.Params["Items"] is List<IElement>);
                Assert.NotNull((IElement)item.Params["Location"]);
                Assert.Empty((List<IElement>)location1.Params["Items"]);
                Assert.Single((List<IElement>)location2.Params["Items"]);
                Assert.Equal(location2, (IElement)item.Params["Location"]);
                Assert.Equal(item, ((List<IElement>)location2.Params["Items"])[0]);
            }
        }

        /// <summary>
        /// Тесты для класса Binder. Связывание и разрыв связей предметов и событий.
        /// </summary>
        public class BinderItemEventTests
        {
            /// <summary>
            /// Связывание предмета и события.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_ItemAndEvent_SuccessfulBinding()
            {
                // Arrange
                var item = new Element(ElemType.Item);
                var @event = new Element(ElemType.Event);
                // Act
                Binder.Bind(item, @event);
                // Assert
                Assert.True(item.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Items"));
                Assert.True(item.Params["Events"] is List<IElement>);
                Assert.True(@event.Params["Items"] is List<IElement>);
                Assert.Single((List<IElement>)item.Params["Events"]);
                Assert.Single((List<IElement>)@event.Params["Items"]);
                Assert.Equal(@event, ((List<IElement>)item.Params["Events"])[0]);
                Assert.Equal(item, ((List<IElement>)@event.Params["Items"])[0]);
            }

            /// <summary>
            /// Связывание предмета и события с ненулевым значением отношения.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_ItemAndEventWithNonZeroRelation_SuccessfulBinding()
            {
                // Arrange
                var item = new Element(ElemType.Item);
                var @event = new Element(ElemType.Event);
                // Act
                Binder.Bind(item, @event, 1);
                // Assert
                Assert.True(item.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Items"));
                Assert.True(item.Params["Events"] is List<IElement>);
                Assert.True(@event.Params["Items"] is List<IElement>);
                Assert.Single((List<IElement>)item.Params["Events"]);
                Assert.Single((List<IElement>)@event.Params["Items"]);
                Assert.Equal(@event, ((List<IElement>)item.Params["Events"])[0]);
                Assert.Equal(item, ((List<IElement>)@event.Params["Items"])[0]);
            }

            /// <summary>
            /// Связывание предмета и события, когда предмет уже содержит событие.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_ItemWithEvent_SuccessfulBinding()
            {
                // Arrange
                var @event = new Element(ElemType.Event);
                var item = new Element(ElemType.Item, @params: new Dictionary<string, object> {
                    { "Events", new List<IElement> { @event } } });
                // Act
                Binder.Bind(item, @event);
                // Assert
                Assert.True(item.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Items"));
                Assert.True(item.Params["Events"] is List<IElement>);
                Assert.True(@event.Params["Items"] is List<IElement>);
                Assert.Single((List<IElement>)item.Params["Events"]);
                Assert.Single((List<IElement>)@event.Params["Items"]);
                Assert.Equal(@event, ((List<IElement>)item.Params["Events"])[0]);
                Assert.Equal(item, ((List<IElement>)@event.Params["Items"])[0]);
            }

            /// <summary>
            /// Связывание предмета и события, когда событие уже содержит предмет.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_EventWithItem_SuccessfulBinding()
            {
                // Arrange
                var item = new Element(ElemType.Item);
                var @event = new Element(ElemType.Event, @params: new Dictionary<string, object> {
                    { "Items", new List<IElement> { item } } });
                // Act
                Binder.Bind(item, @event);
                // Assert
                Assert.True(item.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Items"));
                Assert.True(item.Params["Events"] is List<IElement>);
                Assert.True(@event.Params["Items"] is List<IElement>);
                Assert.Single((List<IElement>)item.Params["Events"]);
                Assert.Single((List<IElement>)@event.Params["Items"]);
                Assert.Equal(@event, ((List<IElement>)item.Params["Events"])[0]);
                Assert.Equal(item, ((List<IElement>)@event.Params["Items"])[0]);
            }

            /// <summary>
            /// Связывание предмета и события, которые уже связаны.
            /// Ожидание: никаких изменений.
            /// </summary>
            [Fact]
            public void Bind_BoundItemAndEvent_NoChanges()
            {
                // Arrange
                Bind_ItemAndEvent_SuccessfulBinding();
                var item = new Element(ElemType.Item);
                var @event = new Element(ElemType.Event);
                Binder.Bind(item, @event);
                // Act
                Binder.Bind(item, @event);
                // Assert
                Assert.True(item.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Items"));
                Assert.True(item.Params["Events"] is List<IElement>);
                Assert.True(@event.Params["Items"] is List<IElement>);
                Assert.Single((List<IElement>)item.Params["Events"]);
                Assert.Single((List<IElement>)@event.Params["Items"]);
                Assert.Equal(@event, ((List<IElement>)item.Params["Events"])[0]);
                Assert.Equal(item, ((List<IElement>)@event.Params["Items"])[0]);
            }

            /// <summary>
            /// Разрыв связи между предметом и событием.
            /// Ожидание: успешное разрывание связи.
            /// </summary>
            [Fact]
            public void Unbind_ItemAndEvent_SuccessfulUnbinding()
            {
                // Arrange
                Bind_ItemAndEvent_SuccessfulBinding();
                var item = new Element(ElemType.Item);
                var @event = new Element(ElemType.Event);
                Binder.Bind(item, @event);
                // Act
                Binder.Unbind(item, @event);
                // Assert
                Assert.True(item.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Items"));
                Assert.True(item.Params["Events"] is List<IElement>);
                Assert.True(@event.Params["Items"] is List<IElement>);
                Assert.Empty((List<IElement>)item.Params["Events"]);
                Assert.Empty((List<IElement>)@event.Params["Items"]);
            }

            /// <summary>
            /// Разрыв связи между предметом и событием, когда предмет не содержит события.
            /// Ожидание: разрыв существующей связи.
            /// </summary>
            [Fact]
            public void Unbind_ItemWithoutEvent_SuccessfulUnbinding()
            {
                // Arrange
                var item = new Element(ElemType.Item, @params: new Dictionary<string, object> {
                    { "Events", new List<IElement>() } });
                var @event = new Element(ElemType.Event, @params: new Dictionary<string, object> {
                    { "Items", new List<IElement> { item } } });
                // Act
                Binder.Unbind(item, @event);
                // Assert
                Assert.True(item.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Items"));
                Assert.True(item.Params["Events"] is List<IElement>);
                Assert.True(@event.Params["Items"] is List<IElement>);
                Assert.Empty((List<IElement>)item.Params["Events"]);
                Assert.Empty((List<IElement>)@event.Params["Items"]);
            }

            /// <summary>
            /// Разрыв связи между предметом и событием, когда событие не содержит предмета.
            /// Ожидание: разрыв существующей связи.
            /// </summary>
            [Fact]
            public void Unbind_EventWithoutItem_SuccessfulUnbinding()
            {
                // Arrange
                var @event = new Element(ElemType.Event, @params: new Dictionary<string, object> {
                    { "Items", new List<IElement>() } });
                var item = new Element(ElemType.Item, @params: new Dictionary<string, object> {
                    { "Events", new List<IElement> { @event } } });
                // Act
                Binder.Unbind(item, @event);
                // Assert
                Assert.True(item.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Items"));
                Assert.True(item.Params["Events"] is List<IElement>);
                Assert.True(@event.Params["Items"] is List<IElement>);
                Assert.Empty((List<IElement>)item.Params["Events"]);
                Assert.Empty((List<IElement>)@event.Params["Items"]);
            }

            /// <summary>
            /// Разрыв связи между предметом и событием, когда предмет и событие не связаны.
            /// Ожидание: никаких изменений.
            /// </summary>
            [Fact]
            public void Unbind_UnboundItemAndEvent_NoChanges()
            {
                // Arrange
                var item = new Element(ElemType.Item);
                var @event = new Element(ElemType.Event);
                // Act
                Binder.Unbind(item, @event);
                // Assert
                Assert.False(item.Params.ContainsKey("Events"));
                Assert.False(@event.Params.ContainsKey("Items"));
            }

            /// <summary>
            /// Разрыв связи между предметом и событием, которые имеют пустые параметры для хранения связей.
            /// Ожидание: никаких изменений.
            /// </summary>
            [Fact]
            public void Unbind_EmptyItemAndEvent_NoChanges()
            {
                // Arrange
                var item = new Element(ElemType.Item, @params: new Dictionary<string, object> {
                    { "Events", new List<IElement>() } });
                var @event = new Element(ElemType.Event, @params: new Dictionary<string, object> {
                    { "Items", new List<IElement>() } });
                // Act
                Binder.Unbind(item, @event);
                // Assert
                Assert.True(item.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Items"));
                Assert.True(item.Params["Events"] is List<IElement>);
                Assert.True(@event.Params["Items"] is List<IElement>);
                Assert.Empty((List<IElement>)item.Params["Events"]);
                Assert.Empty((List<IElement>)@event.Params["Items"]);
            }
        }

        /// <summary>
        /// Тесты для класса Binder. Связывание и разрыв связей локаций и событий.
        /// </summary>
        public class BinderLocEventTests
        {
            /// <summary>
            /// Связывание локации и события.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_LocationAndEvent_SuccessfulBinding()
            {
                // Arrange
                var location = new Element(ElemType.Location);
                var @event = new Element(ElemType.Event);
                // Act
                Binder.Bind(location, @event);
                // Assert
                Assert.True(location.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Locations"));
                Assert.True(location.Params["Events"] is List<IElement>);
                Assert.True(@event.Params["Locations"] is List<IElement>);
                Assert.Single((List<IElement>)location.Params["Events"]);
                Assert.Single((List<IElement>)@event.Params["Locations"]);
                Assert.Equal(@event, ((List<IElement>)location.Params["Events"])[0]);
                Assert.Equal(location, ((List<IElement>)@event.Params["Locations"])[0]);
            }

            /// <summary>
            /// Связывание локации и события с ненулевым значением отношения.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_LocationAndEventWithNonZeroRelation_SuccessfulBinding()
            {
                // Arrange
                var location = new Element(ElemType.Location);
                var @event = new Element(ElemType.Event);
                // Act
                Binder.Bind(location, @event, 1);
                // Assert
                Assert.True(location.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Locations"));
                Assert.True(location.Params["Events"] is List<IElement>);
                Assert.True(@event.Params["Locations"] is List<IElement>);
                Assert.Single((List<IElement>)location.Params["Events"]);
                Assert.Single((List<IElement>)@event.Params["Locations"]);
                Assert.Equal(@event, ((List<IElement>)location.Params["Events"])[0]);
                Assert.Equal(location, ((List<IElement>)@event.Params["Locations"])[0]);
            }

            /// <summary>
            /// Связывание локации и события, когда локация уже содержит событие.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_LocationWithEvent_SuccessfulBinding()
            {
                // Arrange
                var @event = new Element(ElemType.Event);
                var location = new Element(ElemType.Location, @params: new Dictionary<string, object> {
                    { "Events", new List<IElement> { @event } } });
                // Act
                Binder.Bind(location, @event);
                // Assert
                Assert.True(location.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Locations"));
                Assert.True(location.Params["Events"] is List<IElement>);
                Assert.True(@event.Params["Locations"] is List<IElement>);
                Assert.Single((List<IElement>)location.Params["Events"]);
                Assert.Single((List<IElement>)@event.Params["Locations"]);
                Assert.Equal(@event, ((List<IElement>)location.Params["Events"])[0]);
                Assert.Equal(location, ((List<IElement>)@event.Params["Locations"])[0]);
            }

            /// <summary>
            /// Связывание локации и события, когда событие уже содержит локацию.
            /// Ожидание: успешное связывание.
            /// </summary>
            [Fact]
            public void Bind_EventWithLocation_SuccessfulBinding()
            {
                // Arrange
                var location = new Element(ElemType.Location);
                var @event = new Element(ElemType.Event, @params: new Dictionary<string, object> {
                    { "Locations", new List<IElement> { location } } });
                // Act
                Binder.Bind(location, @event);
                // Assert
                Assert.True(location.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Locations"));
                Assert.True(location.Params["Events"] is List<IElement>);
                Assert.True(@event.Params["Locations"] is List<IElement>);
                Assert.Single((List<IElement>)location.Params["Events"]);
                Assert.Single((List<IElement>)@event.Params["Locations"]);
                Assert.Equal(@event, ((List<IElement>)location.Params["Events"])[0]);
                Assert.Equal(location, ((List<IElement>)@event.Params["Locations"])[0]);
            }

            /// <summary>
            /// Связывание локации и события, которые уже связаны.
            /// Ожидание: никаких изменений.
            /// </summary>
            [Fact]
            public void Bind_BoundLocationAndEvent_NoChanges()
            {
                // Arrange
                Bind_LocationAndEvent_SuccessfulBinding();
                var location = new Element(ElemType.Location);
                var @event = new Element(ElemType.Event);
                Binder.Bind(location, @event);
                // Act
                Binder.Bind(location, @event);
                // Assert
                Assert.True(location.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Locations"));
                Assert.True(location.Params["Events"] is List<IElement>);
                Assert.True(@event.Params["Locations"] is List<IElement>);
                Assert.Single((List<IElement>)location.Params["Events"]);
                Assert.Single((List<IElement>)@event.Params["Locations"]);
                Assert.Equal(@event, ((List<IElement>)location.Params["Events"])[0]);
                Assert.Equal(location, ((List<IElement>)@event.Params["Locations"])[0]);
            }

            /// <summary>
            /// Разрыв связи между локацией и событием.
            /// Ожидание: успешное разрывание связи.
            /// </summary>
            [Fact]
            public void Unbind_LocationAndEvent_SuccessfulUnbinding()
            {
                // Arrange
                Bind_LocationAndEvent_SuccessfulBinding();
                var location = new Element(ElemType.Location);
                var @event = new Element(ElemType.Event);
                Binder.Bind(location, @event);
                // Act
                Binder.Unbind(location, @event);
                // Assert
                Assert.True(location.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Locations"));
                Assert.Empty((List<IElement>)location.Params["Events"]);
                Assert.Empty((List<IElement>)@event.Params["Locations"]);
            }

            /// <summary>
            /// Разрыв связи между локацией и событием, когда локация не содержит события.
            /// Ожидание: разрыв существующей связи.
            /// </summary>
            [Fact]
            public void Unbind_LocationWithoutEvent_SuccessfulUnbinding()
            {
                // Arrange
                var location = new Element(ElemType.Location, @params: new Dictionary<string, object> {
                    { "Events", new List<IElement>() } });
                var @event = new Element(ElemType.Event, @params: new Dictionary<string, object> {
                    { "Locations", new List<IElement> { location } } });
                // Act
                Binder.Unbind(location, @event);
                // Assert
                Assert.True(location.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Locations"));
                Assert.Empty((List<IElement>)location.Params["Events"]);
                Assert.Empty((List<IElement>)@event.Params["Locations"]);
            }

            /// <summary>
            /// Разрыв связи между локацией и событием, когда событие не содержит локацию.
            /// Ожидание: разрыв существующей связи.
            /// </summary>
            [Fact]
            public void Unbind_EventWithoutLocation_SuccessfulUnbinding()
            {
                // Arrange
                var @event = new Element(ElemType.Event, @params: new Dictionary<string, object> {
                    { "Locations", new List<IElement>() } });
                var location = new Element(ElemType.Location, @params: new Dictionary<string, object> {
                    { "Events", new List<IElement> { @event } } });
                // Act
                Binder.Unbind(location, @event);
                // Assert
                Assert.True(location.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Locations"));
                Assert.Empty((List<IElement>)location.Params["Events"]);
                Assert.Empty((List<IElement>)@event.Params["Locations"]);
            }

            /// <summary>
            /// Разрыв связи между локацией и событием, когда локация и событие не связаны.
            /// Ожидание: никаких изменений.
            /// </summary>
            [Fact]
            public void Unbind_UnboundLocationAndEvent_NoChanges()
            {
                // Arrange
                var location = new Element(ElemType.Location);
                var @event = new Element(ElemType.Event);
                // Act
                Binder.Unbind(location, @event);
                // Assert
                Assert.False(location.Params.ContainsKey("Events"));
                Assert.False(@event.Params.ContainsKey("Locations"));
            }

            /// <summary>
            /// Разрыв связи между локацией и событием, которые имеют пустые параметры для хранения связей.
            /// Ожидание: никаких изменений.
            /// </summary>
            [Fact]
            public void Unbind_EmptyLocationAndEvent_NoChanges()
            {
                // Arrange
                var location = new Element(ElemType.Location, @params: new Dictionary<string, object> {
                    { "Events", new List<IElement>() } });
                var @event = new Element(ElemType.Event, @params: new Dictionary<string, object> {
                    { "Locations", new List<IElement>() } });
                // Act
                Binder.Unbind(location, @event);
                // Assert
                Assert.True(location.Params.ContainsKey("Events"));
                Assert.True(@event.Params.ContainsKey("Locations"));
                Assert.Empty((List<IElement>)location.Params["Events"]);
                Assert.Empty((List<IElement>)@event.Params["Locations"]);
            }
        }
    }
}
