using System.Text.Json;
using AIGenerator.DtoProvider;
using BaseClasses.Enum;
using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Model.Params;
using BaseClasses.Services.Binds;

namespace AiGenerator.Tests.DtoProvider;

/// <summary>
/// Тесты для класса SimpleDtoProvider.
/// </summary>
public class SimpleDtoProviderTests
{
    /// <summary>
    /// Проверяет, что ToDto для сюжета включает ключи параметров/связей и сериализованные элементы.
    /// </summary>
    [Fact]
    public void ToDto_Plot_IncludesKeysAndElements()
    {
        var provider = new SimpleDtoProvider();
        var powerKey = new ParamKey<int>("Power", "Test");
        var relationKey = new ParamKey<bool>("Related", "Test");
        var hero = new MockElement(ElemType.Character, "Hero", "Main");
        hero.Params.Add(powerKey, 3);
        var sword = new MockElement(ElemType.Item, "Sword", "Steel");
        var plot = new Plot();
        plot.Add(hero);
        plot.Add(sword);
        plot.Relations.Add(new Relation(hero, sword, relationKey, true));

        var dto = provider.ToDto(plot);

        Assert.Contains("Param Keys:", dto);
        Assert.Contains("- Power: int", dto);
        Assert.Contains("Relation Keys:", dto);
        Assert.Contains("- Related: bool", dto);
        Assert.Contains("\"Name\": \"Hero\"", dto);
        Assert.Contains("\"Name\": \"Sword\"", dto);
    }

    /// <summary>
    /// Проверяет, что ToDto для элемента формирует JSON с параметрами и пустыми связями.
    /// </summary>
    [Fact]
    public void ToDto_Element_SerializesParamsAndEmptyRelations()
    {
        var provider = new SimpleDtoProvider();
        var powerKey = new ParamKey<int>("Power", "Test");
        var element = new MockElement(ElemType.Character, "Hero", "Main");
        element.Params.Add(powerKey, 5);

        var json = provider.ToDto(element);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.Equal("Character", root.GetProperty("Type").GetString());
        Assert.Equal("Hero", root.GetProperty("Name").GetString());
        Assert.Equal("Main", root.GetProperty("Description").GetString());
        Assert.Equal(5, root.GetProperty("Params").GetProperty("Power").GetInt32());
        Assert.Equal(JsonValueKind.Array, root.GetProperty("Relations").ValueKind);
        Assert.Empty(root.GetProperty("Relations").EnumerateArray());
    }

    /// <summary>
    /// Проверяет, что FromDto добавляет элемент и связывает его с существующим, используя известные ключи.
    /// </summary>
    [Fact]
    public void FromDto_AddsElementAndBindsRelation()
    {
        var provider = new SimpleDtoProvider();
        var powerKey = new ParamKey<int>("Power", "Test");
        var relationKey = new ParamKey<int>("Related", "Test");
        SeedKeys(provider, powerKey, relationKey);

        var bindCalls = new List<(IElement Source, IElement Target, IParamKey Key, object? Value)>();
        var binder = new Binder();
        binder.Register(ElemType.Character, ElemType.Item, relationKey,
            (source, target, key, value, plot) =>
            {
                bindCalls.Add((source, target, key, value));
                plot.Relations.Add(new Relation(source, target, key, value));
            },
            (_, _, _, _, _) => { });

        var plot = new Plot(binder);
        var sword = new Element(ElemType.Item, "Sword", "Steel");
        plot.Add(sword);

        var dto = BuildDtoJson(
            type: "Character",
            name: "Hero",
            description: "Main",
            parameters: new Dictionary<string, object> { ["Power"] = 5 },
            relations: new List<(string Target, string Param, object Value)>
            {
                ("Sword", "Related", 7)
            });

        var element = provider.FromDto(dto, plot);

        Assert.Equal("Hero", element.Name);
        Assert.True(element.Params.TryGet(powerKey, out var power));
        Assert.Equal(5, power);
        Assert.Equal(2, plot.Elements.Count);

        Assert.Single(bindCalls);
        Assert.Same(element, bindCalls[0].Source);
        Assert.Same(sword, bindCalls[0].Target);
        Assert.Equal(relationKey, bindCalls[0].Key);
        Assert.Equal(7, bindCalls[0].Value);
        Assert.Single(plot.Relations);
    }

