using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

/*Character Tom = new Character("Tom");
Tom.create_full_random(5);
Tom.write_all_traits();*/

/*Character Bob = new Character("Bob");
Bob.create_logic_anchor_random(5, 2);
Bob.write_all_traits();*/

Console.WriteLine("\n");

Character Tony = new Character("Tony");
Tony.CreateByLogicRandom(6);
Tony.WriteAllTraits();

Console.WriteLine("\n");

Character Ivanna = new Character("Ivanna");
Ivanna.CreateByLogicRandom(6);
Ivanna.WriteAllTraits();

Console.WriteLine("\n");

Character Kid = new Character();
Kid.CreateByTwoParentsHalfRandom(8, Ivanna, Tony, "ShiShi");
Kid.WriteAllTraitsWithAff();