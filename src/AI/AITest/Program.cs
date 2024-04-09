using BaseClasses.Model;
using AIGenerator.TextGenerator;
using AIGen = AIGenerator.AIGenerator;
using BaseClasses.Services;

string promptPath = "C:\\Users\\KorolOrol\\Desktop\\TUSUR\\repos\\ANG_GPO\\src\\AI\\AIGenerator\\SystemPromptExample.json";
string savingPath = "C:\\Users\\KorolOrol\\Desktop\\TUSUR\\repos\\ANG_GPO\\src\\AI\\AITest\\SavingPath\\";
AIGen Ngen = new AIGen(promptPath, new OpenAIGenerator("NeuroAPIKey", "https://lk.neuroapi.host"));
AIGen Vgen = new AIGen(promptPath, new VisionCraftGenerator());
Vgen.TextAIGenerator.Model = "Mixtral-8x7B-Instruct-v0.1";
AIGen Ogen = new AIGen(promptPath);

AIGen gen = Ngen;

Plot plot = new Plot();

while (true)
{
    Console.WriteLine("1: Создать одного персонажа");
    Console.WriteLine("2: Создать одно место");
    Console.WriteLine("3: Создать один предмет");
    Console.WriteLine("4: Создать одно событие");
    Console.WriteLine("5: Создать цепочку персонажей");
    Console.WriteLine("6: Создать цепочку мест");
    Console.WriteLine("7: Создать цепочку предметов");
    Console.WriteLine("8: Создать цепочку событий");
    Console.WriteLine("9: Вывести всю информацию");
    Console.WriteLine("10: Сохранить в файл");
    Console.WriteLine("11: Загрузить из файла");
    Console.WriteLine("12: Напечатать в файл");
    Console.WriteLine("0: Выход");
    string choise = Console.ReadLine();
    switch (choise)
    {
        case "1":
            {
                Character character = await gen.GenerateCharacterAsync(plot);
                Console.WriteLine(character.FullInfo());
                break;
            }
        case "2":
            {
                Location location = await gen.GenerateLocationAsync(plot);
                Console.WriteLine(location.FullInfo());
                break;
            }
        case "3":
            {
                Item item = await gen.GenerateItemAsync(plot);
                Console.WriteLine(item.FullInfo());
                break;
            }
        case "4":
            {
                Event ev = await gen.GenerateEventAsync(plot);
                Console.WriteLine(ev.FullInfo());
                break;
            }
        case "5":
            {
                Character character = await gen.GenerateCharacterChainAsync(plot);
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case "6":
            {
                Location location = await gen.GenerateLocationChainAsync(plot);
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case "7":
            {
                Item item = await gen.GenerateItemChainAsync(plot);
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case "8":
            {
                Event @event = await gen.GenerateEventChainAsync(plot);
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case "9":
            Console.WriteLine(plot.FullInfo());
            break;
        case "10":
            {
                string name = Console.ReadLine();
                Serializer.Serialize(plot, savingPath + name + ".txt");
                break;
            }
        case "11":
            {
                string name = Console.ReadLine();
                plot = Serializer.Deserialize<Plot>(savingPath + name + ".txt");
                break;
            }
        case "12":
            {
                string name = Console.ReadLine();
                Serializer.Print(plot, savingPath + name + ".txt");
                break;
            }
        case "0":
            return;
    }
}