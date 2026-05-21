using System;
using System.Collections.Generic;
using System.IO;
using AIGenerator.TextGenerator;
using BaseClasses.Interface;
using BaseClasses.Model;
using System.Text.Json;
using System.Threading.Tasks;
using AIGenerator.DtoProvider;
using AIGenerator.Prompt;

namespace AIGenerator
{
    /// <summary>
    /// ИИ-генератор.
    /// </summary>
    public class LlmAiGenerator : IGenerator, IChainGenerator
    {
        /// <summary>
        /// Генератор текста.
        /// </summary>
        public ITextAiGenerator TextAiGenerator { get; set; }
        
        /// <summary>
        /// Провайдер для преобразования элементов и сюжета в DTO-формат и обратно. Необходим для передачи
        /// информации о сюжете и элементах в ИИ в формате, который он может понять, а также для получения информации
        /// от ИИ и преобразования ее обратно в элементы сюжета. Если не установлен, будет использоваться
        /// SerializerDtoProvider по умолчанию.
        /// </summary>
        public IDtoProvider DtoProvider { get; set; }

        /// <summary>
        /// Системные подсказки для генерации, загружаются из JSON-файла.
        /// </summary>
        public Dictionary<string, string> PromptTemplates { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Приоритет ИИ над заготовленным материалом.
        /// </summary>
        public bool AiPriority { get; set; } = false;

        /// <summary>
        /// Использовать структурированный вывод.
        /// </summary>
        private bool _useStructuredOutput = true;

        /// <summary>
        /// Использовать структурированный вывод.
        /// </summary>
        public bool UseStructuredOutput
        {
            get => _useStructuredOutput;
            set
            {
                _useStructuredOutput = value;
                if (TextAiGenerator is ISupportStructuredOutput supportStructuredOutput)
                {
                    supportStructuredOutput.UseStructuredOutput = value;
                }
            }
        }

        /// <summary>
        /// Загрузка шаблонов из JSON-файла по указанному пути. JSON должен представлять собой словарь,
        /// где ключами являются строки (ключи шаблонов), а значениями - строки (тексты шаблонов).
        /// </summary>
        /// <param name="path">Путь к JSON-файлу, содержащему шаблоны. JSON должен представлять собой словарь,
        /// где ключами являются строки (ключи шаблонов), а значениями - строки (тексты шаблонов).</param>
        public void LoadPromptTemplates(string path)
        {
            PromptTemplates = PromptBuilder.DeserializeTemplateDictionary(File.ReadAllText(path)) ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Конструктор со стандартным ИИ.
        /// </summary>
        /// <param name="promptPath">Путь к файлу с подсказками.</param>
        public LlmAiGenerator(string promptPath)
        {
            LoadPromptTemplates(promptPath);
            TextAiGenerator = new OpenAiGenerator();
            DtoProvider = new SerializerDtoProvider();
        }

        /// <summary>
        /// Конструктор с пользовательским ИИ.
        /// </summary>
        /// <param name="promptPath">Путь к файлу с подсказками.</param>
        /// <param name="textAiGenerator">Генератор текста.</param>
        public LlmAiGenerator(string promptPath, ITextAiGenerator textAiGenerator)
        {
            LoadPromptTemplates(promptPath);
            TextAiGenerator = textAiGenerator;
            DtoProvider = new SerializerDtoProvider();
        }

        /// <summary>
        /// Получение подсказок для генерации
        /// </summary>
        /// <param name="plot">История</param>
        /// <param name="element">Подготовленный элемент истории</param>
        /// <returns>Список подсказок</returns>
        private List<(PromptEntry, string)> GetPromptForResponse(Plot plot, IElement element)
        {
            var pb = new PromptBuilder(PromptTemplates);
            pb.AddMessage(PromptEntry.Schema, DtoProvider.GetSchema());
            pb.AddMessageFromTemplate(PromptEntry.Context, "Setting");
            pb.AddMessageFromTemplate(PromptEntry.Context, "Plot", DtoProvider.ToDto(plot));
            pb.AddMessageFromTemplate(PromptEntry.Request, "Element", DtoProvider.ToDto(element));
            return pb.Build();
        }

        /// <summary>
        /// Генерация элемента истории
        /// </summary>
        /// <param name="plot">История</param>
        /// <param name="preparedElement">Подготовленный элемент истории</param>
        /// <returns>Сгенерированный элемент истории</returns>
        /// <exception cref="Exception">Нейросеть вернула недействительный json</exception>
        public async Task<IElement> GenerateAsync(Plot plot, IElement preparedElement)
        {
            var prompts = GetPromptForResponse(plot, preparedElement);
            string response = await TextAiGenerator.GenerateTextAsync(prompts);
            var aiElement = DtoProvider.FromDto(response, plot);
            plot.Merge(preparedElement, aiElement, AiPriority);
            return preparedElement;
        }

        public IElement Generate(Plot plot, IElement preparedElement)
        {
            return GenerateAsync(plot, preparedElement).GetAwaiter().GetResult();
        }

        public Task<IElement> GenerateChainAsync(Plot plot, IElement preparedElement, int recursion = 3)
        {
            return GenerateChainAsync(plot, preparedElement, null, recursion);
        }

        public IElement GenerateChain(Plot plot, IElement preparedElement, int recursion = 3)
        {
            return GenerateChainAsync(plot, preparedElement, null, recursion).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Генерация цепочки элементов истории
        /// </summary>
        /// <param name="plot">История</param>
        /// <param name="preparedElement">Подготовленный элемент истории</param>
        /// <param name="generationQueue">Очередь генерации, следует оставить пустым</param>
        /// <param name="recursion">Глубина рекурсии</param>
        /// <returns>Сгенерированный элемент истории</returns>
        /// <exception cref="Exception">Нейросеть вернула недействительный json</exception>
        private async Task<IElement> GenerateChainAsync(Plot plot,
            IElement preparedElement,
            Queue<(IElement, IElement, int)>? generationQueue = null,
            int recursion = 3)
        {
            bool isRoot = generationQueue == null;
            generationQueue ??= new Queue<(IElement, IElement, int)>();
            var prompts = GetPromptForResponse(plot, preparedElement);
            string response = await TextAiGenerator.GenerateTextAsync(prompts);
            try
            {
                var aiElement = DtoProvider.FromDto(response, plot);
                plot.Merge(preparedElement, aiElement, AiPriority);
                if (recursion > 0)
                {
                    foreach (var (element, paramKey, value) in DtoProvider.GetNewElements(response, plot))
                    {
                        plot.Add(element);
                        plot.Bind(preparedElement, element, paramKey, value);
                        generationQueue.Enqueue((element, preparedElement, recursion - 1));
                    }
                }
                while (isRoot && generationQueue.Count > 0)
                {
                    var (newElement, _, rec) = generationQueue.Dequeue();
                    await GenerateChainAsync(plot, newElement, generationQueue, rec);
                }
                return preparedElement;
            }
            catch (JsonException e)
            {
                // TODO: ERROR HANDLING - Using generic Exception loses context
                // Should throw a custom AiResponseParsingException with:
                // - Original JSON response for debugging
                // - Inner exception for tracing
                // - More specific error categorization
                // This makes error handling and debugging much more difficult
                throw new Exception(response, e);
            }
        }
    }
}
