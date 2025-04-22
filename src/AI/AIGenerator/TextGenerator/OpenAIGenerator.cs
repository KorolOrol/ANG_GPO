using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.RegularExpressions;

namespace AIGenerator.TextGenerator
{
    /// <summary>
    /// Класс для генерации текста с помощью OpenAI API
    /// </summary>
    public class OpenAIGenerator : ITextAiGenerator, ISupportStructuredOutput
    {
        /// <summary>
        /// Ключ API для OpenAI
        /// </summary>
        private string _apiKey = "YOUR_API_KEY_HERE";

        /// <summary>
        /// Адрес API
        /// </summary>
        private string _endpoint = "https://api.openai.com";

        /// <summary>
        /// Клиент OpenAI
        /// </summary>
        private ChatClient _client;

        /// <summary>
        /// Модель для генерации текста
        /// </summary>
        private string _model = "gpt-3.5-turbo-1106";

        /// <summary>
        /// Ключ API для OpenAI
        /// </summary>
        public string ApiKey
        {
            private get => _apiKey;
            set
            {
                _apiKey = value;
                Client = new ChatClient(Model, new ApiKeyCredential(ApiKey),
                    new OpenAIClientOptions
                    {
                        Endpoint = new Uri(Endpoint),
                        NetworkTimeout = TimeSpan.FromMinutes(30)
                    });
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
                Client = new ChatClient(Model, new ApiKeyCredential(ApiKey),
                    new OpenAIClientOptions
                    {
                        Endpoint = new Uri(Endpoint),
                        NetworkTimeout = TimeSpan.FromMinutes(30)
                    });
            }
        }

        /// <summary>
        /// Использовать структурированный вывод
        /// </summary>
        public bool UseStructuredOutput { get; set; } = true;

        /// <summary>
        /// Получить ключ API из переменной окружения
        /// </summary>
        /// <param name="envVar">Имя переменной окружения</param>
        public void GetApiKeyFromEnvironment(string envVarName)
        {
            string envVar = Environment.GetEnvironmentVariable(envVarName);
            if (envVar != null)
            {
                ApiKey = envVar;
            }
        }

        /// <summary>
        /// Клиент OpenAI
        /// </summary>
        public ChatClient Client
        {
            get => _client;
            set => _client = value;
        }

        /// <summary>
        /// Модель для генерации текста
        /// </summary>
        public string Model
        {
            get => _model;
            set 
            {
                _model = value;
                Client = new ChatClient(Model, new ApiKeyCredential(ApiKey),
                    new OpenAIClientOptions
                    {
                        Endpoint = new Uri(Endpoint),
                        NetworkTimeout = TimeSpan.FromMinutes(30)
                    });
            }
        }

        /// <summary>
        /// Генерация текста
        /// </summary>
        /// <param name="messages">Список сообщений. 
        /// Если UseStructuredOutput - true, то первое сообщение используется 
        /// для структурированного вывода.</param>
        /// <returns>Сгенерированный текст</returns>
        /// <exception cref="Exception">Ошибка генерации текста</exception>
        public async Task<string> GenerateTextAsync(List<string> messages)
        {
            ChatCompletionOptions options = new();
            if (UseStructuredOutput)
            {
                options = new ChatCompletionOptions()
                {
                    ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                        jsonSchemaFormatName: "AiElement",
                        jsonSchema: BinaryData.FromString(messages.First()),
                        jsonSchemaIsStrict: true)
                };
                messages.RemoveAt(0);
            }
            var completion = await Client.CompleteChatAsync(messages.Select(
                message => ChatMessage.CreateUserMessage(message)), options);
            if (completion.Value.FinishReason == ChatFinishReason.Stop)
            {
                string trimmedResult = completion.Value.Content.First().Text;
                trimmedResult = Regex.Match(trimmedResult, 
                                            @"\{.*\}", RegexOptions.Singleline).Value;
                trimmedResult = trimmedResult.Replace("\n\n", "");
                if (trimmedResult == "")
                {
                    throw new Exception("Failed to generate text: " + 
                        completion.Value.Content.First().Text);
                }
                return trimmedResult;
            }
            else
            {
                throw new Exception("Failed to generate text: " + completion.Value.FinishReason);
            }
        }

        /// <summary>
        /// Стандартный конструктор с ключом OpenAI API из переменной окружения
        /// </summary>
        public OpenAIGenerator()
        {
            GetApiKeyFromEnvironment("OpenAIAPIKey");
        }

        /// <summary>
        /// Конструктор с ключом API из переменной окружения и адресом API
        /// </summary>
        /// <param name="keyEnvVar">Имя переменной окружения с ключом API</param>
        /// <param name="endpoint">Адрес API</param>
        public OpenAIGenerator(string keyEnvVar, string endpoint)
        {
            GetApiKeyFromEnvironment(keyEnvVar);
            Endpoint = endpoint;
        }
    }
}
