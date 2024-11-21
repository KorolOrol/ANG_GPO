using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Intrinsics.X86;

public class PrCharacter
{

    /// <summary>
    /// Три Слэша - Всему Голова
    /// </summary>

    #region [PrCharacter characteristics]

    public int ID;
    
    public string? Name;
    public string? Surname;
    public int?    Age;
    public bool?   Gender; // 0 - F, 1 - M
    public string? Description;
    public string? Location;

    private const int _MaxPossibleTraits = 10;
    private const int _MaxPossiblePhobias = 3;

    public int ?MotherID;
    public int ?FatherID;

    public Dictionary<int, double> Relations = new Dictionary<int, double>();
    public List<Trait> Traits = new List<Trait>();
    public List<Trait> Phobias = new List<Trait>();

    /*public List<Item> Items = new List<Item>();*/
    /*public List<Event> Events = new List<Event>;*/

    #endregion

    #region [Output]
    
    /// <summary>
    /// Выводит всю информацию о персонаже
    /// </summary>
    public void Write()
    {
        Console.WriteLine($"ID: {ID}");

        Console.WriteLine($"Имя: {Name}");
        Console.WriteLine($"Фамилия: {Surname}");
        Console.WriteLine($"Возраст: {Age}");
        Console.WriteLine($"Пол: {Gender}");
        Console.WriteLine($"Описание: {Description}");
        Console.WriteLine($"Локация: {Location}");

        Console.Write($"Отношения: "); this.WriteRelations();
        Console.Write($"Черты характера: "); this.WriteAllTraits();
        Console.Write($"Фобии: "); this.WriteAllPhobias();

    }

