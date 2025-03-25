using BaseClasses.Model;
using BaseClasses.Enum;
using BaseClasses.Services;
using BaseClasses.Interface;

namespace BaseClasses.Tests.Services
{
    public class MergerTests
    {
        /// <summary>
        /// Объединение с null элементом
        /// Ожидание: ArgumentNullException
        /// </summary>
        [Fact]
        public void Merge_WithNullSource_ThrowsArgumentNullException()
        {
            // Arrange
            Element element1 = null;
            Element element2 = new Element(ElemType.Character);
            Element element3 = null;
            // Act
            Action actionNE = () => Merger.Merge(element1, element2);
            Action actionEN = () => Merger.Merge(element2, element1);
            Action actionNN = () => Merger.Merge(element1, element3);
            // Assert
            Assert.Throws<ArgumentNullException>(actionNE);
            Assert.Throws<ArgumentNullException>(actionEN);
            Assert.Throws<ArgumentNullException>(actionNN);
        }

        /// <summary>
        /// Объединение элементов разных типов
        /// Ожидание: ArgumentException
        /// </summary>
        [Fact]
        public void Merge_WithDifferentTypes_ThrowsArgumentException()
        {
            // Arrange
            Element character = new Element(ElemType.Character);
            Element item = new Element(ElemType.Item);
            Element location = new Element(ElemType.Location);
            Element @event = new Element(ElemType.Event);
            // Act
            Action actionCI = () => Merger.Merge(character, item);
            Action actionCL = () => Merger.Merge(character, location);
            Action actionCE = () => Merger.Merge(character, @event);
            Action actionIL = () => Merger.Merge(item, location);
            Action actionIE = () => Merger.Merge(item, @event);
            Action actionLE = () => Merger.Merge(location, @event);
            // Assert
            Assert.Throws<ArgumentException>(actionCI);
            Assert.Throws<ArgumentException>(actionCL);
            Assert.Throws<ArgumentException>(actionCE);
            Assert.Throws<ArgumentException>(actionIL);
            Assert.Throws<ArgumentException>(actionIE);
            Assert.Throws<ArgumentException>(actionLE);
        }

        /// <summary>
        /// Объединение пустого базового элемента с непустым
        /// Ожидание: Обновление базового элемента
        /// </summary>
        [Fact]
        public void Merge_WithEmptyBaseElement_UpdatesBaseElement()
        {
            // Arrange
            Element baseElement = new Element(ElemType.Character);
            Element mergedElement = new Element(ElemType.Character, "Name", "Description",
                new Dictionary<string, object> { { "Param1", "Value1" } }, 0);
            // Act
            Merger.Merge(baseElement, mergedElement);
            // Assert
            Assert.Equal("Name", baseElement.Name);
            Assert.Equal("Description", baseElement.Description);
            Assert.Equal(0, baseElement.Time);
            Assert.Equal("Value1", baseElement.Params["Param1"]);
        }

        /// <summary>
        /// Объединение пустых элементов
        /// Ожидание: Сохранение пустого базового элемента
        /// </summary>
        [Fact]
        public void Merge_WithEmptyBaseElementAndEmptyMergedElement_SavesEmptyBaseElement()
        {
            // Arrange
            Element baseElement = new Element(ElemType.Character);
            Element mergedElement = new Element(ElemType.Character);
            // Act
            Merger.Merge(baseElement, mergedElement);
            // Assert
            Assert.Equal("", baseElement.Name);
            Assert.Equal("", baseElement.Description);
            Assert.Equal(-1, baseElement.Time);
            Assert.Empty(baseElement.Params);
        }

        /// <summary>
        /// Объединение непустого базового элемента с пустым
        /// Ожидание: Сохранение базового элемента
        /// </summary>
        [Fact]
        public void Merge_WithEmptyMergedElement_SavesBaseElement()
        {
            // Arrange
            Element baseElement = new Element(ElemType.Character, "Name", "Description",
                new Dictionary<string, object> { { "Param1", "Value1" } }, 0);
            Element mergedElement = new Element(ElemType.Character);
            // Act
            Merger.Merge(baseElement, mergedElement);
            // Assert
            Assert.Equal("Name", baseElement.Name);
            Assert.Equal("Description", baseElement.Description);
            Assert.Equal(0, baseElement.Time);
            Assert.Equal("Value1", baseElement.Params["Param1"]);
        }

        /// <summary>
        /// Объединение элементов с одинаковыми параметрами
        /// Ожидание: Сохранение базового элемента
        /// </summary>
        [Fact]
        public void Merge_WithSameParams_SavesBaseElement()
        {
            // Arrange
            Element baseElement = new Element(ElemType.Character, "Name", "Description",
                new Dictionary<string, object> { { "Param1", "Value1" } }, 0);
            Element mergedElement = new Element(ElemType.Character, "Name2", "Description2",
                new Dictionary<string, object> { { "Param1", "Value2" } }, 0);
            // Act
            Merger.Merge(baseElement, mergedElement);
            // Assert
            Assert.Equal("Name", baseElement.Name);
            Assert.Equal("Description", baseElement.Description);
            Assert.Equal(0, baseElement.Time);
            Assert.Equal("Value1", baseElement.Params["Param1"]);
        }

