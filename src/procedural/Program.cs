using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

/*PrCharacter Tom = new PrCharacter("Tom");
Tom.create_full_random(5);
Tom.write_all_traits();*/

PrCharacter Bob = new PrCharacter("Bob");
Bob.CreateByLogicRandomTraits(2);
Bob.WriteAllTraits();

/*Console.WriteLine("\n");

PrCharacter Tony = new PrCharacter("Тони");
Tony.CreateByLogicRandom(4);
Tony.WriteAllTraitsWithAff();

Console.WriteLine("\n");

PrCharacter Ivanna = new PrCharacter("Иванна");
Ivanna.CreateByLogicRandom(4);
Ivanna.WriteAllTraitsWithAff();

Console.WriteLine("\n");

PrCharacter Kid = new PrCharacter();
Kid.CreateByTwoParentsLogicRandom(8, Ivanna, Tony, "ШиШи");
Kid.WriteAllTraitsWithAff();*/

PrCharacter Pain = new PrCharacter("Пэйн");
Pain.CreateByLogicRandomTraits(6);
Pain.CreateByChaoticRandomPhobias(1);
Pain.WriteAllTraits();

Console.WriteLine("\n");

Pain.WriteDesc();

Console.WriteLine("\n");

Pain.GetRelations(Bob);

string json = JsonConvert.SerializeObject(Pain, Formatting.Indented);
Console.WriteLine(json);