    /// <summary>
    /// Выводит все черты персонажа
    /// </summary>
    public void WriteAllTraits()
    {
        foreach (var trait in Traits)
        {
            Console.Write($"{trait.Title} ");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Выводит все черты персонажа с параметром их влияния 
    /// </summary>
    public void WriteAllTraitsWithAff()
    {
        Console.Write($"{Name} имеет такие черты: ");
        Console.WriteLine("\n");

        foreach (var trait in Traits)
        {
            Console.Write($"{trait.Title} {trait.Affection} ");
            Console.WriteLine("\n");
        }
    }

    /// <summary>
    /// Выводит все черты персонажа с префиксными словами, определяющими уровень влияния черты
    /// </summary>
    public void WriteAllTraitsWithAffDesc()
    {
        Console.Write($"{Name} имеет такие черты: ");

        foreach (var trait in Traits)
        {
            if (trait.Affection <= 0.20)
            {
                Console.Write($"чуток {trait.Title} ");
            }
            else if (trait.Affection <= 0.4)
            {
                Console.Write($"слегка {trait.Title} ");
            }
            else if (trait.Affection <= 0.6)
            {
                Console.Write($"довольно {trait.Title} ");
            }
            else if (trait.Affection <= 0.8)
            {
                Console.Write($"максимально {trait.Title} ");
            }
            else if (trait.Affection <= 1)
            {
                Console.Write($"чрезвычайно {trait.Title} ");
            }
        }
    }

    /// <summary>
    /// Выводит все фобии персонажа
    /// </summary>
    public void WriteAllPhobias()
    {
        foreach (var phobia in Phobias)
        {
            Console.Write($"{phobia.Title} ");
        }
        Console.WriteLine();
    }

    /// <summary>
    /// Выводит все фобии персонажа с параметром их влияния 
    /// </summary>
    public void WriteAllPhobiasWithAff()
    {
        Console.Write($"{Name} имеет такие фобии: ");
        Console.WriteLine("\n");

        foreach (var phobia in Phobias)
        {
            Console.Write($"{phobia.Title} {phobia.Affection} ");
            Console.WriteLine("\n");
        }
    }

    public void WriteRelations()
    {
        foreach (var relation in Relations)
        {
            Console.Write($"{relation}");
        }
        Console.WriteLine();
    }

    #endregion

    #region [Construstors] // TODO: DO

    public PrCharacter()
    {
        this.ID = GlobalData.PrCharactersCreated;

        GlobalData.PrCharactersCreated++;
        GlobalData.Characters.Add(this);
    }

    public PrCharacter(string name)
    {
        this.Name = name;
        this.Surname = "";
        this.ID = GlobalData.PrCharactersCreated;

        GlobalData.PrCharactersCreated++;
        GlobalData.Characters.Add(this);
    }

/*    public PrCharacter(string name, string surname, int age, bool gender)
    {
        this.Name = name;
        this.Surname = surname;
        this.Age = age;
        this.Gender = gender;
        this.ID = GlobalData.PrCharactersCreated;

        GlobalData.PrCharactersCreated++;
        GlobalData.Characters.Add(this);
    }*/

    public PrCharacter(string name, string surname = "", int age = 0, bool gender = false)
    {
        this.ID = GlobalData.PrCharactersCreated;
        this.Name = name;

        if (surname != "")
        {
            this.Surname = surname;
        }
        if (age != 0)
        {
            this.Age = age;
        }
        if (gender != false)
        {
            this.Gender = gender;
        }

        GlobalData.PrCharactersCreated++;
        GlobalData.Characters.Add(this);
    }

    #endregion

    #region [Deprecated methods]

    /// <summary>
    /// Проверяет черту по всем якорям.
    /// </summary>
    /// <param name="id"></param>
    /// <param name="anchor"></param>
    /// <returns></returns>
    public bool CheckTablAnchor(int id, int anchor)
    {
        foreach (Trait a in Traits)
        {

            if (GlobalData.tabl[GlobalData.TraitsList.IndexOf(a), id] == 0 || GlobalData.tabl[GlobalData.TraitsList.IndexOf(a), id] != anchor)
            {
                /*Console.WriteLine("Черта {0} НЕ совместима с чертой {1}", a.title, GlobalData.traits_list[id].title);*/
                return false;
            }

            /*Console.WriteLine("Черта {0} совместима с чертой {1}", a.title, GlobalData.traits_list[id].title);*/
        }

        /*Console.WriteLine("");*/
        return true;
    }

    /// <summary>
    /// Проверяет возможность создания персонажа с введённым якорем.
    /// </summary>
    /// <param name="anchor"></param>
    /// <param name="traits_count"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool CheckAnchorPossibility(int anchor, int traits_count, int id)
    {
        int cnt = 0;

        for (int i = 0; i < GlobalData.TraitsList.Count; i++)
        {
            if (GlobalData.tabl[id, i] == anchor)
            {
                cnt++;
            }
        }

        if (cnt < traits_count)
        {
            Console.WriteLine("Черты были очищены, потому что черта {0} имеет cnt {1}", GlobalData.TraitsList[id].Title, cnt);
            return false;
        }

        Console.WriteLine("");
        return true;
    }


    /// <summary>
    /// Создание персонажа через "якорь". Якорь - число от 0 до 3. Чем якорь больше, тем персонаж будет интереснее (сомнительнее)
    /// </summary>
    /// <param name="traits_count"></param>
    /// <param name="anchor"></param>
    [Obsolete("CreateByAnchorLogic is deprecated, please use CreateByAnchorLogic instead."/*, true*/)]
    public void CreateByAnchorLogic(int traits_count, int anchor)
    {
        if (_MaxPossibleTraits < traits_count)
        {
            Console.WriteLine("Недостаточно черт в базе данных");
        }
        else
        {
            for (int iv = 0; iv < traits_count; iv++)
            {
                var random = new Random().Next(GlobalData.TraitsList.Count);

                if (CheckAnchorPossibility(anchor, traits_count, random) && CheckTablAnchor(random, anchor))
                {
                    Traits.Add(GlobalData.TraitsList[random]);
                }
                else
                {
                    iv--;
                }
            }
        }
    }

    #endregion
}