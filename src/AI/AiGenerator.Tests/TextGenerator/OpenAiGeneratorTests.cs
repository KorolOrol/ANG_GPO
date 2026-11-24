using System.Reflection;
using AIGenerator.Prompt;
using AIGenerator.TextGenerator;
using Microsoft.Extensions.AI;

namespace AiGenerator.Tests.TextGenerator;

/// <summary>
/// Тесты для класса OpenAiGenerator.
/// </summary>
public class OpenAiGeneratorTests
{
    private const string SchemaJson = @"{
  ""$schema"": ""https://json-schema.org/draft/2020-12/schema"",
  ""type"": ""object"",
  ""additionalProperties"": false,
  ""properties"": {
    ""ok"": { ""type"": ""boolean"" }
  },
  ""required"": [""ok""]
}";

    [Fact]
    public async Task GenerateTextAsync_WithStructuredOutput_UsesSchemaAndOptions()
    {
        var client = new FakeChatClient();
        client.EnqueueResponse("{\"ok\":true}\n\n");
        var generator = CreateGenerator(client, useStructuredOutput: true);
        generator.MaxCompletionTokens = 256;
        generator.Temperature = 0.2f;
        generator.TopP = 0.7f;
        generator.TopK = 42;
        generator.FrequencyPenalty = 0.1f;
        generator.PresencePenalty = 0.2f;
        generator.RepeatPenalty = 1.3f;
        generator.Seed = 777;
        generator.Stop = new List<string> { "STOP" };

        var result = await generator.GenerateTextAsync(new List<(PromptEntry, string)>
        {
            (PromptEntry.Schema, SchemaJson),
            (PromptEntry.Context, "System"),
            (PromptEntry.Request, "User")
        });

        Assert.Equal("{\"ok\":true}", result);
        Assert.Single(client.Messages);
        Assert.Equal(2, client.Messages[0].Count);
        Assert.Equal(ChatRole.System, client.Messages[0][0].Role);
        Assert.Equal(ChatRole.User, client.Messages[0][1].Role);

        var options = client.Options.Single();
        Assert.IsType<ChatResponseFormatJson>(options.ResponseFormat);
        Assert.Equal(256, options.MaxOutputTokens);
        Assert.Equal(0.2f, options.Temperature);
        Assert.Equal(0.7f, options.TopP);
        Assert.Equal(0.1f, options.FrequencyPenalty);
        Assert.Equal(0.2f, options.PresencePenalty);
        Assert.Equal(1.3f, options.AdditionalProperties["repetition_penalty"]);
        Assert.Equal(42, options.AdditionalProperties["top_k"]);
        Assert.Equal(777, options.AdditionalProperties["seed"]);
        Assert.Equal(new[] { "STOP" }, options.StopSequences);
    }

    [Fact]
    public async Task GenerateTextAsync_WhenStructuredOutputDisabled_ExtractsFirstJson()
    {
        var client = new FakeChatClient();
        client.EnqueueResponse("prefix {\"ok\":true}\n\n suffix");
        var generator = CreateGenerator(client, useStructuredOutput: false);

        var result = await generator.GenerateTextAsync(new List<(PromptEntry, string)>
        {
            (PromptEntry.Schema, SchemaJson),
            (PromptEntry.Context, "System"),
            (PromptEntry.Request, "User")
        });

        Assert.Equal("{\"ok\":true}", result);
        Assert.Single(client.Messages);
        Assert.Equal(3, client.Messages[0].Count);
        Assert.Equal(ChatRole.System, client.Messages[0][0].Role);
        Assert.Equal(ChatRole.System, client.Messages[0][1].Role);
        Assert.Equal(ChatRole.User, client.Messages[0][2].Role);
    }

    [Fact]
    public async Task GenerateTextAsync_StructuredOutputRequiresSingleSchema()
    {
        var client = new FakeChatClient();
        client.EnqueueResponse("{\"ok\":true}");
        var generator = CreateGenerator(client, useStructuredOutput: true);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            generator.GenerateTextAsync(new List<(PromptEntry, string)>
            {
                (PromptEntry.Context, "System"),
                (PromptEntry.Request, "User")
            }));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            generator.GenerateTextAsync(new List<(PromptEntry, string)>
            {
                (PromptEntry.Schema, SchemaJson),
                (PromptEntry.Schema, SchemaJson),
                (PromptEntry.Request, "User")
            }));
    }

    [Fact]
    public async Task GenerateTextAsync_EmptySchema_ThrowsArgumentException()
    {
        var client = new FakeChatClient();
        var generator = CreateGenerator(client, useStructuredOutput: true);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            generator.GenerateTextAsync(new List<(PromptEntry, string)>
            {
                (PromptEntry.Schema, " "),
                (PromptEntry.Request, "User")
            }));
    }

    [Fact]
    public async Task GenerateTextAsync_NoNonEmptyMessages_ThrowsArgumentException()
    {
        var client = new FakeChatClient();
        var generator = CreateGenerator(client, useStructuredOutput: false);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            generator.GenerateTextAsync(new List<(PromptEntry, string)>
            {
                (PromptEntry.Context, " "),
                (PromptEntry.Request, "")
            }));
    }

    [Fact]
    public async Task GenerateTextAsync_NullMessages_ThrowsArgumentNullException()
    {
        var client = new FakeChatClient();
        var generator = CreateGenerator(client);

        await Assert.ThrowsAsync<ArgumentNullException>(() => generator.GenerateTextAsync(null!));
    }

    [Fact]
    public async Task GenerateTextAsync_WrapsNonTransientException()
    {
        var client = new FakeChatClient();
        client.EnqueueException(new InvalidOperationException("boom"));
        var generator = CreateGenerator(client);

        var ex = await Assert.ThrowsAsync<Exception>(() =>
            generator.GenerateTextAsync(new List<(PromptEntry, string)>
            {
                (PromptEntry.Schema, SchemaJson),
                (PromptEntry.Request, "User")
            }));

        Assert.Equal("Failed to generate text.", ex.Message);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }

    [Fact]
    public async Task GenerateTextAsync_RetriesOnTransientExceptions()
    {
        var client = new FakeChatClient();
        client.EnqueueException(new HttpRequestException("transient"));
        client.EnqueueResponse("{\"ok\":true}");
        var generator = CreateGenerator(client);

        var result = await generator.GenerateTextAsync(new List<(PromptEntry, string)>
        {
            (PromptEntry.Schema, SchemaJson),
            (PromptEntry.Request, "User")
        });

        Assert.Equal("{\"ok\":true}", result);
        Assert.Equal(2, client.Messages.Count);
    }

    [Fact]
    public void Properties_InvalidValues_ThrowArgumentOutOfRangeException()
    {
        var generator = new OpenAiGenerator();

        Assert.Throws<ArgumentOutOfRangeException>(() => generator.MaxCompletionTokens = -1);
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Temperature = 2.5f);
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.TopP = -0.1f);
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.TopK = -5);
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.FrequencyPenalty = 3.0f);
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.PresencePenalty = -3.0f);
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.RepeatPenalty = 3.0f);
        Assert.Throws<ArgumentOutOfRangeException>(() => generator.Seed = -2);
    }

    [Fact]
    public void GetApiKeyFromEnvironment_SetsApiKey()
    {
        const string envName = "OPENAI_TEST_API_KEY";
        var generator = new OpenAiGenerator();
        Environment.SetEnvironmentVariable(envName, "test-key");
        try
        {
            generator.GetApiKeyFromEnvironment(envName);
            var apiKeyField = typeof(OpenAiGenerator).GetField("_apiKey", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(apiKeyField);
            Assert.Equal("test-key", apiKeyField!.GetValue(generator));
        }
        finally
        {
            Environment.SetEnvironmentVariable(envName, null);
        }
    }

    [Fact]
    public async Task GenerateTextAsync_RealRequest_WhenEnvironmentConfigured()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT");
        var model = Environment.GetEnvironmentVariable("OPENAI_MODEL");
        if (string.IsNullOrWhiteSpace(apiKey) ||
            string.IsNullOrWhiteSpace(endpoint) ||
            string.IsNullOrWhiteSpace(model))
        {
            return;
        }

        var generator = new OpenAiGenerator
        {
            UseStructuredOutput = true,
            Stop = new List<string>()
        };
        generator.ApiKey = apiKey;
        generator.Endpoint = endpoint;
        generator.Model = model;

        var result = await generator.GenerateTextAsync(new List<(PromptEntry, string)>
        {
            (PromptEntry.Schema, SchemaJson),
            (PromptEntry.Request, "Return a JSON object with a boolean field ok set to true.")
        });

        Assert.Contains("\"ok\"", result);
    }

    private static OpenAiGenerator CreateGenerator(FakeChatClient client, bool useStructuredOutput = true)
    {
        var generator = new OpenAiGenerator
        {
            UseStructuredOutput = useStructuredOutput,
            Stop = new List<string>()
        };
        SetClient(generator, client);
        return generator;
    }

    private static void SetClient(OpenAiGenerator generator, IChatClient client)
    {
        var property = typeof(OpenAiGenerator).GetProperty("Client");
        var setter = property?.GetSetMethod(true);
        if (setter == null)
        {
            throw new InvalidOperationException("Client setter was not found.");
        }
        setter.Invoke(generator, new object[] { client });
    }

    private sealed class FakeChatClient : IChatClient
    {
        private readonly Queue<Func<Task<ChatResponse>>> _responses = new();

        public List<IReadOnlyList<ChatMessage>> Messages { get; } = new();
        public List<ChatOptions> Options { get; } = new();

        public void EnqueueResponse(string text)
        {
            _responses.Enqueue(() =>
                Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, text))));
        }

        public void EnqueueException(Exception ex)
        {
            _responses.Enqueue(() => Task.FromException<ChatResponse>(ex));
        }

        public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions options,
            CancellationToken cancellationToken = default)
        {
            Messages.Add(messages.ToList());
            Options.Add(options);
            if (_responses.Count == 0)
            {
                return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, "{}")));
            }
            return _responses.Dequeue().Invoke();
        }

        public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(IEnumerable<ChatMessage> messages,
            ChatOptions options, CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("Streaming is not used in tests.");
        }

        public object? GetService(Type serviceType, object? serviceKey = null)
        {
            return null;
        }

        public void Dispose()
        {
            
        }
    }
}
