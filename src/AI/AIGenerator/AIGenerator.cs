using AIGenerator.TextGenerator;
using BaseClasses.Interface;
using BaseClasses.Model;
using Newtonsoft.Json;

namespace AIGenerator
{
    /// <summary>
    /// ИИ-генератор
    /// </summary>
    public class AIGenerator : IGenerator, IChainGenerator
    {
        /// <summary>
        /// Генератор текста
        /// </summary>
        public ITextAIGenerator TextAIGenerator { get; set; }

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
            TextAIGenerator = new OpenAIGenerator();
        }

        /// <summary>
        /// Конструктор с пользовательским ИИ
        /// </summary>
        /// <param name="promptPath">Путь к файлу с подсказками</param>
        /// <param name="apiKey">Переменная среды с ключом API</param>
        /// <param name="endpoint">Переменная среды с конечной точкой</param>
        public AIGenerator(string promptPath, ITextAIGenerator textAIGenerator)
        {
            LoadSystemPrompt(promptPath);
            TextAIGenerator = textAIGenerator;
        }

        /// <summary>
        /// Получение подсказок для генерации
        /// </summary>
        /// <param name="task">Необходимый элемент истории</param>
        /// <param name="plot">История</param>
        /// <returns>Список подсказок</returns>
        private List<string> GetPromptForResponse(string task, Plot plot, string name = "")
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
            if (plot.Locations != null && plot.Locations.Count != 0)
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
            if (plot.Items != null && plot.Items.Count != 0)
            {
                foreach (var item in plot.Items)
                {
                    prompts.Add(string.Format(SystemPrompt["ItemUsage"],
                                              item.Name,
                                              item.Description,
                                              item.Host != null ? item.Host.Name : 
                                                                  SystemPrompt["None"],
                                              item.Location != null ? item.Location.Name : 
                                                                      SystemPrompt["None"],
                                              string.Join(", ", item.Events.Select(e => e.Name))));
                }
            } 
            else
            {
                prompts.Add(SystemPrompt["ItemEmpty"]);
            }
            if (plot.Events != null && plot.Events.Count != 0)
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
            if (name != "")
            {
                prompts.Add(string.Format(SystemPrompt[$"{task}Chain"], name));
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
            string response = await TextAIGenerator.GenerateTextAsync(prompts);
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
            string response = await TextAIGenerator.GenerateTextAsync(prompts);
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
            string response = await TextAIGenerator.GenerateTextAsync(prompts);
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
            string response = await TextAIGenerator.GenerateTextAsync(prompts);
            Event @event = JsonConvert.DeserializeObject<AIEvent>(response)
                                      .ToEvent(plot);
            plot.Events.Add(@event);
            return @event;
        }

        /// <summary>
        /// Генерация персонажа с цепочкой
        /// </summary>
        /// <param name="plot">История</param>
        /// <param name="recursion">Глубина рекурсии</param>
        /// <param name="name">Имя персонажа</param>
        /// <returns>Сгенерированный персонаж</returns>
        public async Task<Character> GenerateCharacterChainAsync(Plot plot, int recursion = 3, string name = "")
        {
            List<string> prompts = GetPromptForResponse("Character", plot, name);
            string response = await TextAIGenerator.GenerateTextAsync(prompts);
            AICharacter character = JsonConvert.DeserializeObject<AICharacter>(response);
            Character returnCharacter = character.ToCharacter(plot);
            plot.Characters.Add(returnCharacter);
            if (recursion > 0)
            {
                foreach (string n in character.NewRelations(plot))
                {
                    Character relation = await GenerateCharacterChainAsync(plot, recursion - 1, n);
                    if (!returnCharacter.Relations.ContainsKey(relation))
                    {
                        returnCharacter.Relations.Add(relation, character.Relations[n]);
                        relation.Relations.Add(returnCharacter, character.Relations[n]);
                    }
                    else
                    {
                        returnCharacter.Relations[relation] = character.Relations[n];
                        relation.Relations[returnCharacter] = character.Relations[n];
                    }
                }
                foreach (string n in character.NewLocations(plot))
                {
                    Location location = await GenerateLocationChainAsync(plot, recursion - 1, n);
                    if (!returnCharacter.Locations.Contains(location))
                        returnCharacter.Locations.Add(location);
                    if (!location.Characters.Contains(returnCharacter))
                        location.Characters.Add(returnCharacter);
                }
                foreach (string n in character.NewItems(plot))
                {
                    Item item = await GenerateItemChainAsync(plot, recursion - 1, n);
                    if (!returnCharacter.Items.Contains(item))
                        returnCharacter.Items.Add(item);
                    if (item.Host != returnCharacter)
                    {
                        item.Host.Items.Remove(item);
                        item.Host = returnCharacter;
                    }
                }
                foreach (string n in character.NewEvents(plot))
                {
                    Event @event = await GenerateEventChainAsync(plot, recursion - 1, n);
                    if (!returnCharacter.Events.Contains(@event))
                        returnCharacter.Events.Add(@event);
                    if (!@event.Characters.Contains(returnCharacter))
                        @event.Characters.Add(returnCharacter);
                }
            }
            return returnCharacter;
        }

