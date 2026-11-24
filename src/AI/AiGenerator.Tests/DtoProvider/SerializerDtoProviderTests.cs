using System.Text.Json;
using AIGenerator.DtoProvider;
using BaseClasses.Enum;
using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Model.Params;
using BaseClasses.Services;

namespace AiGenerator.Tests.DtoProvider;

/// <summary>
/// Тесты для класса SerializerDtoProvider.
/// </summary>
public class SerializerDtoProviderTests
{
    /// <summary>
    /// Проверяет, что ToDto для сюжета использует сериализатор.
    /// </summary>
    [Fact]
    public void ToDto_Plot_ReturnsSerializerJson()
    {
        var plot = new Plot();
        var character = new Element(ElemType.Character, "Hero", "Main");
        plot.Add(character);
        plot.Time = 5;
        var provider = new SerializerDtoProvider();

        var actual = provider.ToDto(plot);

        Assert.Equal(Serializer.SerializeToString(plot), actual);
        using var doc = JsonDocument.Parse(actual);
        Assert.Equal(5, doc.RootElement.GetProperty("Time").GetInt32());
    }

    /// <summary>
    /// Проверяет, что ToDto для элемента использует сериализатор.
    /// </summary>
    [Fact]
    public void ToDto_Element_ReturnsSerializerJson()
    {
        var element = new Element(ElemType.Item, "Sword", "Steel", time: 2);
        var provider = new SerializerDtoProvider();

        var actual = provider.ToDto(element);

        Assert.Equal(Serializer.SerializeToString(element), actual);
        using var doc = JsonDocument.Parse(actual);
        Assert.Equal("Sword", doc.RootElement.GetProperty("Name").GetString());
    }

    /// <summary>
    /// Проверяет, что ToDto выбрасывает исключение для неподдерживаемой реализации IElement.
    /// </summary>
    [Fact]
    public void ToDto_Element_WhenNotConcreteElement_ThrowsArgumentException()
    {
        var provider = new SerializerDtoProvider();
        var mockElement = new MockElement();

        Assert.Throws<ArgumentException>(() => provider.ToDto(mockElement));
    }

    /// <summary>
    /// Проверяет, что FromDto добавляет новый элемент и связывает его с существующим.
    /// </summary>
    [Fact]
    public void FromDto_AddsElementAndBindsRelations()
    {
        var provider = new SerializerDtoProvider();
        var plot = new Plot();
        var existing = new Element(ElemType.Location, "Castle", "Stone");
        plot.Add(existing);

        var newElement = new Element(ElemType.Character, "Hero", "Main", time: 10);
        var relationKey = new ParamKey<int>("Related", "Test");
        var relation = new Relation(newElement, existing, relationKey, 7);
        var dto = BuildDto(newElement, relation);

        var actual = provider.FromDto(dto, plot);

        Assert.Equal(ElemType.Character, actual.Type);
        Assert.Equal("Hero", actual.Name);
        Assert.Equal("Main", actual.Description);
        Assert.Equal(10, actual.Time);
        Assert.Equal(2, plot.Elements.Count);
        Assert.Contains(actual, plot.Elements);

        Assert.Single(plot.Relations);
        var bound = plot.Relations.First();
        Assert.Same(actual, bound.Source);
        Assert.Same(existing, bound.Target);
        Assert.Equal("Related", bound.Param.Name);
        Assert.Equal("Test", bound.Param.Namespace);
        Assert.Equal(typeof(int), bound.Param.ValueType);
        Assert.Equal(7, bound.Value);
    }

    /// <summary>
    /// Проверяет, что FromDto выбрасывает JsonException для некорректного JSON.
    /// </summary>
    [Fact]
    public void FromDto_InvalidJson_ThrowsJsonException()
    {
        var provider = new SerializerDtoProvider();
        var plot = new Plot();

        Assert.ThrowsAny<JsonException>(() => provider.FromDto("not-json", plot));
    }

    /// <summary>
    /// Проверяет, что GetSchema возвращает JSON-схему с ожидаемыми свойствами.
    /// </summary>
    [Fact]
    public void GetSchema_ReturnsValidJsonWithRequiredFields()
    {
        var provider = new SerializerDtoProvider();

        var schema = provider.GetSchema();

        using var doc = JsonDocument.Parse(schema);
        var root = doc.RootElement;
        Assert.Equal("object", root.GetProperty("type").GetString());
        var properties = root.GetProperty("properties");
        Assert.True(properties.TryGetProperty("NewElement", out _));
        Assert.True(properties.TryGetProperty("NewRelations", out _));
    }

    /// <summary>
    /// Проверяет, что GetNewElements возвращает пустой список.
    /// </summary>
    [Fact]
    public void GetNewElements_ReturnsEmptyList()
    {
        var provider = new SerializerDtoProvider();
        var plot = new Plot();

        var result = provider.GetNewElements("{}", plot);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    private static string BuildDto(Element newElement, params Relation[] relations)
    {
        using var elementDoc = JsonDocument.Parse(Serializer.SerializeToString(newElement));
        var elementJson = elementDoc.RootElement.Clone();

        var relationJsons = new List<JsonElement>();
        foreach (var relation in relations)
        {
            using var relationDoc = JsonDocument.Parse(Serializer.SerializeToString(relation));
            relationJsons.Add(relationDoc.RootElement.Clone());
        }

        return JsonSerializer.Serialize(new
        {
            NewElement = elementJson,
            NewRelations = relationJsons
        });
    }

    private sealed class MockElement : IElement
    {
        public MockElement()
        {
            Type = ElemType.Event;
            Name = "Mock";
            Description = "Mock element";
            Params = new ParamBag();
            Time = 0;
        }

        public ElemType Type { get; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ParamBag Params { get; }
        public int Time { get; set; }

        public bool IsEmpty()
        {
            return false;
        }
    }
}