    /// <summary>
    /// Проверяет, что FromDto выбрасывает JsonException при некорректном JSON.
    /// </summary>
    [Fact]
    public void FromDto_InvalidJson_ThrowsJsonException()
    {
        var provider = new SimpleDtoProvider();
        var plot = new Plot();

        Assert.ThrowsAny<JsonException>(() => provider.FromDto("not-json", plot));
    }

    /// <summary>
    /// Проверяет, что FromDto выбрасывает JsonException при неизвестном типе элемента.
    /// </summary>
    [Fact]
    public void FromDto_InvalidType_ThrowsJsonException()
    {
        var provider = new SimpleDtoProvider();
        var plot = new Plot();
        var dto = BuildDtoJson(
            type: "Alien",
            name: "X",
            description: "Unknown",
            parameters: new Dictionary<string, object>(),
            relations: new List<(string Target, string Param, object Value)>());

        Assert.Throws<JsonException>(() => provider.FromDto(dto, plot));
    }

    /// <summary>
    /// Проверяет, что GetSchema возвращает JSON-схему с обязательными свойствами.
    /// </summary>
    [Fact]
    public void GetSchema_ReturnsValidJsonWithRequiredFields()
    {
        var provider = new SimpleDtoProvider();

        var schema = provider.GetSchema();

        using var doc = JsonDocument.Parse(schema);
        var properties = doc.RootElement.GetProperty("properties");
        Assert.True(properties.TryGetProperty("Type", out _));
        Assert.True(properties.TryGetProperty("Name", out _));
        Assert.True(properties.TryGetProperty("Description", out _));
        Assert.True(properties.TryGetProperty("Params", out _));
        Assert.True(properties.TryGetProperty("Relations", out _));
    }

    /// <summary>
    /// Проверяет, что GetNewElements возвращает только новые элементы с корректными типами и значениями.
    /// </summary>
    [Fact]
    public void GetNewElements_ReturnsExpectedNewElements()
    {
        var provider = new SimpleDtoProvider();
        var relationKey = new ParamKey<int>("Related", "Test");
        SeedKeys(provider, new ParamKey<int>("Power", "Test"), relationKey);

        var binder = new Binder();
        binder.Register(ElemType.Character, ElemType.Item, relationKey,
            (_, _, _, _, _) => { },
            (_, _, _, _, _) => { });
        var plot = new Plot(binder);
        plot.Add(new Element(ElemType.Item, "Existing", "Old"));

        var dto = BuildDtoJson(
            type: "Character",
            name: "Hero",
            description: "Main",
            parameters: new Dictionary<string, object>(),
            relations: new List<(string Target, string Param, object Value)>
            {
                ("Existing", "Related", 1),
                ("NewItem", "Related", 3),
                ("Ghost", "Unknown", 2)
            });

        var result = provider.GetNewElements(dto, plot);

        Assert.Single(result);
        var (element, key, value) = result[0];
        Assert.Equal(ElemType.Item, element.Type);
        Assert.Equal("NewItem", element.Name);
        Assert.Equal(relationKey, key);
        Assert.Equal(3, value);
    }

    private static string BuildDtoJson(
        string type,
        string name,
        string description,
        Dictionary<string, object> parameters,
        IEnumerable<(string Target, string Param, object Value)> relations)
    {
        var dto = new
        {
            Type = type,
            Name = name,
            Description = description,
            Params = parameters,
            Relations = relations.Select(r => new { r.Target, r.Param, r.Value }).ToList()
        };
        return JsonSerializer.Serialize(dto, SimpleDtoProvider.Options);
    }

    private static void SeedKeys(SimpleDtoProvider provider, IParamKey paramKey, IParamKey relationKey)
    {
        var plot = new Plot();
        var source = new Element(ElemType.Character, "Seed", "Seed");
        var target = new Element(ElemType.Item, "SeedTarget", "SeedTarget");
        source.Params.Add(paramKey, paramKey.ValueType == typeof(int) ? 1 : null);
        plot.Add(source);
        plot.Add(target);
        plot.Relations.Add(new Relation(source, target, relationKey, 1));
        provider.ToDto(plot);
    }

    private sealed class MockElement : IElement
    {
        public MockElement(ElemType type, string name, string description)
        {
            Type = type;
            Name = name;
            Description = description;
            Params = new ParamBag();
            Time = 0;
        }

        public ElemType Type { get; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ParamBag Params { get; }
        public int Time { get; set; }

        public bool IsEmpty() => false;
    }
}
