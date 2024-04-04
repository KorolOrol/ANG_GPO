using BaseClasses.Model;
using AIGenerator.TextGenerator;
using AIGen = AIGenerator.AIGenerator;

string path = "C:\\Users\\KorolOrol\\Desktop\\TUSUR\\repos\\ANG_GPO\\src\\AI\\AIGenerator\\SystemPromptExample.json";
AIGen Ngen = new AIGen(path, new OpenAIGenerator("NeuroAPIKey", "https://lk.neuroapi.host"));
AIGen Vgen = new AIGen(path, new VisionCraftGenerator());
Vgen.TextAIGenerator.Model = "dolphin-2.6-mixtral-8x7b";
AIGen Ogen = new AIGen(path);

AIGen gen = Ngen;

Plot plot = new Plot();

while (true)
{
    int choise = int.Parse(Console.ReadLine());
    switch (choise)
    {
        case 1:
            {
                Character character = await gen.GenerateCharacterAsync(plot);
                Console.WriteLine(character.FullInfo());
                break;
            }
        case 2:
            {
                Location location = await gen.GenerateLocationAsync(plot);
                Console.WriteLine(location.FullInfo());
                break;
            }
        case 3:
            {
                Item item = await gen.GenerateItemAsync(plot);
                Console.WriteLine(item.FullInfo());
                break;
            }
        case 4:
            {
                Event ev = await gen.GenerateEventAsync(plot);
                Console.WriteLine(ev.FullInfo());
                break;
            }
        case 5:
            {
                Character character = await gen.GenerateCharacterChainAsync(plot);
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case 6:
            {
                Location location = await gen.GenerateLocationChainAsync(plot);
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case 7:
            {
                Item item = await gen.GenerateItemChainAsync(plot);
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case 8:
            {
                Event @event = await gen.GenerateEventChainAsync(plot);
                Console.WriteLine(plot.FullInfo());
                break;
            }
        case 9:
            Console.WriteLine(plot.FullInfo());
            break;
        case 0:
            return;
    }
}