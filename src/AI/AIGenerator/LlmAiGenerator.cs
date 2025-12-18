using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AIGenerator.TextGenerator;
using BaseClasses.Interface;
using BaseClasses.Model;
using BaseClasses.Services;
using BaseClasses.Enum;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AIGenerator
{
    /// <summary>
    /// ИИ-генератор
    /// </summary>
    public class LlmAiGenerator : IGenerator, IChainGenerator
    {
        /// <summary>
        /// Генератор текста
        /// </summary>
        public ITextAiGenerator TextAiGenerator { get; set; }

        /// <summary>
        /// Системные подсказки
        /// </summary>
        public Dictionary<string, string> SystemPrompt { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Настройки сериализации
        /// </summary>
        public JsonSerializerOptions settings = new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            IgnoreReadOnlyProperties = true
        };

        /// <summary>
        /// Приоритет ИИ над заготовленным материалом
        /// </summary>
        public bool AIPriority { get; set; } = false;

        /// <summary>
        /// Использовать структурированный вывод
        /// </summary>
        private bool _useStructuredOutput = true;

        /// <summary>
        /// Использовать структурированный вывод
        /// </summary>
        public bool UseStructuredOutput
        {
            get
            {
                return _useStructuredOutput;
            }
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
        /// Загрузка системных подсказок
        /// </summary>
        /// <param name="path">Путь к файлу с подсказками</param>
        public void LoadSystemPrompt(string path)
        {
            SystemPrompt =
                JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(path))!;
        }

        /// <summary>
        /// Конструктор со стандартным ИИ
        /// </summary>
        /// <param name="promptPath">Путь к файлу с подсказками</param>
        public LlmAiGenerator(string promptPath)
        {
            LoadSystemPrompt(promptPath);
            TextAiGenerator = new OpenAIGenerator();
        }

        /// <summary>
        /// Конструктор с пользовательским ИИ
        /// </summary>
        /// <param name="promptPath">Путь к файлу с подсказками</param>
        /// <param name="textAIGenerator">Генератор текста</param>
        public LlmAiGenerator(string promptPath, ITextAiGenerator textAIGenerator)
        {
            LoadSystemPrompt(promptPath);
            TextAiGenerator = textAIGenerator;
        }

        /// <summary>
        /// Объединение двух компонентов истории в один
        /// </summary>
        /// <param name="preparedElement">Заготовленный компонент</param>
        /// <param name="aiElement">Сгенерированный компонент</param>
        /// <returns>Объединенный компонент</returns>
        private IElement Merge(IElement preparedElement, IElement aiElement)
        {
            Merger.Merge(preparedElement, aiElement, !AIPriority);
            return preparedElement;
        }

        /// <summary>
        /// Получение подсказок для генерации
        /// </summary>
        /// <param name="type">Тип необходимого элемента истории</param>
        /// <param name="plot">История</param>
        /// <param name="element">Подготовленный элемент истории</param>
        /// <returns>Список подсказок</returns>
        private List<string> GetPromptForResponse(Plot plot, IElement element)
        {
            List<string> prompts = new List<string>();
            if (UseStructuredOutput)
            {
                prompts.Add(SystemPrompt[$"{element.Type}StructuredOutput"]);
            }
            prompts.Add(SystemPrompt["Setting"]);
            prompts.Add(SystemPrompt[$"{element.Type}Start"]);
            if (plot.Characters.Count != 0)
            {
                foreach (var character in plot.Characters)
                {
                    prompts.Add(string.Format(SystemPrompt["CharacterUsage"],
                        JsonSerializer.Serialize(new AiElement(character), settings)));
                }
            }
            else
            {
                prompts.Add(SystemPrompt["CharacterEmpty"]);
            }
            if (plot.Locations.Count != 0)
            {
                foreach (var location in plot.Locations)
                {
                    prompts.Add(string.Format(SystemPrompt["LocationUsage"],
                        JsonSerializer.Serialize(new AiElement(location), settings)));
                }
            }
            else
            {
                prompts.Add(SystemPrompt["LocationEmpty"]);
            }
            if (plot.Items.Count != 0)
            {
                foreach (var item in plot.Items)
                {
                    prompts.Add(string.Format(SystemPrompt["ItemUsage"],
                        JsonSerializer.Serialize(new AiElement(item), settings)));
                }
            }
            else
            {
                prompts.Add(SystemPrompt["ItemEmpty"]);
            }
            if (plot.Events.Count != 0)
            {
                foreach (var ev in plot.Events)
                {
                    prompts.Add(string.Format(SystemPrompt["EventUsage"],
                        JsonSerializer.Serialize(new AiElement(ev), settings)));
                }
            }
            else
            {
                prompts.Add(SystemPrompt["EventEmpty"]);
            }
            if (!element.IsEmpty())
            {
                prompts.Add(string.Format(SystemPrompt[$"{element.Type}Prepared"],
                    JsonSerializer.Serialize(new AiElement(element), settings)));
            }
            prompts.Add(SystemPrompt[$"{element.Type}End"]);
            return prompts;
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
            List<string> prompts = GetPromptForResponse(plot, preparedElement);
            string response = await TextAiGenerator.GenerateTextAsync(prompts);
            try
            {
                AiElement aiElement =
                    JsonSerializer.Deserialize<AiElement>(response)!;
                aiElement.ParamsJsonToSystem();
                IElement element = Merge(preparedElement, aiElement.Element(plot));
                plot.Add(element);
                return element;
            }
            catch (JsonException e)
            {
                throw new Exception(response, e);
            }
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
        public async Task<IElement> GenerateChainAsync(Plot plot,
            IElement preparedElement,
            Queue<(IElement, IElement, int)>? generationQueue = null,
            int recursion = 3)
        {
            bool isRoot = generationQueue == null;
            if (generationQueue == null) generationQueue = new Queue<(IElement, IElement, int)>();
            List<string> prompts = GetPromptForResponse(plot, preparedElement);
            string response = await TextAiGenerator.GenerateTextAsync(prompts);
            try
            {
                AiElement aiElement =
                    JsonSerializer.Deserialize<AiElement>(response)!;
                aiElement.ParamsJsonToSystem();
                IElement element = Merge(preparedElement, aiElement.Element(plot));
                plot.Add(element);
                if (recursion > 0)
                {
                    foreach (var (type, list) in aiElement.NewElements(plot))
                    {
                        foreach (string e in list)
                        {
                            double relation = 0;
                            if (aiElement.Params.ContainsKey("Relations") &&
                                ((Dictionary<string, double>)aiElement.Params["Relations"])
                                .ContainsKey(e))
                            {
                                relation = 
                                    ((Dictionary<string, double>)aiElement.Params["Relations"])[e];
                            }
                            var queuedTuple = generationQueue.FirstOrDefault(q => q.Item1.Name == e);
                            IElement? queuedElement = queuedTuple.Item1;
                            if (queuedElement != null)
                            {
                                Binder.Bind(queuedElement, element, relation);
                            }
                            else 
                            {
                                Element newElement = new Element(type, e);
                                Binder.Bind(element, newElement, relation);
                                generationQueue.Enqueue((newElement, element, recursion - 1));
                            }
                        }
                    }
                }
                while (isRoot && generationQueue.Count > 0)
                {
                    var (newElement, parent, rec) = generationQueue.Dequeue();
                    newElement = await GenerateChainAsync(plot, newElement, generationQueue, rec);
                }
                return element;
            }
            catch (JsonException e)
            {
                throw new Exception(response, e);
            }
        }
    }
}
