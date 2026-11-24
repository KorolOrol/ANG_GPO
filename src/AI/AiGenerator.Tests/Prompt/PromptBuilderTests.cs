using System.Text.Json;
using AIGenerator.Prompt;

namespace AiGenerator.Tests.Prompt;

/// <summary>
/// Тесты для класса PromptBuilder.
/// </summary>
public class PromptBuilderTests
{
    /// <summary>
    /// Проверяет, что конструктор с шаблонами сохраняет значения и работает без учета регистра.
    /// </summary>
    [Fact]
    public void Constructor_WithTemplates_LoadsCaseInsensitive()
    {
        var templates = new Dictionary<string, string>
        {
            ["Setting"] = "System prompt"
        };

        var builder = new PromptBuilder(templates);

        Assert.True(builder.TryGetTemplate("setting", out var template));
        Assert.Equal("System prompt", template);
    }

    /// <summary>
    /// Проверяет, что конструктор с null-словарем выбрасывает ArgumentNullException.
    /// </summary>
    [Fact]
    public void Constructor_WithNullTemplates_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new PromptBuilder((Dictionary<string, string>)null!));
    }

    /// <summary>
    /// Проверяет, что AddTemplate сохраняет шаблон и перезаписывает существующий.
    /// </summary>
    [Fact]
    public void AddTemplate_StoresAndOverwritesTemplate()
    {
        var builder = new PromptBuilder();

        builder.AddTemplate("Plot", "Plot: {0}");
        builder.AddTemplate("plot", "Plot v2: {0}");

        Assert.True(builder.TryGetTemplate("PLOT", out var template));
        Assert.Equal("Plot v2: {0}", template);
    }

    /// <summary>
    /// Проверяет, что AddTemplate выбрасывает исключения на неверных аргументах.
    /// </summary>
    [Fact]
    public void AddTemplate_InvalidArguments_Throws()
    {
        var builder = new PromptBuilder();

        Assert.Throws<ArgumentException>(() => builder.AddTemplate("  ", "text"));
        Assert.Throws<ArgumentNullException>(() => builder.AddTemplate("Key", null!));
    }

    /// <summary>
    /// Проверяет, что TryGetTemplate возвращает false для пустого ключа.
    /// </summary>
    [Fact]
    public void TryGetTemplate_WhitespaceKey_ReturnsFalse()
    {
        var builder = new PromptBuilder();

        var result = builder.TryGetTemplate("   ", out var template);

        Assert.False(result);
        Assert.Equal(string.Empty, template);
    }

    /// <summary>
    /// Проверяет, что LoadTemplates загружает шаблоны из плоского JSON.
    /// </summary>
    [Fact]
    public void LoadTemplates_FlatJson_LoadsTemplates()
    {
        var path = NewTempFilePath();
        try
        {
            File.WriteAllText(path, JsonSerializer.Serialize(new Dictionary<string, string>
            {
                ["Setting"] = "System prompt",
                ["Plot"] = "Plot: {0}"
            }));

            var builder = new PromptBuilder();
            builder.LoadTemplates(path);

            Assert.True(builder.TryGetTemplate("setting", out var template));
            Assert.Equal("System prompt", template);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    /// <summary>
    /// Проверяет, что LoadTemplates загружает шаблоны из JSON с узлом Templates.
    /// </summary>
    [Fact]
    public void LoadTemplates_FromTemplatesProperty_Loads()
    {
        var path = NewTempFilePath();
        try
        {
            var json = JsonSerializer.Serialize(new
            {
                Templates = new Dictionary<string, string>
                {
                    ["Setting"] = "System prompt"
                }
            });
            File.WriteAllText(path, json);

            var builder = new PromptBuilder(path);

            Assert.True(builder.TryGetTemplate("SETTING", out var template));
            Assert.Equal("System prompt", template);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    /// <summary>
    /// Проверяет, что LoadTemplates выбрасывает InvalidDataException при неверном формате JSON.
    /// </summary>
    [Fact]
    public void LoadTemplates_InvalidJson_ThrowsInvalidDataException()
    {
        var path = NewTempFilePath();
        try
        {
            File.WriteAllText(path, "[]");
            var builder = new PromptBuilder();

            Assert.Throws<InvalidDataException>(() => builder.LoadTemplates(path));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    /// <summary>
    /// Проверяет, что AddMessage добавляет сообщение и Build возвращает копию списка.
    /// </summary>
    [Fact]
    public void AddMessage_AddsMessage_AndBuildReturnsCopy()
    {
        var builder = new PromptBuilder();

        builder.AddMessage(PromptEntry.Context, "System");
        var snapshot = builder.Build();
        builder.AddMessage(PromptEntry.Request, "User");

        Assert.Single(snapshot);
        Assert.Equal((PromptEntry.Context, "System"), snapshot[0]);
        Assert.Equal(2, builder.Build().Count);
    }

    /// <summary>
    /// Проверяет, что AddMessage выбрасывает исключение на пустом тексте.
    /// </summary>
    [Fact]
    public void AddMessage_EmptyText_ThrowsArgumentException()
    {
        var builder = new PromptBuilder();

        Assert.Throws<ArgumentException>(() => builder.AddMessage(PromptEntry.Context, " "));
    }

    /// <summary>
    /// Проверяет, что AddMessageFromTemplate использует шаблон и форматирует аргументы.
    /// </summary>
    [Fact]
    public void AddMessageFromTemplate_FormatsText()
    {
        var builder = new PromptBuilder();
        builder.AddTemplate("Plot", "Plot: {0}");
        builder.AddTemplate("Schema", "schema");

        builder.AddMessageFromTemplate(PromptEntry.Context, "schema");
        builder.AddMessageFromTemplate(PromptEntry.Request, "plot", "data");

        var messages = builder.Build();
        Assert.Collection(messages,
            entry => Assert.Equal((PromptEntry.Context, "schema"), entry),
            entry => Assert.Equal((PromptEntry.Request, "Plot: data"), entry));
    }

    /// <summary>
    /// Проверяет, что AddMessageFromTemplate выбрасывает KeyNotFoundException для неизвестного ключа.
    /// </summary>
    [Fact]
    public void AddMessageFromTemplate_MissingTemplate_ThrowsKeyNotFoundException()
    {
        var builder = new PromptBuilder();

        Assert.Throws<KeyNotFoundException>(() =>
            builder.AddMessageFromTemplate(PromptEntry.Request, "missing"));
    }

    /// <summary>
    /// Проверяет работу DeserializeTemplateDictionary для разных форматов JSON.
    /// </summary>
    [Fact]
    public void DeserializeTemplateDictionary_HandlesTemplatesAndInvalidShape()
    {
        var direct = JsonSerializer.Serialize(new Dictionary<string, string> { ["A"] = "B" });
        var withRoot = JsonSerializer.Serialize(new { Templates = new Dictionary<string, string> { ["C"] = "D" } });

        var directResult = PromptBuilder.DeserializeTemplateDictionary(direct);
        var rootResult = PromptBuilder.DeserializeTemplateDictionary(withRoot);
        var invalidResult = PromptBuilder.DeserializeTemplateDictionary("[]");

        Assert.Equal("B", directResult!["A"]);
        Assert.Equal("D", rootResult!["C"]);
        Assert.Null(invalidResult);
    }

    private static string NewTempFilePath()
    {
        return Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
    }
}
