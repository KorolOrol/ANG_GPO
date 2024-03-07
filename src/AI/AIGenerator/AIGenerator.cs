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
        /// <param name="characters">Список персонажей</param>
        /// <param name="locations">Список локаций</param>
        /// <param name="items">Список предметов</param>
        /// <param name="events">Список событий</param>
        /// <returns>Список подсказок</returns>
        private List<string> GetPromptForResponse(string task, List<Character>? characters, 
                                                  List<Location>? locations, List<Item>? items, 
                                                  List<Event>? events)
        {
            List<string> prompts = new List<string>
            {
                SystemPrompt["Setting"],
                SystemPrompt[$"{task}Start"]
            };
            if (characters != null && characters.Count != 0)
            {
                foreach (var character in characters)
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
            if (locations != null && locations.Count != 0)
            {
                foreach (var location in locations)
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
            if (items != null && items.Count != 0)
            {
                foreach (var item in items)
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
            if (events != null && events.Count != 0)
            {
                foreach (var ev in events)
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
        /// <param name="characters">Персонажи, которые уже есть в истории</param>
        /// <param name="locations">Локации, которые уже есть в истории</param>
        /// <param name="items">Предметы, которые уже есть в истории</param>
        /// <param name="events">События, которые уже есть в истории</param>
        /// <returns>Сгенерированный персонаж</returns>
        public async Task<Character> GenerateCharacterAsync(List<Character>? characters = null, 
                                                            List<Location>? locations = null, 
                                                            List<Item>? items = null, 
                                                            List<Event>? events = null)
        {
            List<string> prompts = GetPromptForResponse("Character", characters, locations, items, events);
            string response = await TextAIGenerator.GenerateText(prompts);
            return JsonConvert.DeserializeObject<AICharacter>(response).ToCharacter(characters, 
                                                                                    locations, 
                                                                                    items, 
                                                                                    events);
        }

        /// <summary>
        /// Генерация локации
        /// </summary>
        /// <param name="characters">Персонажи, которые уже есть в истории</param>
        /// <param name="locations">Локации, которые уже есть в истории</param>
        /// <param name="items">Предметы, которые уже есть в истории</param>
        /// <param name="events">События, которые уже есть в истории</param>
        /// <returns>Сгенерированная локация</returns>
        public async Task<Location> GenerateLocationAsync(List<Character>? characters = null, 
                                                     List<Location>? locations = null, 
                                                     List<Item>? items = null, 
                                                     List<Event>? events = null)
        {
            List<string> prompts = GetPromptForResponse("Location", characters, locations, items, events);
            string response = await TextAIGenerator.GenerateText(prompts);
            return JsonConvert.DeserializeObject<AILocation>(response).ToLocation(characters,
                                                                                  locations,
                                                                                  items,
                                                                                  events);
        }

        /// <summary>
        /// Генерация предмета
        /// </summary>
        /// <param name="characters">Персонажи, которые уже есть в истории</param>
        /// <param name="locations">Локации, которые уже есть в истории</param>
        /// <param name="items">Предметы, которые уже есть в истории</param>
        /// <param name="events">События, которые уже есть в истории</param>
        /// <returns>Сгенерированный предмет</returns>
        public async Task<Item> GenerateItemAsync(List<Character>? characters = null, 
                                             List<Location>? locations = null, 
                                             List<Item>? items = null, 
                                             List<Event>? events = null)
        {
            List<string> prompts = GetPromptForResponse("Item", characters, locations, items, events);
            string response = await TextAIGenerator.GenerateText(prompts);
            return JsonConvert.DeserializeObject<AIItem>(response).ToItem(characters,
                                                                          locations,
                                                                          items,
                                                                          events);
        }

        /// <summary>
        /// Генерация события
        /// </summary>
        /// <param name="characters">Персонажи, которые уже есть в истории</param>
        /// <param name="locations">Локации, которые уже есть в истории</param>
        /// <param name="items">Предметы, которые уже есть в истории</param>
        /// <param name="events">События, которые уже есть в истории</param>
        /// <returns>Сгенерированное событие</returns>
        public async Task<Event> GenerateEventAsync(List<Character>? characters = null, 
                                               List<Location>? locations = null, 
                                               List<Item>? items = null, 
                                               List<Event>? events = null)
        {
            List<string> prompts = GetPromptForResponse("Event", characters, locations, items, events);
            string response = await TextAIGenerator.GenerateText(prompts);
            return JsonConvert.DeserializeObject<AIEvent>(response).ToEvent(characters,
                                                                            locations,
                                                                            items,
                                                                            events);
        }
    }
}
