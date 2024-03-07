using BaseClasses.Model;
using AIGen = AIGenerator.AIGenerator;

string path = "C:\\Users\\KorolOrol\\Desktop\\TUSUR\\repos\\ANG_GPO\\src\\AI\\AIGenerator\\SystemPromptExample.json";
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
            Character character = await gen.GenerateCharacterAsync(characters, locations, items, events);
            characters.Add(character);
            Console.WriteLine(character.FullInfo());
            break;
        case 2:
            Location location = await gen.GenerateLocationAsync(characters, locations, items, events);
            locations.Add(location);
            Console.WriteLine(location.FullInfo());
            break;
        case 3:
            Item item = await gen.GenerateItemAsync(characters, locations, items, events);
            items.Add(item);
            Console.WriteLine(item.FullInfo());
            break;
        case 4:
            Event ev = await gen.GenerateEventAsync(characters, locations, items, events);
            events.Add(ev);
            Console.WriteLine(ev.FullInfo());
            break;
        case 5:
            foreach(var c in characters)
            {
                Console.WriteLine(c.FullInfo());
            }
            foreach(var l in locations)
            {
                Console.WriteLine(l.FullInfo());
            }
            foreach(var i in items)
            {
                Console.WriteLine(i.FullInfo());
            }
            foreach (var e in events)
            {
                Console.WriteLine(e.FullInfo());
            }
            break;
        case 0:
            return;
    }
}