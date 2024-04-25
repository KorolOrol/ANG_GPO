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

		/*

		/// <summary>
		/// Генерация персонажа
		/// </summary>
		/// <param name="plot">История</param>
		/// <returns>Сгенерированный персонаж</returns>
		public async Task<Character> GenerateCharacterAsync(Plot plot)
		{
			List<string> prompts = GetPromptForResponse(typeof(Character), plot);
			string response = await TextAIGenerator.GenerateTextAsync(prompts);
			try
			{
				Character character = JsonConvert.DeserializeObject<AICharacter>(response)
												 .ToCharacter(plot);
				plot.Characters.Add(character);
				return character;
			}
			catch (JsonReaderException e)
			{
				throw new Exception(response, e);
			}
		}

		/// <summary>
		/// Генерация локации
		/// </summary>
		/// <param name="plot">История</param>
		/// <returns>Сгенерированная локация</returns>
		public async Task<Location> GenerateLocationAsync(Plot plot)
		{
			List<string> prompts = GetPromptForResponse(typeof(Location), plot);
			string response = await TextAIGenerator.GenerateTextAsync(prompts);
			try
			{
				Location location = JsonConvert.DeserializeObject<AILocation>(response)
											   .ToLocation(plot);
				plot.Locations.Add(location);
				return location;
			}
			catch (JsonReaderException e)
			{
				throw new Exception(response, e);
			}
		}

		/// <summary>
		/// Генерация предмета
		/// </summary>
		/// <param name="plot">История</param>
		/// <returns>Сгенерированный предмет</returns>
		public async Task<Item> GenerateItemAsync(Plot plot)
		{
			List<string> prompts = GetPromptForResponse(typeof(Item), plot);
			string response = await TextAIGenerator.GenerateTextAsync(prompts);
			try
			{
				Item item = JsonConvert.DeserializeObject<AIItem>(response)
									   .ToItem(plot);
				plot.Items.Add(item);
				return item;
			}
			catch (JsonReaderException e)
			{
				throw new Exception(response, e);
			}
		}

		/// <summary>
		/// Генерация события
		/// </summary>
		/// <param name="plot">История</param>
		/// <returns>Сгенерированное событие</returns>
		public async Task<Event> GenerateEventAsync(Plot plot)
		{
			List<string> prompts = GetPromptForResponse(typeof(Event), plot);
			string response = await TextAIGenerator.GenerateTextAsync(prompts);
			try
			{
				Event @event = JsonConvert.DeserializeObject<AIEvent>(response)
										  .ToEvent(plot);
				plot.Events.Add(@event);
				return @event;
			}
			catch (JsonReaderException e)
			{
				throw new Exception(response, e);
			}
		}

		*/
		
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
								generationQueue.Enqueue((new Character()
								{
									Name = c,
									Relations = new List<Relation>
									{
										new Relation()
										{
											Character = part as Character,
											Value = (aiPart as AICharacter).Relations[c]
										}
									}
                                }, part, recursion - 1));
							}
						}
						else if (type == typeof(Location)) {
							foreach (string l in list)
							{
								generationQueue.Enqueue((new Location()
								{
									Name = l,
									Characters = new List<Character>
									{
										part as Character
									}
								}, part, recursion - 1));
							}
						}
						else if (type == typeof(Item))
						{
							foreach (string i in list)
							{
								generationQueue.Enqueue((new Item()
								{
									Name = i,
									Host = part as Character
								}, part, recursion - 1));
							}
						}
						else if (type == typeof(Event))
						{
							foreach (string e in list)
							{
								generationQueue.Enqueue((new Event()
								{
									Name = e,
									Characters = new List<Character>
									{
                                        part as Character
									}
								}, part, recursion - 1));
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

		/*
		/// <summary>
		/// Генерация персонажа с цепочкой
		/// </summary>
		/// <param name="plot">История</param>
		/// <param name="recursion">Глубина рекурсии</param>
		/// <param name="name">Имя персонажа</param>
		/// <returns>Сгенерированный персонаж</returns>
		public async Task<Character> GenerateCharacterChainAsync(Plot plot, 
																 Character preparedCharacter = null,
																 int recursion = 3)
		{
			List<string> prompts = GetPromptForResponse(typeof(Character), plot, preparedCharacter);
			string response = await TextAIGenerator.GenerateTextAsync(prompts);
			try
			{
				AICharacter character = JsonConvert.DeserializeObject<AICharacter>(response);
				Character returnCharacter = null;
				if (preparedCharacter != null)
				{
					if (AIPriority)
					{
						returnCharacter = character.ToCharacter(plot);
						returnCharacter.Merge(preparedCharacter);
					}
					else
					{
						preparedCharacter.Merge(character.ToCharacter(plot));
						returnCharacter = preparedCharacter;
					}
				}
				else
				{
					returnCharacter = character.ToCharacter(plot);
				}
				plot.Characters.Add(returnCharacter);
				if (recursion > 0)
				{
					foreach (string n in character.NewRelations(plot))
					{
						Character newPreparedCharacter = new Character()
						{
							Name = n,
							Relations = new List<Relation>
							{
								new Relation()
								{
									Character = returnCharacter,
									Value = character.Relations[n]
								}
							}
						};
						Character relation = await GenerateCharacterChainAsync(plot,
																			   newPreparedCharacter,
																			   recursion - 1);
						Binder.Bind(returnCharacter, relation, character.Relations[n]);
					}
					foreach (string n in character.NewLocations(plot))
					{
						Location newPreparedLocation = new Location()
						{
							Name = n,
							Characters = new List<Character>()
							{
								returnCharacter
							}
						};
						Location location = await GenerateLocationChainAsync(plot,
																			 newPreparedLocation, 
																			 recursion - 1);
						Binder.Bind(returnCharacter, location);
					}
					foreach (string n in character.NewItems(plot))
					{
						Item newPreparedItem = new Item()
						{
							Name = n,
							Host = returnCharacter
						};
						Item item = await GenerateItemChainAsync(plot,
																 newPreparedItem, 
																 recursion - 1);
						Binder.Bind(returnCharacter, item);
					}
					foreach (string n in character.NewEvents(plot))
					{
						Event newPreparedEvent = new Event()
						{
							Name = n,
							Characters = new List<Character>()
							{
								returnCharacter
							}
						};
						Event @event = await GenerateEventChainAsync(plot, 
																	 newPreparedEvent,
																	 recursion - 1);
						Binder.Bind(returnCharacter, @event);
					}
				}
				return returnCharacter;
			}
			catch (JsonReaderException e)
			{
				throw new Exception(response, e);
			}
		}

		/// <summary>
		/// Генерация локации с цепочкой
		/// </summary>
		/// <param name="plot">История</param>
		/// <param name="recursion">Глубина рекурсии</param>
		/// <param name="name">Имя локации</param>
		/// <returns>Сгенерированная локация</returns>
		public async Task<Location> GenerateLocationChainAsync(Plot plot,
															   Location preparedLocation = null, 
															   int recursion = 3)
		{
			List<string> prompts = GetPromptForResponse(typeof(Location), plot, preparedLocation);
			string response = await TextAIGenerator.GenerateTextAsync(prompts);
			try
			{
				AILocation location = JsonConvert.DeserializeObject<AILocation>(response);
				Location returnLocation = null;
				if (preparedLocation != null)
				{
					if (AIPriority)
					{
						returnLocation = location.ToLocation(plot);
						returnLocation.Merge(preparedLocation);
					}
					else
					{
						preparedLocation.Merge(location.ToLocation(plot));
						returnLocation = preparedLocation;
					}
				}
				else
				{
					returnLocation = location.ToLocation(plot);
				}
				plot.Locations.Add(returnLocation);
				if (recursion > 0)
				{
					foreach (string n in location.NewCharacters(plot))
					{
						Character newPreparedCharacter = new Character()
						{
							Name = n,
							Locations = new List<Location>()
							{
								returnLocation
							}
						};
						Character character = await GenerateCharacterChainAsync(plot,
																				newPreparedCharacter, 
																				recursion - 1);
						Binder.Bind(returnLocation, character);
					}
					foreach (string n in location.NewItems(plot))
					{
						Item newPreparedItem = new Item()
						{
							Name = n,
							Location = returnLocation
						};
						Item item = await GenerateItemChainAsync(plot,
																 newPreparedItem, 
																 recursion - 1);
						Binder.Bind(returnLocation, item);
					}
					foreach (string n in location.NewEvents(plot))
					{
						Event newPreparedEvent = new Event()
						{
							Name = n,
							Locations = new List<Location>()
							{
								returnLocation
							}
						};
						Event @event = await GenerateEventChainAsync(plot,
																	 newPreparedEvent, 
																	 recursion - 1);
						Binder.Bind(returnLocation, @event);
					}
				}
				return returnLocation;
			}
			catch (JsonReaderException e)
			{
				throw new Exception(response, e);
			}
		}

		/// <summary>
		/// Генерация предмета с цепочкой
		/// </summary>
		/// <param name="plot">История</param>
		/// <param name="recursion">Глубина рекурсии</param>
		/// <param name="name">Имя предмета</param>
		/// <returns>Сгенерированный предмет</returns>
		public async Task<Item> GenerateItemChainAsync(Plot plot,
													   Item preparedItem = null, 
													   int recursion = 3)
		{
			List<string> prompts = GetPromptForResponse(typeof(Item), plot, preparedItem);
			string response = await TextAIGenerator.GenerateTextAsync(prompts);
			try
			{
				AIItem item = JsonConvert.DeserializeObject<AIItem>(response);
				Item returnItem = null;
				if (preparedItem != null)
				{
					if (AIPriority)
					{
						returnItem = item.ToItem(plot);
						returnItem.Merge(preparedItem);
					}
					else
					{
						preparedItem.Merge(item.ToItem(plot));
						returnItem = preparedItem;
					}
				}
				else
				{
					returnItem = item.ToItem(plot);
				}
				plot.Items.Add(returnItem);
				if (recursion > 0)
				{
					if (item.NewHost(plot) != null)
					{
						Character newPreparedCharacter = new Character()
						{
							Name = item.NewHost(plot),
							Items = new List<Item>()
							{
								returnItem
							}
						};
						Character host = await GenerateCharacterChainAsync(plot, 
																		   newPreparedCharacter, 
																		   recursion - 1);
						Binder.Bind(returnItem, host);
					}
					if (item.NewLocation(plot) != null)
					{
						Location newPreparedLocation = new Location()
						{
							Name = item.NewLocation(plot),
							Items = new List<Item>()
							{
								returnItem
							}
						};
						Location location = await GenerateLocationChainAsync(plot,
																			 newPreparedLocation, 
																			 recursion - 1);
						Binder.Bind(returnItem, location);
					}
					foreach (string n in item.NewEvents(plot))
					{
						Event newPreparedEvent = new Event()
						{
							Name = n,
							Items = new List<Item>()
							{
								returnItem
							}
						};
						Event @event = await GenerateEventChainAsync(plot,
																	 newPreparedEvent, 
																	 recursion - 1);
						Binder.Bind(returnItem, @event);
					}
				}
				return returnItem;
			}
			catch (JsonReaderException e)
			{
				throw new Exception(response, e);
			}
		}

		/// <summary>
		/// Генерация события с цепочкой
		/// </summary>
		/// <param name="plot">История</param>
		/// <param name="recursion">Глубина рекурсии</param>
		/// <param name="name">Имя события</param>
		/// <returns>Сгенерированное событие</returns>
		public async Task<Event> GenerateEventChainAsync(Plot plot,
														 Event preparedEvent = null, 
														 int recursion = 3)
		{
			List<string> prompts = GetPromptForResponse(typeof(Event), plot, preparedEvent);
			string response = await TextAIGenerator.GenerateTextAsync(prompts);
			try
			{
				AIEvent @event = JsonConvert.DeserializeObject<AIEvent>(response);
				Event returnEvent = null;
				if (preparedEvent != null)
				{
					if (AIPriority)
					{
						returnEvent = @event.ToEvent(plot);
						returnEvent.Merge(preparedEvent);
					}
					else
					{
						preparedEvent.Merge(@event.ToEvent(plot));
						returnEvent = preparedEvent;
					}
				}
				else
				{
					returnEvent = @event.ToEvent(plot);
				}
				plot.Events.Add(returnEvent);
				if (recursion > 0)
				{
					foreach (string n in @event.NewCharacters(plot))
					{
						Character newPreparedCharacter = new Character()
						{
							Name = n,
							Events = new List<Event>()
							{
								returnEvent
							}
						};
						Character character = await GenerateCharacterChainAsync(plot,
																				newPreparedCharacter, 
																				recursion - 1);
						Binder.Bind(returnEvent, character);
					}
					foreach (string n in @event.NewLocations(plot))
					{
						Location newPreparedLocation = new Location()
						{
							Name = n,
							Events = new List<Event>()
							{
								returnEvent
							}
						};
						Location location = await GenerateLocationChainAsync(plot,
																			 newPreparedLocation, 
																			 recursion - 1);
						Binder.Bind(returnEvent, location);
					}
					foreach (string n in @event.NewItems(plot))
					{
						Item newPreparedItem = new Item()
						{
							Name = n,
							Events = new List<Event>()
							{
								returnEvent
							}
						};
						Item item = await GenerateItemChainAsync(plot,
																 newPreparedItem, 
																 recursion - 1);
						Binder.Bind(returnEvent, item);
					}
				}
				return returnEvent;
			}
			catch (JsonReaderException e)
			{
				throw new Exception(response, e);
			}
		}
		*/
	}
}
