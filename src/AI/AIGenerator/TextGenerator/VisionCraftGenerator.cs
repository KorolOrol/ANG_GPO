using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AIGenerator.TextGenerator
{
    /// <summary>
    /// Класс для генерации текста с помощью VisionCraft API
    /// </summary>
    public class VisionCraftGenerator : ITextAIGenerator
    {
        /// <summary>
        /// Ключ API для VisionCraft
        /// </summary>
        private string _apiKey = "YOUR_API_KEY_HERE";

        /// <summary>
        /// Адрес API
        /// </summary>
        private string _endpoint = "https://visioncraft.top";

        /// <summary>
        /// Модель для генерации текста
        /// </summary>
        private string _model = "Mixtral-8x7B-Instruct-v0.1";

        /// <summary>
        /// Настройки для генерации текста
        /// </summary>
        private Dictionary<string, double> _settings = new Dictionary<string, double>();

        /// <summary>
        /// Ключ API для VisionCraft
        /// </summary>
        public string ApiKey { 
            private get
            {
                return _apiKey;
            }
            set
            {
                _apiKey = value;
            }
        }

        /// <summary>
        /// Адрес API
        /// </summary>
        public string Endpoint 
        { 
            get
            {
                return _endpoint;
            }
            set
            {
                _endpoint = value;
            }
        }

        /// <summary>
        /// Модель для генерации текста
        /// </summary>
        public string Model 
        {
            get
            {
                return _model;
            }
            set
            {
                _model = value;
            }
        }

        /// <summary>
        /// Настройки для генерации текста
        /// </summary>
        public Dictionary<string, double> Settings
        {
            get
            {
                return _settings;
            }
            set
            {
                _settings = value;
            }
        }

        /// <summary>
        /// Генерация текста
        /// </summary>
        /// <param name="messages">Список сообщений</param>
        /// <returns>Сгенерированный текст</returns>
        public async Task<string> GenerateTextAsync(List<string> messages)
        {
            var completion = new List<Message>();
            foreach (var message in messages)
            {
                completion.Add(new() { role = "user", content = message});
            }
            var data = new
            {
                token = ApiKey,
                model = Model,
                max_new_tokens = Settings["max_new_tokens"],
                temperature = Settings["temperature"],
                top_p = Settings["top_p"],
                top_k = Settings["top_k"],
                repetition_penalty = Settings["repetition_penalty"],
                presence_penalty = Settings["presence_penalty"],
                frequency_penalty = Settings["frequency_penalty"],
                messages = completion
            };
            using (var client = new HttpClient())
            {
                var jsonData = JsonConvert.SerializeObject(data);
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(Endpoint + "/llm", content);
                if (response.IsSuccessStatusCode)
                {
                    Response result = JsonConvert.DeserializeObject<Response>(response.Content.ReadAsStringAsync().Result);
                    return Regex.Match(result.choices.First().message.content, @"\{.*\}", RegexOptions.Singleline).Value;
                }
                else
                {
                    if(response.StatusCode == HttpStatusCode.TooManyRequests)
                    {
                        await Task.Delay(5000);
                        return await GenerateTextAsync(messages);
                    }
                    else
                    {
                        throw new Exception("Failed to generate text: " + response.Content.ReadAsStringAsync().Result);
                    }
                }
            }
        }

        /// <summary>
        /// Получение ключа API из переменной окружения
        /// </summary>
        /// <param name="envVarName">Имя переменной окружения</param>
        public void GetApiKeyFromEnvironment(string envVarName)
        {
            string envVar = Environment.GetEnvironmentVariable(envVarName, EnvironmentVariableTarget.User);
            if (envVar != null)
            {
                ApiKey = envVar;
            }
        }

        /// <summary>
        /// Конструктор класса
        /// </summary>
        public VisionCraftGenerator()
        {
            GetApiKeyFromEnvironment("VisionCraftAPIKey");
            Settings.Add("max_new_tokens", 4096);
            Settings.Add("temperature", 1);
            Settings.Add("top_p", 1);
            Settings.Add("top_k", 0);
            Settings.Add("repetition_penalty", 1);
            Settings.Add("presence_penalty", 0);
            Settings.Add("frequency_penalty", 0);
        }
    }

    /// <summary>
    /// Класс для десериализации ответа от VisionCraft API
    /// </summary>
    internal class Response
    {
        public string model { get; set; }
        public List<Choice> choices { get; set; }
    }

    /// <summary>
    /// Класс для десериализации выбора от VisionCraft API
    /// </summary>
    internal class Choice
    {
        public Message message { get; set; }
        public string finish_reason { get; set; }
    }

    /// <summary>
    /// Класс для десериализации сообщения от VisionCraft API
    /// </summary>
    internal class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }
}
