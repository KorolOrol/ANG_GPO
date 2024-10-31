using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Xml.Linq;
using System.Text.Json;

string choice;
var options = new JsonSerializerOptions { WriteIndented = true };

PrCharacter Tony = new PrCharacter("Тони", "Сальвоне", 25, true);
Tony.CreateByLogicRandomTraits(4);

while (true)
{
    Console.WriteLine("Выберите:");
    Console.WriteLine("1 - генерация персонажа, 2 - настройка генерации персонажа, 3 - редактор персонажа, 4 - экспорт персонажа, 0 - выход");
    choice = Console.ReadLine();

    switch (choice)
    {
        case "0":
            return 0;

        case "1":
            Console.WriteLine("Выберите генерацию:");
            Console.WriteLine("1 - С нуля, 2 - С помощью родителей, 3 - С введённых черт, 4 - С шума");
            string gen_choice = Console.ReadLine();
            break;

        case "2":
            Console.WriteLine("2");
            break;

        case "3":
            {
                if (GlobalData.Characters.Count != 0)
                {
                    Console.WriteLine("Выберите персонажа по его ID");
                    int id_in = Convert.ToInt32(choice);
                    Console.WriteLine(GlobalData.Characters[id_in]);
                }
                else
                {
                    Console.WriteLine("\nERR: Нет сгенерированных персонажей.\n");
                }
                break;
            }

        case "4":
            {
                if (GlobalData.Characters.Count != 0)
                {
                    Console.WriteLine("Выберите персонажа по его ID");
                    int id_in = Convert.ToInt32(Console.ReadLine());

                    string fileName = "TEST.json";
                    string jsonString = JsonSerializer.Serialize(GlobalData.Characters[id_in], options);
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