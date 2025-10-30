using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Xml.Linq;
using BaseClasses.Model;

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
            Console.Write($"{trait.Title} ({trait.Affection}) ");
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

    public PrCharacter(string name, string surname = "", int age = 0, bool? gender = null)
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
        if (gender != null)
        {
            this.Gender = gender;
        }

        GlobalData.PrCharactersCreated++;
        GlobalData.Characters.Add(this);
    }

    #endregion

    #region [Translate To BaseClass]

    public Element Translate(PrCharacter character)
    {
        string baseName = character.Name;
        string description = "";

        if (character.Surname != null) { baseName += " " + character.Surname; }

        if (character.Description != null) { description = character.Description; }

        Dictionary<string, object> baseParams = new Dictionary<string, object>();

        List<string> traits = new List<string>();
        List<string> phobias = new List<string>();

        foreach (var trait in character.Traits)
        {
            traits.Add(trait.Title);
        }

        foreach (var phobia in character.Phobias)
        {
            phobias.Add(phobia.Title);
        }
        baseParams.Add("Черты характера", traits);
        baseParams.Add("Фобии", phobias);

        Element baseCharacter = new Element(ElemType.Character, baseName, description);

        return baseCharacter;
    }

    #endregion
}