using AIGenerator;

TextAIGenerator generator = new TextAIGenerator("NeuroAPIKey", "https://ru.neuroapi.host");
List<string> messages = ["Привет!"];
Console.WriteLine(await generator.GenerateText(messages));