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
        /// Тесты для класса Binder. Связывание и разрыв связей персонажей с предметами.
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
    }
}
