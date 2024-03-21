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
Tony.create_logic_random(6);
Tony.write_all_traits();

Console.WriteLine("\n");

Character Ivanna = new Character("Ivanna");
Ivanna.create_logic_random(6);
Ivanna.write_all_traits();

Console.WriteLine("\n");

Character Kid = new Character();
Kid.create_by_two_parents_random_half(4, Ivanna, Tony, "ShiShi");
Kid.write_all_traits_with_description();