using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AIGenerator;
using AIGenerator.DtoProvider;
using AIGenerator.Prompt;
using AIGenerator.TextGenerator;
using BaseClasses.Enum;
using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Model.Params;
using Xunit;

namespace AiGenerator.Tests
{
    /// <summary>
    /// Тесты для класса LlmAiGenerator.
    /// </summary>
    public class LlmAiGeneratorTests
    {
        /// <summary>
        /// Проверяет, что UseStructuredOutput синхронизируется с ITextAiGenerator, поддерживающим структурированный вывод.
        /// </summary>
        [Fact]
        public void UseStructuredOutput_SetsFlagOnTextGenerator()
        {
            var promptPath = CreatePromptTemplatesFile();
            try
            {
                var textGenerator = new FakeTextAiGenerator();
                var generator = new LlmAiGenerator(promptPath, textGenerator);

                generator.UseStructuredOutput = false;

                Assert.False(generator.UseStructuredOutput);
                Assert.False(textGenerator.UseStructuredOutput);

                generator.UseStructuredOutput = true;

                Assert.True(generator.UseStructuredOutput);
                Assert.True(textGenerator.UseStructuredOutput);
            }
            finally
            {
                File.Delete(promptPath);
            }
        }

        /// <summary>
        /// Проверяет, что UseStructuredOutput не выбрасывает исключений, если генератор не поддерживает интерфейс.
        /// </summary>
        [Fact]
        public void UseStructuredOutput_WithoutSupport_DoesNotThrow()
        {
            var promptPath = CreatePromptTemplatesFile();
            try
            {
                var textGenerator = new PlainTextAiGenerator();
                var generator = new LlmAiGenerator(promptPath, textGenerator);

                var ex = Record.Exception(() => generator.UseStructuredOutput = false);

                Assert.Null(ex);
                Assert.False(generator.UseStructuredOutput);
            }
            finally
            {
                File.Delete(promptPath);
            }
        }

        /// <summary>
        /// Проверяет, что загрузка шаблонов заполняет коллекцию PromptTemplates.
        /// </summary>
        [Fact]
        public void LoadPromptTemplates_ReadsTemplatesFromFile()
        {
            var promptPath = CreatePromptTemplatesFile();
            try
            {
                var generator = new LlmAiGenerator(promptPath, new FakeTextAiGenerator());

                Assert.Equal("System prompt", generator.PromptTemplates["Setting"]);
                Assert.Equal("Plot: {0}", generator.PromptTemplates["Plot"]);
                Assert.Equal("Element: {0}", generator.PromptTemplates["Element"]);
            }
            finally
            {
                File.Delete(promptPath);
            }
        }

        /// <summary>
        /// Проверяет, что GenerateAsync строит промпт, вызывает LLM и объединяет результат с учетом AiPriority.
        /// </summary>
        [Fact]
        public async Task GenerateAsync_BuildsPrompt_AndMergesWithAiPriority()
        {
            var promptPath = CreatePromptTemplatesFile();
            try
            {
                var textGenerator = new FakeTextAiGenerator(new[] { "ai-response" });
                var aiElement = new Element(ElemType.Character, "AI");
                var dtoProvider = CreateDtoProvider((dto, _) => aiElement);
                var generator = new LlmAiGenerator(promptPath, textGenerator)
                {
                    DtoProvider = dtoProvider,
                    AiPriority = true
                };
                var mergeCalls = new List<(IElement BaseElement, IElement TargetElement, bool BasePriority)>();
                var plot = new Plot(mergeAction: (baseElement, targetElement, _, basePriority) =>
                    mergeCalls.Add((baseElement, targetElement, basePriority)));
                var prepared = new Element(ElemType.Character, "Prepared");

                var result = await generator.GenerateAsync(plot, prepared);

                Assert.Same(prepared, result);
                Assert.Single(textGenerator.Calls);
                Assert.Single(mergeCalls);
                Assert.Same(prepared, mergeCalls[0].BaseElement);
                Assert.Same(aiElement, mergeCalls[0].TargetElement);
                Assert.True(mergeCalls[0].BasePriority);

                var prompt = textGenerator.Calls[0];
                Assert.Collection(prompt,
                    entry => Assert.Equal((PromptEntry.Schema, "schema"), entry),
                    entry => Assert.Equal((PromptEntry.Context, "System prompt"), entry),
                    entry => Assert.Equal((PromptEntry.Context, "Plot: plot-dto"), entry),
                    entry => Assert.Equal((PromptEntry.Request, "Element: element-dto"), entry));
            }
            finally
            {
                File.Delete(promptPath);
            }
        }

