using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AIGenerator.TextGenerator
{
    public class VisionCraftGenerator : ITextAIGenerator
    {
        private string _apiKey = "YOUR_API_KEY_HERE";
        private string _endpoint = "https://visioncraft.top";
        private string _model = "gpt-4-1106-preview";
        private Dictionary<string, double> _settings = new Dictionary<string, double>();

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

        public string Model 
        {
            get
            {
                return _model;
            }
            set
            {
                using (var client = new HttpClient())
                {
                    var response = client.GetAsync(_endpoint + "/models-llm");

                    List<string> available_models = 
                        JsonConvert.DeserializeObject<List<string>>(response.Result.ToString());
                    
                    if (available_models != null && available_models.Contains(value))
                    {
                        _model = value;
                    }
                }
            }
        }

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

        public async Task<string> GenerateTextAsync(List<string> messages)
        {
            var completion = new List<Message>();
            foreach (var message in messages)
            {
                completion.Add(new() { role = "system", content = message});
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
                    return await GenerateTextAsync(messages);
                }
            }
        }

        public void GetApiKeyFromEnvironment(string envVarName)
        {
            string envVar = Environment.GetEnvironmentVariable(envVarName, EnvironmentVariableTarget.User);
            if (envVar != null)
            {
                ApiKey = envVar;
            }
        }

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

    internal class Message
    {
        public string role { get; set; }
        public string content { get; set; }
    }

    internal class Response
    {
        public string model { get; set; }
        public List<Choice> choices { get; set; }
    }

    internal class Choice
    {
        public Message message { get; set; }
        public string finish_reason { get; set; }
    }
}
