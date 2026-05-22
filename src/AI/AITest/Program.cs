using AIGenerator;
using AIGenerator.DtoProvider;
using AIGenerator.TextGenerator;
using BaseClasses.Enum;
using BaseClasses.Model;
using BaseClasses.Model.Params;
using BaseClasses.Services;
using BaseClasses.Services.Binds;

/*
OpenAIGenerator text = new OpenAIGenerator("NeuroAPIKey", "https://neuroapi.host");
text.Model = "gpt-3.5-turbo-0125";
List<string> list = new List<string>() {"Привет"};
Console.WriteLine(await text.GenerateTextAsync(list));
*/

/*
LlmAiGenerator Ngen = new(promptPath, new OpenAIGenerator("NeuroAPIKey", "https://neuroapi.host/v1/"));
Ngen.AIPriority = true;
Ngen.TextAiGenerator.Model = "gpt-4o-mini";
((OpenAIGenerator)Ngen.TextAiGenerator).TrimEnd = false;
LlmAiGenerator Ogen = new(promptPath);
*/

string promptPath = @"../../../../AIGenerator/ModernSystemPromptExample.json";
string savingPath = "SavingPath/";
LlmAiGenerator server = new(promptPath);
server.TextAiGenerator = new OpenAiGenerator("GHToken", "https://models.github.ai/inference")
{
    UseStructuredOutput = true,
    Model = "openai/gpt-4.1"
};
server.DtoProvider = new SimpleDtoProvider();
server.AiPriority = true;
server.UseStructuredOutput = true;

LlmAiGenerator gen = server;

var traitsKey = new ParamKey<List<string>>("Traits");

var c = new Element(ElemType.Character, "Иван", "Главный герой истории")
{
    Params = { [traitsKey] = new List<string> { "храбрый", "умный" } }
};
var c2 = new Element(ElemType.Character, "Мария", "Второстепенный персонаж")
{
    Params = { [traitsKey] = new List<string> { "добрая", "красивая" } }
};
var l = new Element(ElemType.Location, "Замок", "Старый замок на холме");
var it = new Element(ElemType.Item, "Меч", "Древний меч с рунами");
var e = new Element(ElemType.Event, "Битва", "Эпическая битва между Иваном и драконом");
var plot = new Plot();
plot.Add(c);
plot.Add(c2);
plot.Add(l);
plot.Add(it);
plot.Add(e);
plot.Bind(c, c2, BaseRelationKeys.Relationship, 75);
plot.Bind(c, l, BaseRelationKeys.Located, true);
plot.Bind(c2, l, BaseRelationKeys.Located, true);
plot.Bind(it, l, BaseRelationKeys.Located, true);
plot.Bind(e, c, BaseRelationKeys.Involves, true);
plot.Bind(e, c2, BaseRelationKeys.Involves, true);
plot.Bind(e, it, BaseRelationKeys.Uses, true);

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
    string choice = Console.ReadLine() ?? string.Empty;
    switch (choice)
    {
        case "11":
        {
                Element character = new Element(ElemType.Character);
                var traits = new List<string>();
                for (int i = 0; i < 3; i++)
                {
                    string trait = Console.ReadLine() ?? "";
                    traits.Add(trait);
                }
                character.Params.Set(traitsKey, traits);
                plot.Add(character);
                character = (Element)await gen.GenerateAsync(plot, character);
                Console.WriteLine(Serializer.PrintToString(character));
                break;
            }
        case "12":
            {
                Element location = 
                    (Element)await gen.GenerateAsync(plot, new Element(ElemType.Location));
                Console.WriteLine(Serializer.PrintToString(location));
                break;
            }
        case "13":
            {
                Element item = 
                    (Element)await gen.GenerateAsync(plot, new Element(ElemType.Item));
                Console.WriteLine(Serializer.PrintToString(item));
                break;
            }
        case "14":
            {
                Element ev = 
                    (Element)await gen.GenerateAsync(plot, new Element(ElemType.Event));
                Console.WriteLine(Serializer.PrintToString(ev));
                break;
            }
        case "21":
            {
                Element character = 
                    (Element)await gen.GenerateChainAsync(plot, new Element(ElemType.Character), recursion: 2);
                Console.WriteLine(Serializer.PrintToString(plot));
                break;
            }
        case "22":
            {
                Element location = 
                    (Element)await gen.GenerateChainAsync(plot, new Element(ElemType.Location));
                Console.WriteLine(Serializer.PrintToString(plot));
                break;
            }
        case "23":
            {
                Element item = 
                    (Element)await gen.GenerateChainAsync(plot, new Element(ElemType.Item));
                Console.WriteLine(Serializer.PrintToString(plot));
                break;
            }
        case "24":
            {
                Element @event = 
                    (Element)await gen.GenerateChainAsync(plot, new Element(ElemType.Event));
                Console.WriteLine(Serializer.PrintToString(plot));
                break;
            }
        case "31":
            {
                Element preparedCharacter = new Element(ElemType.Character);
                preparedCharacter.Name = Console.ReadLine()!;
                preparedCharacter.Description = Console.ReadLine()!;
                Element? foundLocation = 
                    (Element?)plot.Elements.FirstOrDefault(l =>
                        l.Name == Console.ReadLine() && l.Type == ElemType.Location);
                if (foundLocation != null)
                {
                    plot.Bind(preparedCharacter, foundLocation, BaseRelationKeys.Located, true);
                }
                Element character = (Element)await gen.GenerateChainAsync(plot, preparedCharacter);
                Console.WriteLine(Serializer.PrintToString(plot));
                break;
            }
        case "32":
            {
                Element preparedLocation = new Element(ElemType.Location);
                preparedLocation.Name = Console.ReadLine()!;
                preparedLocation.Description = Console.ReadLine()!;
                Element? foundCharacter = 
                    (Element?)plot.Elements.FirstOrDefault(c => 
                        c.Name == Console.ReadLine() && c.Type == ElemType.Character);
                if (foundCharacter != null)
                {
                    plot.Bind(preparedLocation, foundCharacter, BaseRelationKeys.Locates, true);
                }
                Element location = (Element)await gen.GenerateChainAsync(plot, preparedLocation);
                Console.WriteLine(Serializer.PrintToString(plot));
                break;
            }
        case "33":
            {
                Element preparedItem = new Element(ElemType.Item);
                preparedItem.Name = Console.ReadLine()!;
                preparedItem.Description = Console.ReadLine()!;
                Element? foundLocation = 
                    (Element?)plot.Elements.FirstOrDefault(l =>
                        l.Name == Console.ReadLine() && l.Type == ElemType.Location);
                if (foundLocation != null)
                {
                    plot.Bind(preparedItem, foundLocation, BaseRelationKeys.Located, true);
                }
                Element item = (Element)await gen.GenerateChainAsync(plot, preparedItem);
                Console.WriteLine(Serializer.PrintToString(plot));
                break;
            }
        case "34":
            {
                Element preparedEvent = new Element(ElemType.Event);
                preparedEvent.Name = Console.ReadLine()!;
                preparedEvent.Description = Console.ReadLine()!;
                Element? foundLocation = 
                    (Element?)plot.Elements.FirstOrDefault(l =>
                        l.Name == Console.ReadLine() && l.Type == ElemType.Location);
                if (foundLocation != null)
                {
                    plot.Bind(preparedEvent, foundLocation, BaseRelationKeys.Located, true);
                }
                Element ev = (Element)await gen.GenerateChainAsync(plot, preparedEvent);
                Console.WriteLine(Serializer.PrintToString(plot));
                break;
            }
        case "01":
            Console.WriteLine(Serializer.PrintToString(plot));
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