        /// <summary>
        /// Проверяет, что Generate возвращает подготовленный элемент и использует синхронную оболочку.
        /// </summary>
        [Fact]
        public void Generate_ReturnsPreparedElement()
        {
            var promptPath = CreatePromptTemplatesFile();
            try
            {
                var textGenerator = new FakeTextAiGenerator(new[] { "ai-response" });
                var dtoProvider = CreateDtoProvider((_, __) => new Element(ElemType.Character, "AI"));
                var generator = new LlmAiGenerator(promptPath, textGenerator)
                {
                    DtoProvider = dtoProvider
                };
                var plot = new Plot();
                var prepared = new Element(ElemType.Character, "Prepared");

                var result = generator.Generate(plot, prepared);

                Assert.Same(prepared, result);
                Assert.Single(textGenerator.Calls);
            }
            finally
            {
                File.Delete(promptPath);
            }
        }

        /// <summary>
        /// Проверяет, что GenerateChainAsync добавляет новые элементы, создает связи и обходит очередь генерации.
        /// </summary>
        [Fact]
        public async Task GenerateChainAsync_AddsElementsAndProcessesQueue()
        {
            var promptPath = CreatePromptTemplatesFile();
            try
            {
                var textGenerator = new FakeTextAiGenerator(new[] { "root-response", "child-response" });
                var relationKey = new ParamKey<int>("Related");
                var childElement = new Element(ElemType.Item, "Child");
                var aiElements = new Queue<IElement>(new IElement[]
                {
                    new Element(ElemType.Character, "AI-Root"),
                    new Element(ElemType.Item, "AI-Child")
                });
                var dtoProvider = CreateDtoProvider(
                    (_, __) => aiElements.Dequeue(),
                    (dto, _) => dto == "root-response"
                        ? new List<(IElement, IParamKey, object)> { (childElement, relationKey, 3) }
                        : new List<(IElement, IParamKey, object)>());
                var generator = new LlmAiGenerator(promptPath, textGenerator)
                {
                    DtoProvider = dtoProvider
                };
                var bindCalls = 0;
                var binder = new BaseClasses.Services.Binds.Binder();
                binder.Register(ElemType.Character, ElemType.Item, relationKey,
                    (source, target, key, value, plot) =>
                    {
                        bindCalls++;
                        plot.Relations.Add(new Relation(source, target, key, value));
                    },
                    (_, _, _, _, _) => { });
                var plot = new Plot(binder);
                var prepared = new Element(ElemType.Character, "Root");

                await generator.GenerateChainAsync(plot, prepared, recursion: 1);

                Assert.Equal(2, plot.Elements.Count);
                Assert.Contains(prepared, plot.Elements);
                Assert.Contains(childElement, plot.Elements);
                Assert.Equal(1, bindCalls);
                Assert.Equal(2, textGenerator.Calls.Count);
            }
            finally
            {
                File.Delete(promptPath);
            }
        }

