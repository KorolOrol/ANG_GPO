using System;
using System.ClientModel;
using Microsoft.Extensions.AI;
using OpenAI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using AIGenerator.Prompt;

namespace AIGenerator.TextGenerator
{
    /// <summary>
    /// Класс для генерации текста с помощью OpenAI API
    /// </summary>
    public class OpenAiGenerator : ITextAiGenerator, ISupportStructuredOutput
    {
        /// <summary>
        /// Ключ API для OpenAI
        /// </summary>
        // TODO: CRITICAL SECURITY ISSUE - Remove hardcoded default API key
        // This should not have a default value. Require explicit initialization via environment variable or property setter.
        // See analysis: src/AI/AIGenerator/TextGenerator/OpenAiGenerator.cs:22
        private string _apiKey = "YOUR_API_KEY_HERE";

        /// <summary>
        /// Адрес API
        /// </summary>
        private string _endpoint = "https://api.openai.com";

        /// <summary>
        /// Модель для генерации текста
        /// </summary>
        private string _model = "gpt-5.4-mini-2026-03-17";

        private int _maxCompletionTokens = 8192;
        private float _temperature = 1.0f;
        private float _topP = 0.95f;
        private int _topK = 40;
        private float _frequencyPenalty = 0.0f;
        private float _presencePenalty = 0.0f;
        private float _repeatPenalty = 1.1f;
        private int _seed = -1;

