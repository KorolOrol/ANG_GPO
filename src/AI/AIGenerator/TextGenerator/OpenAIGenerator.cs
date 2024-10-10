using OpenAI;
using OpenAI.Managers;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;
using System.Text.RegularExpressions;

namespace AIGenerator.TextGenerator
{
    /// <summary>
    /// Класс для генерации текста с помощью OpenAI API
    /// </summary>
    public class OpenAIGenerator : ITextAiGenerator
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
        private OpenAIService _client;

        /// <summary>
        /// Модель для генерации текста
        /// </summary>
        private string _model = Models.Gpt_3_5_Turbo_1106;

        /// <summary>
        /// Ключ API для OpenAI
        /// </summary>
        public string ApiKey
        {
            private get => _apiKey;
            set
            {
                _apiKey = value;
                Client = new OpenAIService(new OpenAiOptions()
                {
                    ApiKey = value,
                    BaseDomain = Endpoint
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
                Client = new OpenAIService(new OpenAiOptions()
                {
                    ApiKey = ApiKey,
                    BaseDomain = value
                });
            }
        }

        /// <summary>
        /// Получить ключ API из переменной окружения
        /// </summary>
        /// <param name="envVar">Имя переменной окружения</param>
        public void GetApiKeyFromEnvironment(string envVarName)
        {
            string envVar = Environment.GetEnvironmentVariable(envVarName, EnvironmentVariableTarget.User);
            if (envVar != null)
            {
                ApiKey = envVar;
            }
        }

        /// <summary>
        /// Клиент OpenAI
        /// </summary>
        private OpenAIService Client
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
            set => _model = value;
        }

        public bool TrimEnd { get; set; }

        /// <summary>
        /// Генерация текста
        /// </summary>
        /// <param name="messages">Список сообщений</param>
        /// <returns>Сгенерированный текст</returns>
        /// <exception cref="Exception">Ошибка генерации текста</exception>
        public async Task<string> GenerateTextAsync(List<string> messages)
        {
            var completion = await Client.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest()
            {
                Messages = messages.Select(message => ChatMessage.FromSystem(message)).ToList(),
                Model = Model
            });
            if (completion.Successful)
            {
                string trimmedResult = completion.Choices.First().Message.Content;
                if (TrimEnd) trimmedResult = TrimRepatingEnd(trimmedResult);
                trimmedResult = Regex.Match(trimmedResult, 
                                            @"\{.*\}", RegexOptions.Singleline).Value;
                if (trimmedResult == "")
                {
                    throw new Exception("Failed to generate text: " + 
                        completion.Choices.First().Message.Content);
                }
                return trimmedResult;
            }
            else
            {
                await Task.Delay(5000);
                if (completion.HttpStatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    await Task.Delay(5000);
                    return await GenerateTextAsync(messages);
                }
                throw new Exception("Failed to generate text: " + completion.Error.Message);
            }
        }

        /// <summary>
        /// Стандартный конструктор с ключом OpenAI API из переменной окружения
        /// </summary>
        public OpenAIGenerator()
        {
            GetApiKeyFromEnvironment("OpenAIAPIKey");
            TrimEnd = false;
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
            TrimEnd = true;
        }

        private string TrimRepatingEnd(string input)
        {
            for (int i = (int)Math.Floor((double)input.Length / 2); i > 0 ; i--)
            {
                if (input.Substring(input.Length - i, i) == 
                    input.Substring(input.Length - 2 * i, i))
                {
                    return input.Substring(0, input.Length - i);
                }
            }
            return input;
        }
    }
}
