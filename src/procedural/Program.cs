using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

/*Character Tom = new Character("Tom");
Tom.create_full_random(5);
Tom.write_all_traits();*/

Character Bob = new Character("Bob");
Bob.CreateByLogicRandomTraits(2);
Bob.WriteAllTraits();

/*Console.WriteLine("\n");

Character Tony = new Character("Тони");
Tony.CreateByLogicRandom(4);
Tony.WriteAllTraitsWithAff();

Console.WriteLine("\n");

Character Ivanna = new Character("Иванна");
Ivanna.CreateByLogicRandom(4);
Ivanna.WriteAllTraitsWithAff();

Console.WriteLine("\n");

Character Kid = new Character();
Kid.CreateByTwoParentsLogicRandom(8, Ivanna, Tony, "ШиШи");
Kid.WriteAllTraitsWithAff();*/

Character Pain = new Character("Пэйн");
Pain.CreateByLogicRandomTraits(2);
Pain.CreateByChaoticRandomPhobias(1);
Pain.WriteAllTraits();

Console.WriteLine("\n");

Pain.WriteDesc();

Console.WriteLine("\n");

Pain.GetRelations(Bob);

string json = JsonConvert.SerializeObject(Pain, Formatting.Indented);
Console.WriteLine(json);