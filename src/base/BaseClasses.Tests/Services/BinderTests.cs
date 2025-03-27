using BaseClasses.Model;
using BaseClasses.Enum;
using BaseClasses.Services;
using BaseClasses.Interface;

namespace BaseClasses.Tests.Services
{
    public class BinderTests
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
}