        /// <summary>
        /// Проверяет, что GenerateChainAsync оборачивает JsonException в Exception с текстом ответа.
        /// </summary>
        [Fact]
        public async Task GenerateChainAsync_WrapsJsonExceptionWithResponse()
        {
            var promptPath = CreatePromptTemplatesFile();
            try
            {
                var textGenerator = new FakeTextAiGenerator(new[] { "bad-json" });
                var dtoProvider = CreateDtoProvider((_, __) => throw new JsonException("Bad json"));
                var generator = new LlmAiGenerator(promptPath, textGenerator)
                {
                    DtoProvider = dtoProvider
                };
                var plot = new Plot();
                var prepared = new Element(ElemType.Character, "Prepared");

                var ex = await Assert.ThrowsAsync<Exception>(() =>
                    generator.GenerateChainAsync(plot, prepared, recursion: 0));

                Assert.Equal("bad-json", ex.Message);
                Assert.IsType<JsonException>(ex.InnerException);
            }
            finally
            {
                File.Delete(promptPath);
            }
        }

        private static FakeDtoProvider CreateDtoProvider(
            Func<string, Plot, IElement>? fromDto = null,
            Func<string, Plot, List<(IElement, IParamKey, object)>>? getNewElements = null)
        {
            return new FakeDtoProvider
            {
                Schema = () => "schema",
                PlotDto = _ => "plot-dto",
                ElementDto = _ => "element-dto",
                FromDtoFunc = fromDto ?? ((_, __) => new Element(ElemType.Character, "AI")),
                GetNewElementsFunc = getNewElements ?? ((_, __) => new List<(IElement, IParamKey, object)>())
            };
        }

        private static string CreatePromptTemplatesFile()
        {
            var templates = new Dictionary<string, string>
            {
                ["Setting"] = "System prompt",
                ["Plot"] = "Plot: {0}",
                ["Element"] = "Element: {0}"
            };
            var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");
            File.WriteAllText(path, JsonSerializer.Serialize(templates));
            return path;
        }

        private sealed class FakeTextAiGenerator : ITextAiGenerator, ISupportStructuredOutput
        {
            private readonly Queue<string> _responses;

            public FakeTextAiGenerator(IEnumerable<string>? responses = null)
            {
                _responses = new Queue<string>(responses ?? Array.Empty<string>());
            }

            public List<List<(PromptEntry, string)>> Calls { get; } = new List<List<(PromptEntry, string)>>();

            public bool UseStructuredOutput { get; set; }

            public string ApiKey
            {
                set { }
            }

            public string Endpoint { get; set; } = string.Empty;

            public string Model { get; set; } = string.Empty;

            public Task<string> GenerateTextAsync(List<(PromptEntry, string)> messages)
            {
                Calls.Add(messages);
                return Task.FromResult(_responses.Count > 0 ? _responses.Dequeue() : string.Empty);
            }
        }

        private sealed class PlainTextAiGenerator : ITextAiGenerator
        {
            public string ApiKey
            {
                set { }
            }

            public string Endpoint { get; set; } = string.Empty;

            public string Model { get; set; } = string.Empty;

            public Task<string> GenerateTextAsync(List<(PromptEntry, string)> messages)
            {
                return Task.FromResult(string.Empty);
            }
        }

        private sealed class FakeDtoProvider : IDtoProvider
        {
            public Func<Plot, string>? PlotDto { get; set; }
            public Func<IElement, string>? ElementDto { get; set; }
            public Func<string, Plot, IElement>? FromDtoFunc { get; set; }
            public Func<string>? Schema { get; set; }
            public Func<string, Plot, List<(IElement, IParamKey, object)>>? GetNewElementsFunc { get; set; }

            public string ToDto(Plot plot)
            {
                return PlotDto?.Invoke(plot) ?? string.Empty;
            }

            public string ToDto(IElement element)
            {
                return ElementDto?.Invoke(element) ?? string.Empty;
            }

            public IElement FromDto(string dto, Plot plot)
            {
                if (FromDtoFunc == null)
                {
                    throw new InvalidOperationException("FromDto is not configured.");
                }
                return FromDtoFunc(dto, plot);
            }

            public string GetSchema()
            {
                return Schema?.Invoke() ?? string.Empty;
            }

            public List<(IElement, IParamKey, object)> GetNewElements(string dto, Plot plot)
            {
                return GetNewElementsFunc?.Invoke(dto, plot) ?? new List<(IElement, IParamKey, object)>();
            }
        }
    }
}