        /// <summary>
        /// Генерация локации с цепочкой
        /// </summary>
        /// <param name="plot">История</param>
        /// <param name="recursion">Глубина рекурсии</param>
        /// <param name="name">Имя локации</param>
        /// <returns>Сгенерированная локация</returns>
        public async Task<Location> GenerateLocationChainAsync(Plot plot, int recursion = 3, string name = "")
        {
            List<string> prompts = GetPromptForResponse("Location", plot, name);
            string response = await TextAIGenerator.GenerateTextAsync(prompts);
            AILocation location = JsonConvert.DeserializeObject<AILocation>(response);
            Location returnLocation = location.ToLocation(plot);
            plot.Locations.Add(returnLocation);
            if (recursion > 0)
            {
                foreach (string n in location.NewCharacters(plot))
                {
                    Character character = await GenerateCharacterChainAsync(plot, recursion - 1, n);
                    if (!returnLocation.Characters.Contains(character))
                        returnLocation.Characters.Add(character);
                    if (!character.Locations.Contains(returnLocation))
                        character.Locations.Add(returnLocation);
                }
                foreach (string n in location.NewItems(plot))
                {
                    Item item = await GenerateItemChainAsync(plot, recursion - 1, n);
                    if (!returnLocation.Items.Contains(item))
                        returnLocation.Items.Add(item);
                    if (item.Location != returnLocation)
                    {
                        item.Location.Items.Remove(item);
                        item.Location = returnLocation;
                    }
                }
                foreach (string n in location.NewEvents(plot))
                {
                    Event @event = await GenerateEventChainAsync(plot, recursion - 1, n);
                    if (!returnLocation.Events.Contains(@event))
                        returnLocation.Events.Add(@event);
                    if (!@event.Locations.Contains(returnLocation))
                        @event.Locations.Add(returnLocation);
                }
            }
            return returnLocation;
        }

        /// <summary>
        /// Генерация предмета с цепочкой
        /// </summary>
        /// <param name="plot">История</param>
        /// <param name="recursion">Глубина рекурсии</param>
        /// <param name="name">Имя предмета</param>
        /// <returns>Сгенерированный предмет</returns>
        public async Task<Item> GenerateItemChainAsync(Plot plot, int recursion = 3, string name = "")
        {
            List<string> prompts = GetPromptForResponse("Item", plot, name);
            string response = await TextAIGenerator.GenerateTextAsync(prompts);
            AIItem item = JsonConvert.DeserializeObject<AIItem>(response);
            Item returnItem = item.ToItem(plot);
            plot.Items.Add(returnItem);
            if (recursion > 0)
            {
                if (item.NewHost(plot) != null)
                {
                    Character host = await GenerateCharacterChainAsync(plot, recursion - 1, item.Host);
                    if (returnItem.Host != host)
                    {
                        returnItem.Host.Items.Remove(returnItem);
                        returnItem.Host = host;
                    }
                    if (!host.Items.Contains(returnItem))
                        host.Items.Add(returnItem);
                }
                if (item.NewLocation(plot) != null)
                {
                    Location location = await GenerateLocationChainAsync(plot, recursion - 1, item.Location);
                    if (returnItem.Location != location)
                    {
                        if (returnItem.Location != null)
                            returnItem.Location.Items.Remove(returnItem);
                        returnItem.Location = location;
                    }
                    if (!location.Items.Contains(returnItem))
                        location.Items.Add(returnItem);
                }
                foreach (string n in item.NewEvents(plot))
                {
                    Event @event = await GenerateEventChainAsync(plot, recursion - 1, n);
                    if (!returnItem.Events.Contains(@event))
                        returnItem.Events.Add(@event);
                    if (!@event.Items.Contains(returnItem))
                        @event.Items.Add(returnItem);
                }
            }
            return returnItem;
        }

        /// <summary>
        /// Генерация события с цепочкой
        /// </summary>
        /// <param name="plot">История</param>
        /// <param name="recursion">Глубина рекурсии</param>
        /// <param name="name">Имя события</param>
        /// <returns>Сгенерированное событие</returns>
        public async Task<Event> GenerateEventChainAsync(Plot plot, int recursion = 3, string name = "")
        {
            List<string> prompts = GetPromptForResponse("Event", plot, name);
            string response = await TextAIGenerator.GenerateTextAsync(prompts);
            AIEvent @event = JsonConvert.DeserializeObject<AIEvent>(response);
            Event returnEvent = @event.ToEvent(plot);
            plot.Events.Add(returnEvent);
            if (recursion > 0)
            {
                foreach (string n in @event.NewCharacters(plot))
                {
                    Character character = await GenerateCharacterChainAsync(plot, recursion - 1, n);
                    if (!returnEvent.Characters.Contains(character))
                        returnEvent.Characters.Add(character);
                    if (!character.Events.Contains(returnEvent))
                        character.Events.Add(returnEvent);
                }
                foreach (string n in @event.NewLocations(plot))
                {
                    Location location = await GenerateLocationChainAsync(plot, recursion - 1, n);
                    if (!returnEvent.Locations.Contains(location))
                        returnEvent.Locations.Add(location);
                    if (!location.Events.Contains(returnEvent))
                        location.Events.Add(returnEvent);
                }
                foreach (string n in @event.NewItems(plot))
                {
                    Item item = await GenerateItemChainAsync(plot, recursion - 1, n);
                    if (!returnEvent.Items.Contains(item))
                        returnEvent.Items.Add(item);
                    if (!item.Events.Contains(returnEvent))
                        item.Events.Add(returnEvent);
                }
            }
            return returnEvent;
        }
    }
}
