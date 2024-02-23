using BaseClasses.Interface;
using BaseClasses.Model;
using Newtonsoft.Json;

namespace AIGenerator
{
    public class AIGenerator : IGenerator
    {
        public TextAIGenerator TextAIGenerator { get; set; }

        public Dictionary<string, string> SystemPrompt { get; set; }

        public void LoadSystemPrompt(string path)
        {
            SystemPrompt = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));
        }

        public AIGenerator(string promptPath)
        {
            LoadSystemPrompt(promptPath);
            TextAIGenerator = new TextAIGenerator();
        }

        public AIGenerator(string promptPath, string apiKey, string endpoint)
        {
            LoadSystemPrompt(promptPath);
            TextAIGenerator = new TextAIGenerator(apiKey, endpoint);
        }

        private List<string> GetPromptForResponse(string task, List<Character>? characters, List<Location>? locations, List<Item>? items, List<Event>? events)
        {
            List<string> prompts = new List<string>
            {
                SystemPrompt[$"{task}Start"],
                SystemPrompt["Setting"]
            };
            if (characters != null)
            {
                prompts.Add(SystemPrompt["CharacterUsage"]);
                foreach (var character in characters)
                {
                    prompts.Add(string.Format(SystemPrompt["CharacterUsage"],
                                              character.Name,
                                              character.Description,
                                              character.Temper));
                }
            }
            if (locations != null)
            {
                prompts.Add(SystemPrompt["LocationUsage"]);
                foreach (var location in locations)
                {
                    prompts.Add(string.Format(SystemPrompt["LocationUsage"],
                                              location.Name,
                                              location.Description));
                }
            }
            if (items != null)
            {
                prompts.Add(SystemPrompt["ItemUsage"]);
                foreach (var item in items)
                {
                    prompts.Add(string.Format(SystemPrompt["ItemUsage"],
                                              item.Name,
                                              item.Description));
                }
            }
            if (events != null)
            {
                prompts.Add(SystemPrompt["EventUsage"]);
                foreach (var ev in events)
                {
                    prompts.Add(string.Format(SystemPrompt["EventUsage"],
                                              ev.Name,
                                              ev.Description));
                }
            }
            prompts.Add(SystemPrompt[$"{task}End"]);
            return prompts;
        }

        public async Task<Character> GenerateCharacterAsync(List<Character>? characters = null, List<Location>? locations = null, List<Item>? items = null, List<Event>? events = null)
        {
            List<string> prompts = GetPromptForResponse("Character", characters, locations, items, events);
            string response = await TextAIGenerator.GenerateText(prompts);
            return JsonConvert.DeserializeObject<Character>(response);
        }

        public async Task<Location> GenerateLocation(List<Character>? characters = null, List<Location>? locations = null, List<Item>? items = null, List<Event>? events = null)
        {
            List<string> prompts = GetPromptForResponse("Location", characters, locations, items, events);
            string response = await TextAIGenerator.GenerateText(prompts);
            return JsonConvert.DeserializeObject<Location>(response);
        }

        public async Task<Item> GenerateItem(List<Character>? characters = null, List<Location>? locations = null, List<Item>? items = null, List<Event>? events = null)
        {
            List<string> prompts = GetPromptForResponse("Item", characters, locations, items, events);
            string response = await TextAIGenerator.GenerateText(prompts);
            return JsonConvert.DeserializeObject<Item>(response);
        }

        public async Task<Event> GenerateEvent(List<Character>? characters = null, List<Location>? locations = null, List<Item>? items = null, List<Event>? events = null)
        {
            List<string> prompts = GetPromptForResponse("Event", characters, locations, items, events);
            string response = await TextAIGenerator.GenerateText(prompts);
            return JsonConvert.DeserializeObject<Event>(response);
        }
    }
}
