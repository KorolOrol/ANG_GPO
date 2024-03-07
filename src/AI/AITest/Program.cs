using BaseClasses.Model;
using AIGen = AIGenerator.AIGenerator;

string path = "C:\\Users\\KorolOrol\\Desktop\\TUSUR\\repos\\ANG_GPO\\src\\AI\\AITest\\SystemPromptExample.json";
AIGen gen = new AIGen(path);

List<Character> characters = new List<Character>();
List<Location> locations = new List<Location>();
List<Item> items = new List<Item>();
List<Event> events = new List<Event>();

while (true)
{
    int choise = int.Parse(Console.ReadLine());
    switch (choise)
    {
        case 1:
            Character character = await gen.GenerateCharacterAsync();
            characters.Add(character);
            Console.WriteLine(character.FullInfo());
            break;
        case 2:
            Location location = await gen.GenerateLocationAsync();
            locations.Add(location);
            Console.WriteLine(location.FullInfo());
            break;
        case 3:
            Item item = await gen.GenerateItemAsync();
            items.Add(item);
            Console.WriteLine(item.FullInfo());
            break;
        case 4:
            Event ev = await gen.GenerateEventAsync();
            events.Add(ev);
            Console.WriteLine(ev.FullInfo());
            break;
    }
}