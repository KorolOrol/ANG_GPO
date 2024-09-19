using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

/*PrCharacter Tom = new PrCharacter("Tom");
Tom.create_full_random(5);
Tom.write_all_traits();*/

/*PrCharacter Bob = new PrCharacter("Bob");
Bob.CreateByLogicRandomTraits(2);
Bob.WriteAllTraits();*/

Console.WriteLine("\n");

PrCharacter Tony = new PrCharacter("Тони", "Сальвоне", 25, true);
Tony.CreateByLogicRandomTraits(4);
Tony.WriteAllTraitsWithAff();

Console.WriteLine("\n");

PrCharacter Ivanna = new PrCharacter("Иванна", "Сальвоне", 23, false);
Ivanna.CreateByLogicRandomTraits(4);
Ivanna.WriteAllTraitsWithAff();

Console.WriteLine("\n");

PrCharacter Kid = new PrCharacter();
Kid.CreateByTwoParentsHalfRandomTraits(6, Ivanna, Tony, "Фрик");
Kid.WriteAllTraitsWithAff();

Kid.GetRelations(Tony);
Kid.GetRelations(Ivanna);

string json = JsonConvert.SerializeObject(Kid, Formatting.Indented);
Console.WriteLine(json);

GlobalData.getKinship(Kid, Tony);

/*PrCharacter Pain = new PrCharacter("Пэйн");
Pain.CreateByLogicRandomTraits(6);
Pain.CreateByChaoticRandomPhobias(1);
Pain.WriteAllTraits();

Console.WriteLine("\n");

Pain.WriteDesc();

Console.WriteLine("\n");

Pain.GetRelations(Bob);

string json = JsonConvert.SerializeObject(Pain, Formatting.Indented);
Console.WriteLine(json);*/