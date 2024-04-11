using System;
using System.Diagnostics;
public class Trait
{
    public static Random rand = new Random();

    public string title;
    public float affection = rand.NextSingle();
    public string description;

    public Trait(string title)
    {
        this.title = title;
        this.description = "";
    }

    public Trait(string title, string description)
    {
        this.title = title;
        this.description = description;
    }

    public Trait(string title, List<string> description)
    {
        this.title = title;
        this.description = description[rand.Next(description.Count)];
    }
}