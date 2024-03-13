using BaseClasses.Model;
using AIGen = AIGenerator.AIGenerator;

string path = "C:\\Users\\KorolOrol\\Desktop\\TUSUR\\repos\\ANG_GPO\\src\\AI\\AIGenerator\\SystemPromptExample.json";
AIGen gen = new AIGen(path, "NeuroAPIKey", "https://s2.neuroapi.host");

Plot plot = new Plot();

while (true)
{
    int choise = int.Parse(Console.ReadLine());
    switch (choise)
    {
        case 1:
            Character character = await gen.GenerateCharacterAsync(plot);
            plot.Characters.Add(character);
            Console.WriteLine(character.FullInfo());
            break;
        case 2:
            Location location = await gen.GenerateLocationAsync(plot);
            plot.Locations.Add(location);
            Console.WriteLine(location.FullInfo());
            break;
        case 3:
            Item item = await gen.GenerateItemAsync(plot);
            plot.Items.Add(item);
            Console.WriteLine(item.FullInfo());
            break;
        case 4:
            Event ev = await gen.GenerateEventAsync(plot);
            plot.Events.Add(ev);
            Console.WriteLine(ev.FullInfo());
            break;
        case 5:
            Console.WriteLine(plot.FullInfo());
            break;
        case 0:
            return;
    }
}