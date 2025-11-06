using System;
using System.Collections.Generic;
public class Trait
{
    private static Random rand = new Random();

    public string Title;
    public double Affection = Math.Round(rand.NextDouble(), 3);
    public string Description;

    public Trait(string title)
    {
        Title = title;
        Description = "";
    }

    public Trait(string title, string description)
    {
        Title = title;
        Description = description;
    }

    public Trait(string title, List<string> description)
    {
        Title = title;
        Description = description[rand.Next(description.Count)];
    }
}