        /// <summary>
        /// Объединение элементов с разными параметрами
        /// Ожидание: Добавление отличных параметров в базовый элемента
        /// </summary>
        [Fact]
        public void Merge_WithDifferentParams_AddsNewParamsToBaseElement()
        {
            // Arrange
            Element baseElement = new Element(ElemType.Character, "Name", "Description",
                new Dictionary<string, object> { { "Param1", "Value1" } }, 0);
            Element mergedElement = new Element(ElemType.Character, "Name2", "Description2",
                new Dictionary<string, object> { { "Param2", "Value2" } }, 0);
            // Act
            Merger.Merge(baseElement, mergedElement);
            // Assert
            Assert.Equal("Name", baseElement.Name);
            Assert.Equal("Description", baseElement.Description);
            Assert.Equal(0, baseElement.Time);
            Assert.Equal("Value1", baseElement.Params["Param1"]);
            Assert.Equal("Value2", baseElement.Params["Param2"]);
        }

        /// <summary>
        /// Объединение элементов без приоритета базового элемента
        /// Ожидаение: Обновление всех параметров базового элемента
        /// </summary>
        [Fact]
        public void Merge_WithoutBasePriority_UpdatesAllParams()
        {
            // Arrange
            Element baseElement = new Element(ElemType.Character, "Name", "Description",
                new Dictionary<string, object> { { "Param1", "Value1" } }, 0);
            Element mergedElement = new Element(ElemType.Character, "Name2", "Description2",
                new Dictionary<string, object> { { "Param1", "Value2" } }, 0);
            // Act
            Merger.Merge(baseElement, mergedElement, false);
            // Assert
            Assert.Equal("Name2", baseElement.Name);
            Assert.Equal("Description2", baseElement.Description);
            Assert.Equal(0, baseElement.Time);
            Assert.Equal("Value2", baseElement.Params["Param1"]);
        }

        /// <summary>
        /// Объединение элементов с вложенными элементами
        /// Ожидание: Привязка вложенных элементов к базовому элементу
        /// </summary>
        [Fact]
        public void Merge_WithNestedElements_BindsElementsToBaseElement()
        {
            // Arrange
            Element baseElement = new Element(ElemType.Character, "Name", "Description",
                new Dictionary<string, object> { { "Param1", "Value1" } }, 0);
            Element mergedElement = new Element(ElemType.Character, "Name2", "Description2",
                new Dictionary<string, object> { { "Param1", "Value2" } }, 0);
            Element nestedCharacter = new Element(ElemType.Character, "Nested", "Nested description",
                new Dictionary<string, object> { { "Param1", "Nested value" } }, 0);
            Element nestedItem = new Element(ElemType.Item, "Nested item", "Nested item description",
                new Dictionary<string, object> { { "Param1", "Nested item value" } }, 0);
            Element nestedLocation = new Element(ElemType.Location, "Nested location", "Nested location description",
                new Dictionary<string, object> { { "Param1", "Nested location value" } }, 0);
            Element nestedEvent = new Element(ElemType.Event, "Nested event", "Nested event description",
                new Dictionary<string, object> { { "Param1", "Nested event value" } }, 0);
            Binder.Bind(mergedElement, nestedCharacter, 10);
            Binder.Bind(mergedElement, nestedItem);
            Binder.Bind(mergedElement, nestedLocation);
            Binder.Bind(mergedElement, nestedEvent);
            // Act
            Merger.Merge(baseElement, mergedElement);
            // Assert
            Assert.Equal("Name", baseElement.Name);
            Assert.Equal("Description", baseElement.Description);
            Assert.Equal(0, baseElement.Time);
            Assert.Equal("Value1", baseElement.Params["Param1"]);
            Assert.Equal(nestedCharacter, ((List<Relation>)baseElement.Params["Relations"]).First().Character);
            Assert.Equal(10, ((List<Relation>)baseElement.Params["Relations"]).First().Value);
            Assert.Contains(nestedItem, ((List<IElement>)baseElement.Params["Items"]));
            Assert.Contains(nestedLocation, ((List<IElement>)baseElement.Params["Locations"]));
            Assert.Contains(nestedEvent, ((List<IElement>)baseElement.Params["Events"]));
        }

        /// <summary>
        /// Объединение элементов со списком в параметрах
        /// Ожидание: Добавление значений в список параметра
        /// </summary>
        [Fact]
        public void Merge_WithListInParams_AddsValuesToList()
        {
            // Arrange
            Element baseElement = new Element(ElemType.Character, "Name", "Description",
                new Dictionary<string, object> { { "Param1", new List<string> { "Value1" } } }, 0);
            Element mergedElement = new Element(ElemType.Character, "Name2", "Description2",
                new Dictionary<string, object> { { "Param1", new List<string> { "Value2" } } }, 0);
            // Act
            Merger.Merge(baseElement, mergedElement);
            // Assert
            Assert.Equal("Name", baseElement.Name);
            Assert.Equal("Description", baseElement.Description);
            Assert.Equal(0, baseElement.Time);
            Assert.Contains("Value1", (List<string>)baseElement.Params["Param1"]);
            Assert.Contains("Value2", (List<string>)baseElement.Params["Param1"]);
        }

        /// <summary>
        /// Объединение элементов с одним элементом в параметрах
        /// Ожидание: Привязка элемента к базовому элементу
        /// </summary>
        [Fact]
        public void Merge_WithElementInParams_BindsElementToBaseElement()
        {
            // Arrange
            Element baseElement = new Element(ElemType.Item, "Name", "Description");
            Element mergedElement = new Element(ElemType.Item, "Name2", "Description2");
            Element host = new Element(ElemType.Character, "Host", "Host description");
            Binder.Bind(mergedElement, host);
            // Act
            Merger.Merge(baseElement, mergedElement);
            // Assert
            Assert.Equal("Name", baseElement.Name);
            Assert.Equal("Description", baseElement.Description);
            Assert.Equal(host, (Element)baseElement.Params["Host"]);
        }
    }
}
