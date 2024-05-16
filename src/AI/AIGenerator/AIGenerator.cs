using AIGenerator.TextGenerator;
using BaseClasses.Interface;
using BaseClasses.Model;
using Newtonsoft.Json;
using BaseClasses.Services;

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
		/// Настройки сериализации
		/// </summary>
		public JsonSerializerSettings settings = new JsonSerializerSettings
		{
			NullValueHandling = NullValueHandling.Ignore,
			DefaultValueHandling = DefaultValueHandling.Ignore
        };

		public bool AIPriority { get; set; } = false;

		private static readonly Dictionary<Type, Type> Classes = new Dictionary<Type, Type>
		{
			{ typeof(Character), typeof(AICharacter) },
			{ typeof(Location), typeof(AILocation) },
			{ typeof(Item), typeof(AIItem) },
			{ typeof(Event), typeof(AIEvent) }
        };

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

		private IPart Merge(IPart preparedPart, IPart aiPart)
		{
            if (!preparedPart.IsEmpty())
            {
                if (AIPriority)
                {
                    aiPart.Merge(preparedPart);
					return aiPart;
                }
                else
                {
                    preparedPart.Merge(aiPart);
                    return preparedPart;
                }
            }
			return aiPart;
        }

		/// <summary>
		/// Получение подсказок для генерации
		/// </summary>
		/// <param name="task">Необходимый элемент истории</param>
		/// <param name="plot">История</param>
		/// <returns>Список подсказок</returns>
		private List<string> GetPromptForResponse(Type type, Plot plot, IPart part)
		{
			List<string> prompts = new List<string>
			{
				SystemPrompt["Setting"],
				SystemPrompt[$"{type.Name}Start"]
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
					character.Relations.Select(kv => $"{kv.Character.Name}: {kv.Value}")),
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
			if (!part.IsEmpty())
			{
				switch (part)
				{
					case Character c:
						{
							prompts.Add(string.Format(SystemPrompt["CharacterPrepared"],
								JsonConvert.SerializeObject(new AICharacter(c), settings)));
							break;
						}
					case Location l:
						{
							prompts.Add(string.Format(SystemPrompt["LocationPrepared"],
								JsonConvert.SerializeObject(new AILocation(l), settings)));
							break;
						}
					case Item i:
						{
							prompts.Add(string.Format(SystemPrompt["ItemPrepared"],
								JsonConvert.SerializeObject(new AIItem(i), settings)));
							break;
						}
					case Event e:
						{
							prompts.Add(string.Format(SystemPrompt["EventPrepared"],
								JsonConvert.SerializeObject(new AIEvent(e), settings)));
							break;
						}
				}
			}
			prompts.Add(SystemPrompt[$"{type.Name}End"]);
			return prompts;
		}

		/// <summary>
		/// Генерация элемента истории
		/// </summary>
		/// <param name="plot">История</param>
		/// <param name="preparedPart">Подготовленный элемент истории</param>
		/// <returns>Сгенерированный элемент истории</returns>
		/// <exception cref="Exception">Нейросеть вернула недействительный json</exception>
		public async Task<IPart> GenerateAsync(Plot plot, IPart preparedPart)
		{
			List<string> prompts = GetPromptForResponse(preparedPart.GetType(), plot, preparedPart);
			string response = await TextAIGenerator.GenerateTextAsync(prompts);
			try
			{
                object aiPart = JsonConvert.DeserializeObject(response, Classes[preparedPart.GetType()]);
				IPart part;
				switch (aiPart)
				{
					case AICharacter c:
						{
                            part = Merge(preparedPart, (IPart)c.ToBase(plot));
                            plot.Characters.Add(part as Character);
							return part;
                        }
					case AILocation l:
						{
                            part = Merge(preparedPart, (IPart)l.ToBase(plot));
                            plot.Locations.Add(part as Location);
                            return part;
                        }
					case AIItem i:
						{
                            part = Merge(preparedPart, (IPart)i.ToBase(plot));
                            plot.Items.Add(part as Item);
							return part;
                        }
					case AIEvent e:
						{
                            part = Merge(preparedPart, (IPart)e.ToBase(plot));
                            plot.Events.Add(part as Event);
                            return part;
                        }
				}
				return default;
            }
            catch (JsonReaderException e)
			{
                throw new Exception(response, e);
            }
		}

		/// <summary>
		/// Генерация цепочки элементов истории
		/// </summary>
		/// <param name="plot">История</param>
		/// <param name="preparedPart">Подготовленный элемент истории</param>
		/// <param name="generationQueue">Очередь генерации, следует оставить пустым</param>
		/// <param name="recursion">Глубина рекурсии</param>
		/// <returns>Сгенерированный элемент истории</returns>
		/// <exception cref="Exception">Нейросеть вернула недействительный json</exception>
		public async Task<IPart> GenerateChainAsync(Plot plot, 
													IPart preparedPart, 
													Queue<(IPart, IPart, int)> generationQueue = null, 
													int recursion = 3)
		{
			bool isRoot = generationQueue == null;
			if (isRoot) generationQueue = new();
			List<string> prompts = GetPromptForResponse(preparedPart.GetType(), plot, preparedPart);
			string response = await TextAIGenerator.GenerateTextAsync(prompts);
			try
			{
				object aiPart = JsonConvert.DeserializeObject(response, Classes[preparedPart.GetType()]);
				IPart part;
                switch (aiPart)
                {
                    case AICharacter c:
                        {
                            part = Merge(preparedPart, (IPart)c.ToBase(plot));
                            plot.Characters.Add(part as Character);
							break;
                        }
                    case AILocation l:
                        {
                            part = Merge(preparedPart, (IPart)l.ToBase(plot));
                            plot.Locations.Add(part as Location);
							break;
                        }
                    case AIItem i:
                        {
                            part = Merge(preparedPart, (IPart)i.ToBase(plot));
                            plot.Items.Add(part as Item);
                            break;
                        }
                    case AIEvent e:
                        {
                            part = Merge(preparedPart, (IPart)e.ToBase(plot));
                            plot.Events.Add(part as Event);
                            break;
                        }
					default:
						{
							part = default;
							break;
						}
                }
                if (recursion > 0)
				{
					foreach (var (type, list) in (aiPart as IAIClass).NewParts(plot))
					{
						if (type == typeof(Character))
						{
							foreach (string c in list)
							{
								Character newCharacter = new Character()
								{
									Name = c
								};
								Binder.Bind(part, newCharacter, aiPart is AICharacter ? 
									(aiPart as AICharacter).Relations[c] :
									0);
                                generationQueue.Enqueue((newCharacter, part, recursion - 1));
							}
						}
						else if (type == typeof(Location)) {
							foreach (string l in list)
							{
								Location newLocation = new Location()
                                {
                                    Name = l
                                };
								Binder.Bind(part, newLocation);
                                generationQueue.Enqueue((newLocation, part, recursion - 1));
							}
						}
						else if (type == typeof(Item))
						{
							foreach (string i in list)
							{
								Item newItem = new Item()
                                {
                                    Name = i
                                };
								Binder.Bind(part, newItem);
                                generationQueue.Enqueue((newItem, part, recursion - 1));
							}
						}
						else if (type == typeof(Event))
						{
							foreach (string e in list)
							{
								Event newEvent = new Event()
                                {
                                    Name = e
                                };
								Binder.Bind(part, newEvent);
                                generationQueue.Enqueue((newEvent, part, recursion - 1));
							}
						}	
					}
				}
				while (isRoot && generationQueue.Count > 0)
				{
                    var (newPart, parent, rec) = generationQueue.Dequeue();
                    newPart = await GenerateChainAsync(plot, newPart, generationQueue, rec);
					// Можно сделать universal binder в binder
					// Интерфейс для базовых классов

					// По идее все уже связано
					double relation = newPart is Character && parent is Character c ? 
                        (newPart as Character).Relations
						.FirstOrDefault(r => r.Character == parent, new()).Value : 0;
					Binder.Bind(parent, newPart, relation);
                }
				return part;
			}
			catch (JsonReaderException e)
			{
				throw new Exception(response, e);
			}
		}
	}
}
