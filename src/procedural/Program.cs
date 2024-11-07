using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Drawing;

string choice;

Console.BackgroundColor = ConsoleColor.DarkGray;
Console.WindowWidth = 150;

PrCharacter Tony = new PrCharacter("Тони", "Сальвоне", 25, true);
Tony.CreateByLogicRandomTraits(4);

PrCharacter Pain = new PrCharacter("Пейн", "Фуфил", 25, true);
Pain.CreateByLogicRandomTraits(4);

Tony.GetRelations(Pain);

while (true)
{
    Console.WriteLine("\nВыберите:");
    Console.Write("1 - генерация персонажа, 2 - прочесть персонажа, 3 - настройка генерации персонажа, 4 - редактор персонажа, 5 - экспорт персонажа, 0 - выход: ");
    choice = Console.ReadLine();

    switch (choice)
    {
        case "0":
            return 0;

        case "1":
            {
                Console.WriteLine("Выберите генерацию:");
                Console.Write("1 - С нуля, 2 - С помощью родителей, 3 - С введённых черт, 4 - С шума: ");
                string gen_choice = Console.ReadLine();

                switch (gen_choice)
                {
                    case "1":
                        {
                            break;
                        }

                    case "2":
                        {
                            break;
                        }

                    case "3":
                        {
                            break;
                        }

                    case "4":
                        {
                            Console.WriteLine("WIP");
                            break;
                        }

                    default:
                        Console.WriteLine("\nERR: Неправильный запрос.\n");
                        break;
                }
                break;
            }

        case "2":
            {
                if (GlobalData.Characters.Count != 0)
                {
                    Console.Write("Выберите персонажа по его ID: ");
                    int id_in = Convert.ToInt32(Console.ReadLine());
                    try
                    {
                        GlobalData.Characters[id_in].Write();
                    }
                    catch
                    {
                        Console.WriteLine("Нет персонажа с таким ID!");
                    }
                }
                else
                {
                    Console.WriteLine("\nERR: Нет сгенерированных персонажей.\n");
                }

                break;
            }

        case "3":
            {
                break;
            }

        case "4":
            {
                if (GlobalData.Characters.Count != 0)
                {
                    Console.Write("Выберите персонажа по его ID: ");
                    int id_in = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine(GlobalData.Characters[id_in]);
                }
                else
                {
                    Console.WriteLine("\nERR: Нет сгенерированных персонажей.\n");
                }
                break;
            }

        case "5":
            {
                if (GlobalData.Characters.Count != 0)
                {
                    Console.Write("Выберите персонажа по его ID: ");
                    int id_in = Convert.ToInt32(Console.ReadLine());

                    string fileName = "TEST.json";
                    var jsonString = JsonConvert.SerializeObject(GlobalData.Characters[id_in]);
                    File.WriteAllText(fileName, jsonString);

                    Console.WriteLine(jsonString);

                }
                else
                {
                    Console.WriteLine("\nERR: Нет сгенерированных персонажей.\n");
                }
                break;
            }

        default:
            Console.WriteLine("\nERR: Неправильный запрос.\n");
            break;
    }
}

/*Console.WriteLine("\n");

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

GlobalData.getKinship(Kid, Tony);*/