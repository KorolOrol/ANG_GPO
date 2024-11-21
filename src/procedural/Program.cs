using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Drawing;
using System.Runtime.CompilerServices;

string choice;

Console.BackgroundColor = ConsoleColor.DarkGray;
Console.WindowWidth = 150;

/*PrCharacter Tony = new PrCharacter("Тони", "Сальвоне", 25, true);
Tony.CreateByLogicRandomTraits(4);

PrCharacter Pain = new PrCharacter("Пейн", "Фуфил", 25, true);
Pain.CreateByLogicRandomTraits(4);

Tony.GetRelations(Pain);*/

while (true)
{
    Console.WriteLine("\nВыберите:");
    Console.Write("1 - генерация персонажа, 2 - прочесть персонажа, 3 - настройка генерации персонажа, 4 - редактор персонажа, 5 - экспорт персонажа, 0 - выход: ");
    choice = Console.ReadLine();

    Console.WriteLine();

    switch (choice)
    {
        case "0": // 0 - выход
            return 0;

        case "1": // 1 - генерация персонажа
            {
                Console.WriteLine("Выберите генерацию:");
                Console.Write("1 - С нуля, 2 - С помощью родителей, 3 - С введённых черт, 4 - С шума: ");
                string gen_choice = Console.ReadLine();

                Console.WriteLine();

                switch (gen_choice)
                {
                    case "1":
                        {
                            Console.WriteLine("Выберите вид генерации с нуля:");
                            Console.Write("1 - Хаотический (черты характеры могут быть несовместимы), 2 - Логический (черты характера будут совместимы друг с другом): ");
                            string from_zero_choice = Console.ReadLine();

                            Console.WriteLine();

                            switch (from_zero_choice)
                            {
                                case "1":
                                    {
                                        Console.WriteLine("Введите: Имя {Фамилия} {Возраст} {Пол} | {} - необязательный параметр");
                                        Console.WriteLine("Возраст - число | Пол - false/true (женский/мужской)");
                                        string[] character = Console.ReadLine().Split();

                                        string temp_name = "";
                                        string temp_surname = "";
                                        int temp_age = 0;
                                        bool temp_gender = false;

                                        if (character.Length > 4)
                                        {
                                            Console.WriteLine("\nERR: Слишком много параметров.\n");
                                        }
                                        else if (character.Length == 4)
                                        {
                                            temp_name = character[0];
                                            temp_surname = character[1];
                                            temp_age = Convert.ToInt32(character[2]);
                                            temp_gender = Convert.ToBoolean(character[3]);
                                        }
                                        else if (character.Length == 3) // TO DO: Более умная проверка, что данные введены те. А не, например, 0 0 Макс 
                                        {
                                            temp_name = character[0];
                                            temp_surname = character[1];
                                            temp_age = Convert.ToInt32(character[2]);
                                        }
                                        else if (character.Length == 2)
                                        {
                                            temp_name = character[0];
                                            temp_surname = character[1];
                                        }
                                        else if (character.Length == 1)
                                        {
                                            temp_name = character[0];
                                        }
                                        else
                                        {
                                            Console.WriteLine("\nERR: Неправильный запрос.\n");
                                        }

                                        PrCharacter prCharacter = new PrCharacter(temp_name, temp_surname, temp_age, temp_gender);
                                        // PrGenerator.CreateByChaoticRandomTraits(prCharacter, 5);

                                        break;
                                    }
                                case "2":
                                    {
                                        Console.WriteLine("Введите Имя {Фамилия} {Возраст} {Пол}. {} - необязательный параметр");
                                        string character = Console.ReadLine();
                                        // CreateByLogicRandomTraits
                                        break;
                                    }

                                case "0": // 0 - выход
                                    return 0;
                                    
                                default:
                                    {
                                        Console.WriteLine("\nERR: Неправильный запрос.\n");
                                        break;
                                    }
                            }
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

                    case "0": // 0 - выход
                        return 0;

                    default:
                        Console.WriteLine("\nERR: Неправильный запрос.\n");
                        break;
                }
                break;
            }

        case "2": // 2 - прочесть персонажа
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

        case "3": // 3 - настройка генерации персонажа
            {
                break;
            }

        case "4": // 4 - редактор персонажа
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

        case "5": // 5 - экспорт персонажа
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