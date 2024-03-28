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

Character Tony = new Character("Тони");
Tony.CreateByLogicRandom(1);
Tony.WriteAllTraitsWithAff();

Console.WriteLine("\n");

Character Ivanna = new Character("Иванна");
Ivanna.CreateByLogicRandom(10);
Ivanna.WriteAllTraitsWithAff();

Console.WriteLine("\n");

Character Kid = new Character();
Kid.CreateByTwoParentsLogic(Ivanna, Tony, "ШиШи");
Kid.WriteAllTraitsWithAff();