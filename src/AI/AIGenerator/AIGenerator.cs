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
                                              string.Join(", ", character.Traits)));
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
        public async Task<Location> GenerateLocation(List<Character>? characters = null, 
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
        public async Task<Item> GenerateItem(List<Character>? characters = null, 
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
        public async Task<Event> GenerateEvent(List<Character>? characters = null, 
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
