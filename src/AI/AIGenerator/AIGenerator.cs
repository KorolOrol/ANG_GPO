using BaseClasses.Interface;
using BaseClasses.Model;
using Newtonsoft.Json;

namespace AIGenerator
{
    /// <summary>
    /// ИИ-генератор
    /// </summary>
    public class AIGenerator : IGenerator
    {
        /// <summary>
        /// Генератор текста
        /// </summary>
        public TextAIGenerator TextAIGenerator { get; set; }

        /// <summary>
        /// Системные подсказки
        /// </summary>
        public Dictionary<string, string> SystemPrompt { get; set; }

        /// <summary>
        /// Загрузка системных подсказок
        /// </summary>
        /// <param name="path">Путь к файлу с подсказками</param>
        public void LoadSystemPrompt(string path)
        {
            SystemPrompt = 
                JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(path));
        }

        /// <summary>
        /// Конструктор со стандартным ИИ
        /// </summary>
        /// <param name="promptPath">Путь к файлу с подсказками</param>
        public AIGenerator(string promptPath)
        {
            LoadSystemPrompt(promptPath);
            TextAIGenerator = new TextAIGenerator();
        }

        /// <summary>
        /// Конструктор с пользовательским ИИ
        /// </summary>
        /// <param name="promptPath">Путь к файлу с подсказками</param>
        /// <param name="apiKey">Переменная среды с ключом API</param>
        /// <param name="endpoint">Переменная среды с конечной точкой</param>
        public AIGenerator(string promptPath, string apiKey, string endpoint)
        {
            LoadSystemPrompt(promptPath);
            TextAIGenerator = new TextAIGenerator(apiKey, endpoint);
        }

        /// <summary>
        /// Получение подсказок для генерации
        /// </summary>
        /// <param name="task">Необходимый элемент истории</param>
        /// <param name="plot">История</param>
        /// <returns>Список подсказок</returns>
        private List<string> GetPromptForResponse(string task, Plot plot)
        {
            List<string> prompts = new List<string>
            {
                SystemPrompt["Setting"],
                SystemPrompt[$"{task}Start"]
            };
            if (plot.Characters != null && plot.Characters.Count != 0)
            {
                foreach (var character in plot.Characters)
                {
                    prompts.Add(string.Format(SystemPrompt["CharacterUsage"],
                                              character.Name,
                                              character.Description,
                                              string.Join(", ", character.Traits),
                                              string.Join(", ", 
                    character.Relations.Select(kv => $"{kv.Key.Name}: {kv.Value}")),
                                              string.Join(", ", character.Locations.Select(l => l.Name)),
                                              string.Join(", ", character.Items.Select(i => i.Name)),
                                              string.Join(", ", character.Events.Select(e => e.Name))));
                }
            }
            else
            {
                prompts.Add(SystemPrompt["CharacterEmpty"]);
            }
            if (plot.Characters != null && plot.Characters.Count != 0)
            {
                foreach (var location in plot.Locations)
                {
                    prompts.Add(string.Format(SystemPrompt["LocationUsage"],
                                              location.Name,
                                              location.Description,
                                              string.Join(", ", location.Characters.Select(c => c.Name)),
                                              string.Join(", ", location.Items.Select(i => i.Name)),
                                              string.Join(", ", location.Events.Select(e => e.Name))));
                }
            } 
            else
            {
                prompts.Add(SystemPrompt["LocationEmpty"]);
            }
            if (plot.Characters != null && plot.Characters.Count != 0)
            {
                foreach (var item in plot.Items)
                {
                    prompts.Add(string.Format(SystemPrompt["ItemUsage"],
                                              item.Name,
                                              item.Description,
                                              item.Host.Name,
                                              item.Location.Name,
                                              string.Join(", ", item.Events.Select(e => e.Name))));
                }
            } 
            else
            {
                prompts.Add(SystemPrompt["ItemEmpty"]);
            }
            if (plot.Characters != null && plot.Characters.Count != 0)
            {
                foreach (var ev in plot.Events)
                {
                    prompts.Add(string.Format(SystemPrompt["EventUsage"],
                                              ev.Name,
                                              ev.Description,
                                              string.Join(", ", ev.Characters.Select(c => c.Name)),
                                              string.Join(", ", ev.Locations.Select(l => l.Name)),
                                              string.Join(", ", ev.Items.Select(i => i.Name))));
                }
            } 
            else
            {
                prompts.Add(SystemPrompt["EventEmpty"]);
            }
            prompts.Add(SystemPrompt[$"{task}End"]);
            return prompts;
        }

        /// <summary>
        /// Генерация персонажа
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Сгенерированный персонаж</returns>
        public async Task<Character> GenerateCharacterAsync(Plot plot)
        {
            List<string> prompts = GetPromptForResponse("Character", plot);
            string response = await TextAIGenerator.GenerateText(prompts);
            Character character = JsonConvert.DeserializeObject<AICharacter>(response)
                                             .ToCharacter(plot);
            plot.Characters.Add(character);
            return character;
        }

        /// <summary>
        /// Генерация локации
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Сгенерированная локация</returns>
        public async Task<Location> GenerateLocationAsync(Plot plot)
        {
            List<string> prompts = GetPromptForResponse("Location", plot);
            string response = await TextAIGenerator.GenerateText(prompts);
            Location location = JsonConvert.DeserializeObject<AILocation>(response)
                                           .ToLocation(plot);
            plot.Locations.Add(location);
            return location;
        }

        /// <summary>
        /// Генерация предмета
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Сгенерированный предмет</returns>
        public async Task<Item> GenerateItemAsync(Plot plot)
        {
            List<string> prompts = GetPromptForResponse("Item", plot);
            string response = await TextAIGenerator.GenerateText(prompts);
            Item item = JsonConvert.DeserializeObject<AIItem>(response)
                                   .ToItem(plot);
            plot.Items.Add(item);
            return item;
        }

        /// <summary>
        /// Генерация события
        /// </summary>
        /// <param name="plot">История</param>
        /// <returns>Сгенерированное событие</returns>
        public async Task<Event> GenerateEventAsync(Plot plot)
        {
            List<string> prompts = GetPromptForResponse("Event", plot);
            string response = await TextAIGenerator.GenerateText(prompts);
            Event @event = JsonConvert.DeserializeObject<AIEvent>(response)
                                      .ToEvent(plot);
            return @event;
        }
    }
}
