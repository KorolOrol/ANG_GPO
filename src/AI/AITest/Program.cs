using BaseClasses.Model;
using AIGenerator.TextGenerator;
using AIGen = AIGenerator.AIGenerator;
using BaseClasses.Services;

string promptPath = "C:\\Users\\KorolOrol\\Desktop\\TUSUR\\repos\\ANG_GPO\\src\\AI\\AIGenerator\\SystemPromptExample.json";
string savingPath = "C:\\Users\\KorolOrol\\Desktop\\TUSUR\\repos\\ANG_GPO\\src\\AI\\AITest\\SavingPath\\";
AIGen Ngen = new AIGen(promptPath, new OpenAIGenerator("NeuroAPIKey", "https://lk.neuroapi.host"));
Ngen.AIPriority = true;
AIGen Vgen = new AIGen(promptPath, new VisionCraftGenerator());
Vgen.TextAIGenerator.Model = "Mixtral-8x7B-Instruct-v0.1";
AIGen Ogen = new AIGen(promptPath);

AIGen gen = Ngen;

Plot plot = new Plot();

while (true)
{
    Console.WriteLine("11: Создать одного персонажа");
    Console.WriteLine("12: Создать одно место");
    Console.WriteLine("13: Создать один предмет");
    Console.WriteLine("14: Создать одно событие");
    Console.WriteLine("21: Создать цепочку персонажей");
    Console.WriteLine("22: Создать цепочку мест");
    Console.WriteLine("23: Создать цепочку предметов");
    Console.WriteLine("24: Создать цепочку событий");
    Console.WriteLine("31: Создать цепочку с заготовленным персонажем");
    Console.WriteLine("32: Создать цепочку с заготовленным местом");
    Console.WriteLine("33: Создать цепочку с заготовленным предметом");
    Console.WriteLine("34: Создать цепочку с заготовленным событием");
    Console.WriteLine("01: Вывести всю информацию");
    Console.WriteLine("02: Сохранить в файл");
    Console.WriteLine("03: Загрузить из файла");
    Console.WriteLine("04: Напечатать в файл");
    Console.WriteLine("0: Выход");
    string choise = Console.ReadLine();
    switch (choise)
    {
        case "11":
            {
                Character character = (Character)await gen.GenerateAsync(plot, new Character());
                Console.WriteLine(character.FullInfo());
                break;
            }
        case "12":
            {
                Location location = (Location)await gen.GenerateAsync(plot, new Location());
                Console.WriteLine(location.FullInfo());
                break;
            }
        case "13":
            {
                Item item = (Item)await gen.GenerateAsync(plot, new Item());
                Console.WriteLine(item.FullInfo());
                break;
            }
        case "14":
            {
                Event ev = (Event)await gen.GenerateAsync(plot, new Event());
                Console.WriteLine(ev.FullInfo());
                break;
            }
        case "21":
            {
                Character character = (Character)await gen.GenerateChainAsync(plot, new Character());
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case "22":
            {
                Location location = (Location)await gen.GenerateChainAsync(plot, new Location());
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case "23":
            {
                Item item = (Item)await gen.GenerateChainAsync(plot, new Item());
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case "24":
            {
                Event @event = (Event)await gen.GenerateChainAsync(plot, new Event());
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case "31":
            {
                Character preparedCharacter = new Character();
                preparedCharacter.Name = Console.ReadLine();
                preparedCharacter.Description = Console.ReadLine();
                Location foundLocation = plot.Locations.FirstOrDefault(l => l.Name == Console.ReadLine());
                if (foundLocation != null)
                {
                    preparedCharacter.Locations.Add(foundLocation);
                }
                Character character = (Character)await gen.GenerateChainAsync(plot, preparedCharacter);
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case "32":
            {
                Location preparedLocation = new Location();
                preparedLocation.Name = Console.ReadLine();
                preparedLocation.Description = Console.ReadLine();
                Character foundCharacter = plot.Characters.FirstOrDefault(c => c.Name == Console.ReadLine());
                if (foundCharacter != null)
                {
                    preparedLocation.Characters.Add(foundCharacter);
                }
                Location location = (Location)await gen.GenerateChainAsync(plot, preparedLocation);
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case "33":
            {
                Item preparedItem = new Item();
                preparedItem.Name = Console.ReadLine();
                preparedItem.Description = Console.ReadLine();
                preparedItem.Location = 
                    plot.Locations.FirstOrDefault(l => l.Name == Console.ReadLine());
                Item item = (Item)await gen.GenerateChainAsync(plot, preparedItem);
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case "34":
            {
                Event preparedEvent = new Event();
                preparedEvent.Name = Console.ReadLine();
                preparedEvent.Description = Console.ReadLine();
                Location foundLocation = plot.Locations.FirstOrDefault(l => l.Name == Console.ReadLine());
                if (foundLocation != null)
                {
                    preparedEvent.Locations.Add(foundLocation);
                }
                Event ev = (Event)await gen.GenerateChainAsync(plot, preparedEvent);
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case "01":
            Console.WriteLine(plot.FullInfo());
            break;
        case "02":
            {
                string name = Console.ReadLine();
                Serializer.Serialize(plot, savingPath + name + ".txt");
                break;
            }
        case "03":
            {
                string name = Console.ReadLine();
                plot = Serializer.Deserialize<Plot>(savingPath + name + ".txt");
                break;
            }
        case "04":
            {
                string name = Console.ReadLine();
                Serializer.Print(plot, savingPath + name + ".txt");
                break;
            }
        case "0":
            return;
    }
}