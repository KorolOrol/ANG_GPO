using BaseClasses.Model;
using BaseClasses.Enum;
using BaseClasses.Services;
using AIGenerator;
using AIGenerator.TextGenerator;

/*
OpenAIGenerator text = new OpenAIGenerator("NeuroAPIKey", "https://neuroapi.host");
text.Model = "gpt-3.5-turbo-0125";
List<string> list = new List<string>() {"Привет"};
Console.WriteLine(await text.GenerateTextAsync(list));
*/

/*
Element c = new Element(ElemType.Character, "Вася", "Вася Пупкин");
Element c2 = new Element(ElemType.Character, "Петя", "");
Element c3 = new Element(ElemType.Character, "Коля", "");
Element c4 = new Element(ElemType.Character, "Саша", "");
Binder.Bind(c, c2, 10);
Binder.Bind(c, c3, 20);
Binder.Bind(c, c4, 30);

Serializer.Serialize(c, "relTest.txt");
*/

/*
LlmAiGenerator Ngen = new(promptPath, new OpenAIGenerator("NeuroAPIKey", "https://neuroapi.host/v1/"));
Ngen.AIPriority = true;
Ngen.TextAiGenerator.Model = "gpt-4o-mini";
((OpenAIGenerator)Ngen.TextAiGenerator).TrimEnd = false;
LlmAiGenerator Ogen = new(promptPath);
*/

string promptPath = "C:\\Users\\KorolOrol\\Desktop\\TUSUR\\ANG_GPO\\src\\AI\\AIGenerator\\SystemPromptExample.json";
string savingPath = "C:\\Users\\KorolOrol\\Desktop\\TUSUR\\ANG_GPO\\src\\AI\\AITest\\SavingPath\\";
LlmAiGenerator server = new(promptPath);
server.TextAiGenerator.Endpoint = "http://127.0.0.1:1234/v1/";
server.TextAiGenerator.Model = "saiga_nemo_12b_gguf";
server.AIPriority = true;
server.UseStructuredOutput = true;


LlmAiGenerator gen = server;

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
    string choice = Console.ReadLine()!;
    switch (choice)
    {
        case "11":
            {
                Element character = 
                    (Element)await gen.GenerateAsync(plot, new Element(ElemType.Character));
                Console.WriteLine(character.FullInfo());
                break;
            }
        case "12":
            {
                Element location = 
                    (Element)await gen.GenerateAsync(plot, new Element(ElemType.Location));
                Console.WriteLine(location.FullInfo());
                break;
            }
        case "13":
            {
                Element item = 
                    (Element)await gen.GenerateAsync(plot, new Element(ElemType.Item));
                Console.WriteLine(item.FullInfo());
                break;
            }
        case "14":
            {
                Element ev = 
                    (Element)await gen.GenerateAsync(plot, new Element(ElemType.Event));
                Console.WriteLine(ev.FullInfo());
                break;
            }
        case "21":
            {
                Element character = 
                    (Element)await gen.GenerateChainAsync(plot, new Element(ElemType.Character), recursion: 2);
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case "22":
            {
                Element location = 
                    (Element)await gen.GenerateChainAsync(plot, new Element(ElemType.Location));
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case "23":
            {
                Element item = 
                    (Element)await gen.GenerateChainAsync(plot, new Element(ElemType.Item));
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case "24":
            {
                Element @event = 
                    (Element)await gen.GenerateChainAsync(plot, new Element(ElemType.Event));
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case "31":
            {
                Element preparedCharacter = new Element(ElemType.Character);
                preparedCharacter.Name = Console.ReadLine()!;
                preparedCharacter.Description = Console.ReadLine()!;
                Element? foundLocation = 
                    (Element?)plot.Locations.FirstOrDefault(l => l.Name == Console.ReadLine());
                if (foundLocation != null)
                {
                    Binder.Bind(preparedCharacter, foundLocation);
                }
                Element character = (Element)await gen.GenerateChainAsync(plot, preparedCharacter);
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case "32":
            {
                Element preparedLocation = new Element(ElemType.Location);
                preparedLocation.Name = Console.ReadLine()!;
                preparedLocation.Description = Console.ReadLine()!;
                Element? foundCharacter = 
                    (Element?)plot.Characters.FirstOrDefault(c => c.Name == Console.ReadLine());
                if (foundCharacter != null)
                {
                    Binder.Bind(preparedLocation, foundCharacter);
                }
                Element location = (Element)await gen.GenerateChainAsync(plot, preparedLocation);
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case "33":
            {
                Element preparedItem = new Element(ElemType.Item);
                preparedItem.Name = Console.ReadLine()!;
                preparedItem.Description = Console.ReadLine()!;
                Element? foundLocation = 
                    (Element?)plot.Locations.FirstOrDefault(l => l.Name == Console.ReadLine());
                if (foundLocation != null)
                {
                    Binder.Bind(preparedItem, foundLocation);
                }
                Element item = (Element)await gen.GenerateChainAsync(plot, preparedItem);
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case "34":
            {
                Element preparedEvent = new Element(ElemType.Event);
                preparedEvent.Name = Console.ReadLine()!;
                preparedEvent.Description = Console.ReadLine()!;
                Element? foundLocation = 
                    (Element?)plot.Locations.FirstOrDefault(l => l.Name == Console.ReadLine());
                if (foundLocation != null)
                {
                    Binder.Bind(preparedEvent, foundLocation);
                }
                Element ev = (Element)await gen.GenerateChainAsync(plot, preparedEvent);
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case "01":
            Console.WriteLine(plot.FullInfo());
            break;
        case "02":
            {
                string name = Console.ReadLine()!;
                Serializer.Serialize(plot, savingPath + name + ".txt");
                break;
            }
        case "03":
            {
                string name = Console.ReadLine()!;
                plot = Serializer.Deserialize<Plot>(savingPath + name + ".txt");
                break;
            }
        case "04":
            {
                string name = Console.ReadLine()!;
                Serializer.Print(plot, savingPath + name + ".txt");
                break;
            }
        case "0":
            return;
    }
}