        /// <summary>
        /// Максимальное количество токенов для генерации текста.
        /// </summary>
        public int MaxCompletionTokens
        {
            get => _maxCompletionTokens;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(MaxCompletionTokens),
                    "Value must be non-negative.");
                _maxCompletionTokens = value;
            }
        }

        /// <summary>
        /// Температура для генерации текста. Чем выше значение, тем более разнообразные и креативные ответы
        /// будет генерировать модель.
        /// </summary>
        public float Temperature
        {
            get => _temperature;
            set
            {
                if (value < 0.0 || value > 2.0) throw new ArgumentOutOfRangeException(nameof(Temperature),
                    "Value must be between 0.0 and 2.0.");
                _temperature = value;
            }
        }

        /// <summary>
        /// Top-p (nucleus sampling) для генерации текста. Этот параметр определяет порог вероятности
        /// для выбора токенов при генерации текста.
        /// </summary>
        public float TopP
        {
            get => _topP;
            set
            {
                if (value < 0.0 || value > 1.0) throw new ArgumentOutOfRangeException(nameof(TopP),
                    "Value must be between 0.0 and 1.0");
                _topP = value;
            }
        }

        /// <summary>
        /// Top-k для генерации текста. Этот параметр определяет количество наиболее вероятных токенов,
        /// из которых будет выбран следующий токен при генерации текста.
        /// </summary>
        public int TopK
        {
            get => _topK;
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(TopK), "Value must be non-negative.");
                _topK = value;
            }
        }

        /// <summary>
        /// Частотное наказание для генерации текста. Этот параметр определяет, насколько сильно модель будет
        /// наказывать токены, которые уже были сгенерированы в тексте. Чем выше значение, тем сильнее
        /// будет наказание за повторение токенов.
        /// </summary>
        public float FrequencyPenalty
        {
            get => _frequencyPenalty;
            set
            {
                if (value < -2.0 || value > 2.0) throw new ArgumentOutOfRangeException(nameof(FrequencyPenalty),
                    "Value must be between -2.0 and 2.0");
                _frequencyPenalty = value;
            }
        }

        /// <summary>
        /// Наказание за присутствие для генерации текста. Этот параметр определяет, насколько сильно модель будет
        /// наказывать токены, которые уже были сгенерированы в тексте, независимо от их частоты. Чем выше значение,
        /// тем сильнее будет наказание за повторение токенов, даже если они не были сгенерированы часто.
        /// Это может помочь уменьшить повторение одних и тех же фраз или слов в тексте,
        /// особенно если они были сгенерированы несколько раз, но не часто.
        /// </summary>
        public float PresencePenalty
        {
            get => _presencePenalty;
            set
            {
                if (value < -2.0 || value > 2.0) throw new ArgumentOutOfRangeException(nameof(PresencePenalty),
                    "Value must be between -2.0 and 2.0");
                _presencePenalty = value;
            }
        }

        /// <summary>
        /// Наказание за повторение для генерации текста. Этот параметр определяет, насколько сильно модель будет
        /// наказывать токены, которые уже были сгенерированы в тексте, учитывая как их частоту, так и присутствие.
        /// </summary>
        public float RepeatPenalty
        {
            get => _repeatPenalty;
            set
            {
                if (value < 0.0 || value > 2.0) throw new ArgumentOutOfRangeException(nameof(RepeatPenalty),
                    "Value must be between 0.0 and 2.0");
                _repeatPenalty = value;
            }
        }

        /// <summary>
        /// Семя для генерации текста.
        /// </summary>
        public int Seed
        {
            get => _seed;
            set
            {
                if (value < 0 && value != -1) throw new ArgumentOutOfRangeException(nameof(Seed),
                    "Value must be non-negative or -1 for random seed.");
                _seed = value;
            }
        }

        /// <summary>
        /// Список токенов, при генерации которых модель должна остановиться.
        /// </summary>
        // TODO: Potential null reference issue - verify Stop is never null before use.
        // See line 417: if (Stop.Count > 0) can throw NullReferenceException if Stop is null.
        // Consider lazy initialization or non-null guarantee in constructor.
        public List<string> Stop { get; set; }

        /// <summary>
        /// Ключ API для OpenAI
        /// </summary>
        public string ApiKey
        {
            private get => _apiKey;
            set
            {
                _apiKey = value;
                UpdateClient();
            }
        }

        /// <summary>
        /// Адрес API
        /// </summary>
        public string Endpoint
        {
            get => _endpoint;
            set
            {
                _endpoint = value;
                UpdateClient();
            }
        }

        /// <summary>
        /// Использовать структурированный вывод
        /// </summary>
        public bool UseStructuredOutput { get; set; } = true;

        /// <summary>
        /// Получить ключ API из переменной окружения
        /// </summary>
        /// <param name="envVarName">Имя переменной окружения</param>
        public void GetApiKeyFromEnvironment(string envVarName)
        {
            string? envVar;
            // Try to get the environment variable in the most compatible way
            // On Windows, try User first, then Process; on other platforms, just use the default
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                envVar = Environment.GetEnvironmentVariable(envVarName, EnvironmentVariableTarget.User)
                    ?? Environment.GetEnvironmentVariable(envVarName, EnvironmentVariableTarget.Process);
            }
            else
            {
                envVar = Environment.GetEnvironmentVariable(envVarName);
            }
            if (!string.IsNullOrEmpty(envVar))
            {
                ApiKey = envVar;
            }
        }

        /// <summary>
        /// Клиент OpenAI
        /// </summary>
        public IChatClient Client { get; private set; }

        /// <summary>
        /// Модель для генерации текста
        /// </summary>
        public string Model
        {
            get => _model;
            set
            {
                _model = value;
                UpdateClient();
            }
        }

        public List<Func<string, string>> ResultFilters { get; set; } = new List<Func<string, string>>
        {
            (result) => result.Replace("\n\n", "")
        };

        private const int MaxRetryAttempts = 3;
        private static readonly TimeSpan _RetryBaseDelay = TimeSpan.FromSeconds(1);
        private static readonly TimeSpan _RetryMaxDelay = TimeSpan.FromSeconds(8);

        private static bool IsTransient(Exception ex)
        {
            if (ex is TimeoutException || ex is TaskCanceledException || ex is HttpRequestException || ex is System.IO.IOException)
            {
                return true;
            }
            if (ex.InnerException != null)
            {
                return IsTransient(ex.InnerException);
            }
            return false;
        }

        private static TimeSpan GetRetryDelay(int attempt)
        {
            var delayMs = Math.Min(_RetryMaxDelay.TotalMilliseconds,
                _RetryBaseDelay.TotalMilliseconds * Math.Pow(2, attempt - 1));
            return TimeSpan.FromMilliseconds(delayMs);
        }

        private static string ExtractFirstJsonObject(string text)
        {
            // TODO: CODE QUALITY - Excessive complexity in manual JSON parsing
            // This method has 5+ levels of nested conditions and manual state tracking.
            // Issues:
            // - Hard to understand and test
            // - Doesn't handle edge cases (incomplete JSON, complex escaping)
            // - Consider using JsonDocument.Parse() with try/catch instead
            // - Or use a proven library for robust JSON extraction
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            int start = text.IndexOf('{');
            if (start < 0) return string.Empty;

            int depth = 0;
            bool inString = false;
            bool escape = false;

            for (int i = start; i < text.Length; i++)
            {
                char c = text[i];
                if (inString)
                {
                    if (escape)
                    {
                        escape = false;
                        continue;
                    }
                    if (c == '\\')
                    {
                        escape = true;
                        continue;
                    }
                    if (c == '"')
                    {
                        inString = false;
                    }
                    continue;
                }

                if (c == '"')
                {
                    inString = true;
                    continue;
                }

                if (c == '{')
                {
                    depth++;
                    continue;
                }

                if (c == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        return text.Substring(start, i - start + 1);
                    }
                }
            }

            return string.Empty;
        }

        private void UpdateClient()
        {
            var openAiClient = new OpenAIClient(new ApiKeyCredential(ApiKey),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(Endpoint),
                    NetworkTimeout = TimeSpan.FromMinutes(30)
                });

            Client = openAiClient.GetChatClient(Model).AsIChatClient();
        }

        /// <summary>
        /// Генерация текста
        /// </summary>
        /// <param name="messages">Список сообщений. 
        /// Если UseStructuredOutput - true, то сообщение со схемой используется 
        /// для структурированного вывода.</param>
        /// <returns>Сгенерированный текст</returns>
        /// <exception cref="Exception">Ошибка генерации текста</exception>
        // TODO: METHOD COMPLEXITY - GenerateTextAsync is 110+ lines with multiple responsibilities.
        // Consider breaking into:
        // 1. ValidateMessages()
        // 2. BuildChatMessages()
        // 3. ConfigureOptions()
        // 4. ExecuteWithRetry()
        // This would improve readability, testability, and maintainability.
        public async Task<string> GenerateTextAsync(List<(PromptEntry, string)> messages)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));

            var schemaEntries = messages.Where(m => m.Item1 == PromptEntry.Schema).ToList();
            if (UseStructuredOutput)
            {
                if (schemaEntries.Count != 1)
                {
                    throw new ArgumentException("When structured output is enabled, exactly one schema must be provided.",
                        nameof(messages));
                }
                if (string.IsNullOrWhiteSpace(schemaEntries[0].Item2))
                {
                    throw new ArgumentException("Schema content must be non-empty.", nameof(messages));
                }
            }

            var chatMessages = new List<ChatMessage>();
            foreach (var (entryType, content) in messages)
            {
                if (string.IsNullOrWhiteSpace(content)) continue;

                switch (entryType)
                {
                    case PromptEntry.Context:
                        chatMessages.Add(new ChatMessage(ChatRole.System, content));
                        break;
                    case PromptEntry.Request:
                        chatMessages.Add(new ChatMessage(ChatRole.User, content));
                        break;
                    case PromptEntry.Schema:
                        if (!UseStructuredOutput)
                        {
                            chatMessages.Add(new ChatMessage(ChatRole.System, content));
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(messages), "Unknown prompt entry type.");
                }
            }

            if (chatMessages.Count == 0)
            {
                throw new ArgumentException("At least one non-empty message must be provided.", nameof(messages));
            }

            var options = new ChatOptions();
            options.AdditionalProperties ??= new AdditionalPropertiesDictionary();
            
            if (UseStructuredOutput)
            {
                var schemaElement = JsonDocument.Parse(schemaEntries[0].Item2).RootElement;
                options.ResponseFormat = ChatResponseFormat.ForJsonSchema(schemaElement, "AiElement");
            }

            options.MaxOutputTokens = MaxCompletionTokens;
            options.Temperature = Temperature;
            options.TopP = TopP;
            options.FrequencyPenalty = FrequencyPenalty;
            options.PresencePenalty = PresencePenalty;
            options.AdditionalProperties["top_k"] = TopK;
            options.AdditionalProperties["repetition_penalty"] = RepeatPenalty;

            if (Seed != -1)
            {
                options.AdditionalProperties["seed"] = Seed;
            }
            // TODO: POTENTIAL NULL REFERENCE - Stop can be null, causing NullReferenceException
            // Ensure Stop is never null. Either initialize in constructor or check explicitly.
            if (Stop != null && Stop.Count > 0)
            {
                options.StopSequences = Stop;
            }

            Exception? lastException = null;
            for (int attempt = 1; attempt <= MaxRetryAttempts; attempt++)
            {
                try
                {
                    var completion = await Client.GetResponseAsync(chatMessages, options);
                    string rawText = completion.Text;
                    string trimmedResult = rawText.Trim();

                    if (!UseStructuredOutput)
                    {
                        trimmedResult = ExtractFirstJsonObject(trimmedResult);
                    }

                    foreach (var filter in ResultFilters)
                    {
                        trimmedResult = filter(trimmedResult);
                    }

                    if (string.IsNullOrWhiteSpace(trimmedResult))
                    {
                        throw new Exception("Failed to generate text: " + rawText);
                    }
                    return trimmedResult;
                }
                catch (Exception ex) when (IsTransient(ex) && attempt < MaxRetryAttempts)
                {
                    lastException = ex;
                    await Task.Delay(GetRetryDelay(attempt));
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to generate text.", ex);
                }
            }

            throw new Exception("Failed to generate text after retries.", lastException);
        }

        /// <summary>
        /// Стандартный конструктор с ключом OpenAI API из переменной окружения
        /// </summary>
        public OpenAiGenerator()
        {
            GetApiKeyFromEnvironment("OpenAIAPIKey");
            UpdateClient();
            Stop = new List<string>();
        }

        /// <summary>
        /// Конструктор с ключом API из переменной окружения и адресом API
        /// </summary>
        /// <param name="keyEnvVar">Имя переменной окружения с ключом API</param>
        /// <param name="endpoint">Адрес API</param>
        public OpenAiGenerator(string keyEnvVar, string endpoint)
        {
            GetApiKeyFromEnvironment(keyEnvVar);
            Endpoint = endpoint;
            UpdateClient();
            Stop = new List<string>();
        }
    }
}
